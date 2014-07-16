using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Net;
using System.Threading.Tasks;

using CoreTweet;
using CoreTweet.Core;
using CoreTweet.Rest;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Kbtter4.Models.Plugin;
using Kbtter4.Tenko;
using Kbtter4.Ayaya;
using Kbtter4.Cache;
using Kbtter3.Query;


using Livet;

namespace Kbtter4.Models
{
    /// <summary>
    /// モデル層を定義します。
    /// このクラスは継承できません。
    /// </summary>
    public sealed class Kbtter : NotificationObject
    {
        public static readonly string ConfigurationFolderName = "config";
        public static readonly string CacheFolderName = "cache";
        public static readonly string PluginFolderName = "plugin";
        public static readonly string TegakiFolderName = "tegaki";

        public static readonly string LoggingFileName = "Kbtter4.log";
        public static readonly string ConfigurationFileName = ConfigurationFolderName + "/config.json";
        public static readonly string PluginLocalDataFileName = ConfigurationFolderName + "/plugindata.json";

        private static readonly string CacheDatabaseFileNameSuffix = "-cache.db";
        //private static readonly string CacheUserImageFileNameSuffix = "-icon.png";
        //private static readonly string CacheUserBackgroundImageFileNameSuffix = "-background.png";
        //private static readonly string CacheUserProfileFileNameSuffix = "-profile.json";


        public Tokens Token { get; set; }
        private OAuth.OAuthSession OAuthSession { get; set; }

        private List<IDisposable> StreamManager { get; set; }
        private IConnectableObservable<StreamingMessage> Streaming { get; set; }

        public StatusTimeline HomeStatusTimeline { get; private set; }
        public NotificationTimeline HomeNotificationTimeline { get; private set; }
        public ObservableSynchronizedCollection<StatusTimeline> StatusTimelines { get; private set; }
        public ObservableSynchronizedCollection<NotificationTimeline> NotificationTimelines { get; private set; }
        public ObservableSynchronizedCollection<DirectMessageTimeline> DirectMessageTimelines { get; private set; }
        public ObservableSynchronizedCollection<User> Users { get; private set; }
        public ObservableSynchronizedCollection<Kbtter4Account> Accounts { get; private set; }

        public ObservableSynchronizedCollection<User> SearchResultUsers { get; private set; }
        public ObservableSynchronizedCollection<Status> SearchResultStatuses { get; private set; }

        public Kbtter4Cache AuthenticatedUserCache { get; private set; }

        #region AuthenticatedUser変更通知プロパティ
        private User _AuthenticatedUser;

        public User AuthenticatedUser
        {
            get
            { return _AuthenticatedUser; }
            private set
            {
                if (_AuthenticatedUser == value)
                    return;
                _AuthenticatedUser = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public Kbtter4Setting Setting { get; set; }

        public ObservableSynchronizedCollection<Kbtter4Plugin> GlobalPlugins { get; private set; }
        public IList<Kbtter4PluginLoader> PluginLoaders { get; private set; }
        public IDictionary<string, string> PluginData { get; private set; }

        public Kbtter4CommandManager CommandManager { get; private set; }

        public object PluginMonitoringToken { get; private set; }

        public Kbtter3Query GlobalMuteQuery { get; private set; }

        public List<string> Logs { get; set; }

        #region コンストラクタ・デストラクタ
        private Kbtter()
        {

            StreamManager = new List<IDisposable>();
            Users = new ObservableSynchronizedCollection<User>();
            Accounts = new ObservableSynchronizedCollection<Kbtter4Account>();
            LoadSetting();
            HomeStatusTimeline = new StatusTimeline(Setting, "true");
            HomeNotificationTimeline = new NotificationTimeline(Setting, "true");
            DirectMessageTimelines = new ObservableSynchronizedCollection<DirectMessageTimeline>();
            StatusTimelines = new ObservableSynchronizedCollection<StatusTimeline>();
            NotificationTimelines = new ObservableSynchronizedCollection<NotificationTimeline>();

            SearchResultStatuses = new ObservableSynchronizedCollection<Status>();
            SearchResultUsers = new ObservableSynchronizedCollection<User>();

            AuthenticatedUser = new User();

            GlobalPlugins = new ObservableSynchronizedCollection<Kbtter4Plugin>();
            PluginLoaders = new List<Kbtter4PluginLoader>();
            PluginData = new Dictionary<string, string>();
            PluginMonitoringToken = new object();

            GlobalMuteQuery = new Kbtter3Query("false");

            CommandManager = new Kbtter4CommandManager();

            Logs = new List<string>();
        }

        /// <summary>
        /// なし
        /// </summary>
        ~Kbtter()
        {
            StopStreaming();
            Parallel.ForEach(GlobalPlugins, p => p.Dispose());
            SaveLog();
            Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(ConfigurationFileName);
            SaveSetting();
        }
        #endregion

        #region シングルトン
        static Kbtter _instance;

        /// <summary>
        /// Kbtterの唯一のインスタンスを取得します。
        /// </summary>
        public static Kbtter Instance
        {
            get
            {
                if (_instance == null) _instance = new Kbtter();
                return _instance;
            }
        }
        #endregion

        public void Initialize()
        {

            CreateFolders();
            InitializePlugins();
            RegisterCommands();
        }


        private void CreateFolders()
        {
            if (!Directory.Exists(PluginFolderName)) Directory.CreateDirectory(PluginFolderName);
            if (!Directory.Exists(CacheFolderName)) Directory.CreateDirectory(CacheFolderName);
            if (!Directory.Exists(TegakiFolderName)) Directory.CreateDirectory(TegakiFolderName);
        }

        private void LoadSetting()
        {
            if (!Directory.Exists(ConfigurationFolderName)) Directory.CreateDirectory(ConfigurationFolderName);
            Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(ConfigurationFileName);
            PluginData = Kbtter4Extension.LoadJson<Dictionary<string, string>>(PluginLocalDataFileName);
            foreach (var i in Setting.Accounts) Accounts.Add(i);
        }

        public void SaveSetting()
        {
            UpdateUserTimelineData();
            Setting.SaveJson(ConfigurationFileName);
            PluginData.SaveJson(PluginLocalDataFileName);
        }

        public void UpdateUserTimelineData()
        {
            var ac = Setting.Accounts.FirstOrDefault(p => p.UserId == AuthenticatedUser.Id);
            if (ac == null) return;
            ac.Timelines.Clear();
            foreach (var i in StatusTimelines)
            {
                ac.Timelines.Add(new Kbtter4SettingStatusTimelineData { Name = i.Name, Query = i.Query.QueryText });
            }
        }

        #region Streaming接続
        public void StartStreaming()
        {

            Streaming = Token.Streaming.StartObservableStream(
                StreamingType.User,
                new StreamingParameters(include_entities => "true", include_followings_activity => "true"))
                .Publish();

            StreamManager.Add(Streaming.Subscribe(
                (p) =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        switch (p.Type)
                        {
                            case MessageType.Create:
                                Kbtter_OnStatus(this, new Kbtter4MessageReceivedEventArgs<StatusMessage>(p as StatusMessage));
                                break;
                            case MessageType.Event:
                                Kbtter_OnEvent(this, new Kbtter4MessageReceivedEventArgs<EventMessage>(p as EventMessage));
                                break;
                            case MessageType.DirectMesssage:
                                Kbtter_OnDirectMessage(this, new Kbtter4MessageReceivedEventArgs<DirectMessageMessage>(p as DirectMessageMessage));
                                break;
                            case MessageType.DeleteStatus:
                            case MessageType.DeleteDirectMessage:
                                Kbtter_OnId(this, new Kbtter4MessageReceivedEventArgs<IdMessage>(p as IdMessage));
                                break;
                            case MessageType.Disconnect:
                                LogInformation("Disconnected");
                                SaveLog();
                                break;
                            default:
                                break;

                        }
                    }, TaskCreationOptions.PreferFairness)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null && !t.IsCanceled) RestartStreaming();
                    });
                },
                (ex) =>
                {
                    LogInformation("エラーが発生したため再接続しました : " + ex.Message);
                    SaveLog();
                    RestartStreaming();
                },
                () =>
                {
                    LogInformation("UserStreamが切断されたため再接続しました");
                    SaveLog();
                    RestartStreaming();
                }
            ));
            StreamManager.Add(Streaming.Connect());
            Parallel.ForEach(GlobalPlugins, p => p.OnStartStreaming());
        }

        public void RestartStreaming()
        {
            StopStreaming();
            StartStreaming();
        }

        public void StopStreaming()
        {
            foreach (var i in StreamManager) i.Dispose();
            StreamManager.Clear();
            Parallel.ForEach(GlobalPlugins, p => p.OnStopStreaming());
        }
        #endregion

        #region Streamingイベントハンドラ
        private void Kbtter_OnStatus(object sender, Kbtter4MessageReceivedEventArgs<StatusMessage> e)
        {

            GlobalMuteQuery.ClearVariables();
            GlobalMuteQuery.SetVariable("Status", e.Message.Status);
            if (GlobalMuteQuery.Execute().AsBoolean()) return;

            var s = e.Message;

            if (s.Status.RetweetedStatus != null)
            {
                //自分がRTした
                if (s.Status.User.Id == AuthenticatedUser.Id)
                {
                    AuthenticatedUserCache.AddRetweet(
                        new Kbtter4RetweetCache
                        {
                            CreatedDate = s.Status.RetweetedStatus.CreatedAt.LocalDateTime,
                            Id = s.Status.Id,
                            OriginalId = s.Status.RetweetedStatus.Id,
                            ScreenName = s.Status.RetweetedStatus.User.ScreenName
                        });
                    RaisePropertyChanged("Retweets");
                }
                //自分のがRTされたかRTがRTされた
                if (s.Status.RetweetedStatus.User.Id == AuthenticatedUser.Id ||
                    s.Status.Text.StartsWith("RT @" + AuthenticatedUser.ScreenName + ":"))
                {
                    UpdateHeadline("リツイートされました : " + s.Status.RetweetedStatus.Text.TrimLineFeeds());
                    HomeNotificationTimeline.TryAddNotification(new Kbtter4Notification(s));
                }
            }
            var u = s.DeepCopy();
            if (u != null) UpdateUserInformation(u.Status.User);

            foreach (var i in GlobalPlugins) s = i.OnStatusDestructive(s.DeepCopy()) ?? s;

            HomeStatusTimeline.TryAddStatus(s.Status);
            foreach (var tl in StatusTimelines)
            {
                tl.TryAddStatus(s.Status);
            }
            RaisePropertyChanged("Statuses");
            if (s.Status.Entities.UserMentions.Any(p => p.Id == AuthenticatedUser.Id) && s.Status.RetweetedStatus == null)
            {
                UpdateHeadline("メンション : " + s.Status.Text.TrimLineFeeds());
            }

            Parallel.ForEach(GlobalPlugins, p => p.OnStatus(s.DeepCopy()));
        }

        private void Kbtter_OnEvent(object sender, Kbtter4MessageReceivedEventArgs<EventMessage> e)
        {
            var s = e.Message;

            if (s.Source.Id == AuthenticatedUser.Id)
            {
                switch (s.Event)
                {
                    case EventCode.Favorite:
                        AuthenticatedUserCache.AddFavorite(
                            new Kbtter4FavoriteCache
                            {
                                Id = s.TargetStatus.Id,
                                ScreenName = s.TargetStatus.User.ScreenName,
                                CreatedDate = s.TargetStatus.CreatedAt.LocalDateTime
                            });
                        RaisePropertyChanged("Favorites");
                        break;
                    case EventCode.Unfavorite:
                        AuthenticatedUserCache.RemoveFavorite(s.TargetStatus.Id);
                        RaisePropertyChanged("Favorites");
                        break;
                }
            }
            switch (s.Event)
            {
                case EventCode.UserUpdate:
                    UpdateUserInformation(s.Source);
                    break;
                default:
                    UpdateUserInformation(s.Source);
                    UpdateUserInformation(s.Target);
                    break;
            }



            foreach (var i in GlobalPlugins) s = i.OnEventDestructive(s.DeepCopy()) ?? s;

            if (s.Source.Id != AuthenticatedUser.Id && s.Target.Id == AuthenticatedUser.Id)
            {
                switch (s.Event)
                {
                    case EventCode.Favorite:
                        UpdateHeadline("ふぁぼられました : " + s.TargetStatus.Text.TrimLineFeeds());
                        break;
                }

                var n = new Kbtter4Notification(s);
                HomeNotificationTimeline.TryAddNotification(n);
                foreach (var tl in NotificationTimelines)
                {
                    tl.TryAddNotification(n);
                }
            }
            Parallel.ForEach(GlobalPlugins, p => p.OnEvent(s.DeepCopy()));
        }

        private void Kbtter_OnDirectMessage(object sender, Kbtter4MessageReceivedEventArgs<DirectMessageMessage> e)
        {
            var dm = e.Message;
            foreach (var i in GlobalPlugins) dm = i.OnDirectMessageDestructive(dm.DeepCopy()) ?? dm;

            UpdateUserInformation(dm.DirectMessage.Sender);
            UpdateUserInformation(dm.DirectMessage.Recipient);

            var pu = dm.DirectMessage.Sender.Id == AuthenticatedUser.Id ? dm.DirectMessage.Recipient : dm.DirectMessage.Sender;
            if (DirectMessageTimelines.All(p => p.Party.Id != pu.Id))
            {
                DirectMessageTimelines.Add(new DirectMessageTimeline(Setting, pu));
            }
            DirectMessageTimelines.First(p => p.Party.Id == pu.Id).TryAddDirectMessage(dm.DirectMessage);

            Parallel.ForEach(GlobalPlugins, p => p.OnDirectMessage(dm.DeepCopy()));
        }

        private void Kbtter_OnId(object sender, Kbtter4MessageReceivedEventArgs<IdMessage> e)
        {
            var mes = e.Message;
            foreach (var i in GlobalPlugins) mes = i.OnIdEventDestructive(mes.DeepCopy()) ?? mes;

            switch (mes.Type)
            {
                case MessageType.DeleteStatus:
                    if (AuthenticatedUserCache.Retweets().Where(p => p.Id == mes.Id).Count() != 0)
                    {
                        AuthenticatedUserCache.RemoveRetweet(mes.Id ?? 0);
                        RaisePropertyChanged("Retweets");
                    }

                    var tt = HomeStatusTimeline.Statuses.FirstOrDefault(p => p.Id == mes.Id);
                    if (tt != null) HomeStatusTimeline.Statuses.Remove(tt);
                    foreach (var i in StatusTimelines)
                    {
                        tt = i.Statuses.FirstOrDefault(p => p.Id == mes.Id);
                        if (tt != null) i.Statuses.Remove(tt);
                    }
                    break;
                case MessageType.DeleteDirectMessage:
                    foreach (var i in DirectMessageTimelines)
                    {
                        var dvm = i.DirectMessages.FirstOrDefault(p => p.Id == mes.Id);
                        if (dvm != null) i.DirectMessages.Remove(dvm);
                    }
                    break;
                default:
                    break;
            }

            Parallel.ForEach(GlobalPlugins, p => p.OnIdEvent(mes.DeepCopy()));
        }
        #endregion

        #region 認証
        public async Task<OAuth.OAuthSession> CreateOAuthSession()
        {
            return await OAuth.AuthorizeAsync(Setting.Consumer.Key, Setting.Consumer.Secret);
        }

        public async Task<bool> RegisterAccount(OAuth.OAuthSession session, string pin)
        {
            try
            {
                var t = await OAuth.GetTokensAsync(session, pin);
                var ac = new Kbtter4Account();
                ac.AccessToken = t.AccessToken;
                ac.AccessTokenSecret = t.AccessTokenSecret;
                ac.ScreenName = t.ScreenName;
                ac.UserId = t.UserId;
                ac.Timelines = new ObservableSynchronizedCollection<Kbtter4SettingStatusTimelineData>();
                ac.Timelines.Add(new Kbtter4SettingStatusTimelineData { Name = "リプライ・メンション", Query = "Status.Text match /@" + ac.ScreenName + "/" });
                Accounts.Add(ac);

                Setting.Accounts.Add(ac);
                SaveSetting();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void RemoveAccount(Kbtter4Account ac)
        {
            Setting.Accounts.Remove(ac);
            Accounts.Remove(ac);
            SaveSetting();
        }

        public Task<string> Authenticate(Kbtter4Account ac)
        {
            return Task<string>.Run(async () =>
            {
                StopStreaming();
                ClearStock();
                Parallel.ForEach(GlobalPlugins, p => p.OnLogout(AuthenticatedUser));

                Token = Tokens.Create(Setting.Consumer.Key, Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);
                try
                {
                    var u = await Token.Users.ShowAsync(user_id => ac.UserId);
                    AuthenticatedUser = u;
                    AuthenticatedUserCache = new Kbtter4Cache(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheDatabaseFileNameSuffix);
                    Parallel.ForEach(GlobalPlugins, p => p.OnLogin(AuthenticatedUser));
                    InitializeUserCaches();
                    InitializeDirectMessages();
                    InitializeUserDefinitionTimelines();
                    if (Setting.Timelines.AllowHomeStatusTimelineInitialReading) InitializeHomeStatusTimeline();
                    StartStreaming();
                    return "";
                }
                catch (TwitterException e)
                {
                    return e.Message;
                }
            });

        }
        #endregion

        #region キャッシュ
        object userop = new object();

        private void UpdateUserInformation(User user)
        {
            lock (userop)
            {
                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].Id == user.Id) Users[i] = user;
                }
                if (AuthenticatedUser.Id == user.Id) AuthenticatedUser = user;
            }
        }

        public void AddUserToUsersList(User user)
        {
            lock (userop)
            {
                if (Users.Any(p => p.Id == user.Id)) return;
                Users.Add(user);
            }
        }
        #endregion

        #region 固有機能
        public void ClearStock()
        {
            HomeStatusTimeline.Statuses.Clear();
            HomeNotificationTimeline.Notifications.Clear();
            foreach (var i in DirectMessageTimelines) i.DirectMessages.Clear();
            foreach (var i in StatusTimelines) i.Statuses.Clear();
            foreach (var i in NotificationTimelines) i.Notifications.Clear();
        }

        public async void InitializeDirectMessages()
        {
            try
            {
                var rdms = await Token.DirectMessages.ReceivedAsync(count => 200);
                var sdms = await Token.DirectMessages.SentAsync(count => 200);
                var adms = rdms.ToList();
                adms.AddRange(sdms);
                adms.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));
                adms.ForEach(p => Kbtter_OnDirectMessage(this, new Kbtter4MessageReceivedEventArgs<DirectMessageMessage>(new DirectMessageMessage { DirectMessage = p })));
            }
            catch (TwitterException e)
            {
                LogInformation("ログイン時にDMリストを取得できませんでした : " + e.Message);
                SaveLog();
            }

        }

        public async void InitializeUserCaches()
        {
            try
            {
                var favs = await Token.Favorites.ListAsync(count => 200);
                AuthenticatedUserCache.AddFavorite(favs.Select(p => new Kbtter4FavoriteCache { Id = p.Id, ScreenName = p.User.ScreenName, CreatedDate = p.CreatedAt.LocalDateTime }));
            }
            catch (TwitterException e)
            {
                LogInformation("ログイン時にお気に入りを取得できませんでした : " + e.Message);
                SaveLog();
            }
        }

        private async void InitializeHomeStatusTimeline()
        {
            var tws = await Token.Statuses.HomeTimelineAsync(count => Setting.Timelines.HomeStatusTimelineInitialRead);

            foreach (var i in tws.Reverse())
            {
                Kbtter_OnStatus(this, new Kbtter4MessageReceivedEventArgs<StatusMessage>(new StatusMessage { Status = i }));
            }
        }

        private void InitializeUserDefinitionTimelines()
        {
            StatusTimelines.Clear();
            foreach (var i in Setting.Accounts.First(p => p.UserId == AuthenticatedUser.Id).Timelines)
            {
                StatusTimelines.Add(new StatusTimeline(Setting, i.Query, i.Name));
            }
        }

        public bool CheckFavorited(long id)
        {
            return AuthenticatedUserCache.Favorites().Where(p => p.Id == id).Count() != 0;
        }

        public bool CheckRetweeted(long id)
        {
            return AuthenticatedUserCache.Retweets().Where(p => p.OriginalId == id).Count() != 0;
        }

        public void AddFavorite(Status t)
        {
            AuthenticatedUserCache.AddFavorite(new Kbtter4FavoriteCache { Id = t.Id, CreatedDate = t.CreatedAt.LocalDateTime, ScreenName = t.User.ScreenName });
        }

        public void RemoveFavorite(long id)
        {
            AuthenticatedUserCache.RemoveFavorite(id);
        }

        public void Search(string text)
        {
            Task.Run(async () =>
            {
                try
                {
                    SearchResultStatuses.Clear();
                    var rst = await Token.Search.TweetsAsync(q => text, count => Setting.Searching.StatusCount);
                    foreach (var i in rst) SearchResultStatuses.Add(i);
                }
                catch (TwitterException e)
                {
                    NotifyToView("ツイートを検索出来ませんでした : " + e.Message);
                }

                try
                {
                    SearchResultUsers.Clear();
                    var rst = await Token.Users.SearchAsync(q => text, count => Setting.Searching.UserCount);
                    foreach (var i in rst) SearchResultUsers.Add(i);
                }
                catch (TwitterException e)
                {
                    NotifyToView("ユーザーを検索出来ませんでした : " + e.Message);
                }
            });
        }

        #region Model通知機能


        #region NotifyText変更通知プロパティ
        private string _NotifyText;

        public string NotifyText
        {
            get
            { return _NotifyText; }
            set
            {
                _NotifyText = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        private void NotifyToView(string text)
        {
            NotifyText = text;
        }


        #region HeadlineText変更通知プロパティ
        private string _HeadlineText;

        public string HeadlineText
        {
            get
            { return _HeadlineText; }
            set
            {
                if (_HeadlineText == value)
                    return;
                _HeadlineText = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        private void UpdateHeadline(string text)
        {
            HeadlineText = DateTime.Now.ToShortTimeString() + " " + text;
        }

        #endregion


        #endregion

        #region プラグイン

        private void InitializePlugins()
        {
            Task.Run(() =>
            {
                var asmtypes = GetType().Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(Kbtter4PluginLoader)));
                foreach (var i in asmtypes)
                {
                    PluginLoaders.Add(Activator.CreateInstance(i) as Kbtter4PluginLoader);
                }

                var pflist = Directory.GetFiles(PluginFolderName);

                foreach (var pl in PluginLoaders)
                {
                    foreach (var p in pl.Load(this, pflist))
                    {
                        GlobalPlugins.Add(p);
                    }
                }
                SaveLog();
                Parallel.ForEach(GlobalPlugins, p => p.Initialize());
            });
        }

        #endregion

        #region ログ
        /// <summary>
        /// エラーをログに登録します。
        /// </summary>
        /// <param name="err">テキスト。これの前に[ERROR]が付加されます。</param>
        public void LogError(string err)
        {
            Logs.Add(DateTime.Now.ToString() + " [ERROR]" + Environment.NewLine + err + Environment.NewLine);

        }

        /// <summary>
        /// 情報をログに登録します。
        /// </summary>
        /// <param name="info">テキスト。これの前に[INFO]が付加されます。</param>
        public void LogInformation(string info)
        {
            Logs.Add(DateTime.Now.ToString() + " [INFORMATION] " + Environment.NewLine + info + Environment.NewLine);
        }

        /// <summary>
        /// 警告をログに登録します。
        /// </summary>
        /// <param name="warn">テキスト。これの前に[WARN]が付加されます。</param>
        public void LogWarning(string warn)
        {
            Logs.Add(DateTime.Now.ToString() + " [WARNING] " + Environment.NewLine + warn + Environment.NewLine);
        }

        /// <summary>
        /// 現在のログを保存します。
        /// </summary>
        public void SaveLog()
        {
            lock (PluginMonitoringToken)
            {
                File.WriteAllLines(LoggingFileName, Logs);
            }
        }
        #endregion

        #region コマンド
        //private string CommandHoge(IDictionary<string, object> args)

        private void RegisterCommands()
        {
            CommandManager.AddCommand(new Kbtter4Command
            {
                Name = "update",
                Description = "ツイートします。",
                Function = CommandUpdate,
                Parameters = new[] {
                    new Kbtter4CommandParameter{Name="text",IsRequired=true},
                }
            });
            CommandManager.AddCommand(new Kbtter4Command
            {
                Name = "efb",
                Description = "(非推奨コマンド)userパラメータで指定したSNのユーザーの最新ツイートをふぁぼります。\ncountパラメータで件数を指定できます。",
                Function = CommandEternalForceBlizzard,
                Parameters = new[] { 
                    new Kbtter4CommandParameter{Name="user",IsRequired=true},
                    new Kbtter4CommandParameter{Name="count"},
                }
            });
            CommandManager.AddCommand(new Kbtter4Command
            {
                Name = "louise",
                Description = "????????",
                Function = CommandLouise,
            });
            CommandManager.AddCommand(new Kbtter4Command
            {
                Name = "shindanmaker",
                Description = "診断メーカーの結果を直接取得します。idパラメータに取得したい診断のIDを指定してください。\nnameパラメータに文字列を指定すると、その名前で診断します(ない場合はログイン中のユーザーの名前)。",
                AsynchronousFunction = CommandShindanMakerDirectly,
                IsAsync = true,
                Parameters = new[] { 
                    new Kbtter4CommandParameter("id",true),
                    new Kbtter4CommandParameter("name",false),
                    new Kbtter4CommandParameter("tweet",false)
                }
            });
        }

        private string CommandUpdate(IDictionary<string, object> args)
        {
            if (AuthenticatedUser == null) return "ログインしてください";
            if (args["text"] as string == "") return "テキストを入力してください";
            Token.Statuses.UpdateAsync(status => args["text"]);
            return "投稿しました";
        }

        private string CommandLouise(IDictionary<string, object> args)
        {
            return "ルイズ！ルイズ！ルイズ！ルイズぅぅうううわぁああああああああああああああああああああああん！！！\n" +
                    "あぁああああ…ああ…あっあっー！あぁああああああ！！！ルイズルイズルイズぅううぁわぁああああ！！！\n" +
                    "あぁクンカクンカ！クンカクンカ！スーハースーハー！スーハースーハー！いい匂いだなぁ…くんくん\n" +
                    "んはぁっ！ルイズ・フランソワーズたんの桃色ブロンドの髪をクンカクンカしたいお！クンカクンカ！あぁあ！！\n" +
                    "間違えた！モフモフしたいお！モフモフ！モフモフ！髪髪モフモフ！カリカリモフモフ…きゅんきゅんきゅい！！\n" +
                    "小説12巻のルイズたんかわいかったよぅ！！あぁぁああ…あああ…あっあぁああああ！！ふぁぁあああんんっ！！\n" +
                    "アニメ2期放送されて良かったねルイズたん！あぁあああああ！かわいい！ルイズたん！かわいい！あっああぁああ！\n" +
                    "コミック2巻も発売されて嬉し…いやぁああああああ！！！にゃああああああああん！！ぎゃああああああああ！！\n" +
                    "ぐあああああああああああ！！！コミックなんて現実じゃない！！！！あ…小説もアニメもよく考えたら…\n" +
                    "ル イ ズ ち ゃ ん は 現実 じ ゃ な い？にゃあああああああああああああん！！うぁああああああああああ！！\n" +
                    "そんなぁああああああ！！いやぁぁぁあああああああああ！！はぁああああああん！！ハルケギニアぁああああ！！\n" +
                    "この！ちきしょー！やめてやる！！現実なんかやめ…て…え！？見…てる？表紙絵のルイズちゃんが僕を見てる？\n" +
                    "表紙絵のルイズちゃんが僕を見てるぞ！ルイズちゃんが僕を見てるぞ！挿絵のルイズちゃんが僕を見てるぞ！！\n" +
                    "アニメのルイズちゃんが僕に話しかけてるぞ！！！よかった…世の中まだまだ捨てたモンじゃないんだねっ！\n" +
                    "いやっほぉおおおおおおお！！！僕にはルイズちゃんがいる！！やったよケティ！！ひとりでできるもん！！！\n" +
                    "あ、コミックのルイズちゃああああああああああああああん！！いやぁあああああああああああああああ！！！！\n" +
                    "あっあんああっああんあアン様ぁあ！！シ、シエスター！！アンリエッタぁああああああ！！！タバサｧぁあああ！！\n" +
                    "ううっうぅうう！！俺の想いよルイズへ届け！！ハルゲニアのルイズへ届け";
        }

        public string CommandEternalForceBlizzard(IDictionary<string, object> args)
        {
            if (AuthenticatedUser == null) return "ログインしてください";
            int c = 100;
            var un = args["user"] as string;
            if (args.ContainsKey("count")) c = (int)args["count"];
            Task.Run(() =>
            {
                var tl = Token.Statuses.UserTimeline(screen_name => un, count => c);
                Parallel.ForEach<Status>(tl, p =>
                {
                    try
                    {
                        Token.Favorites.Create(id => p.Id);
                    }
                    catch { }
                });
            });

            return un + "さんの最新ツイート" + c.ToString() + "件をエターナルフォースブリザードしました";
        }

        private async Task<string> CommandShindanMakerDirectly(IDictionary<string, object> args)
        {
            if (AuthenticatedUser == null) return "ログインしてください";
            string name;
            if (args.ContainsKey("name"))
            {
                name = args["name"] as string;
            }
            else
            {
                name = AuthenticatedUser.Name;
            }
            var id = (int)args["id"];
            var ret = await Kbtter4ShindanMakerCooperator.DiagnoseAsync(id, name);
            if (ret == null) return "取得できませんでした";
            if (args.ContainsKey("tweet") && (bool)args["tweet"])
            {
                try
                {
                    await Token.Statuses.UpdateAsync(status => ret + " shindanmaker.com/" + id.ToString());
                }
                catch (TwitterException e)
                {
                    return ret + "\n\nツイートは出来ませんでした : " + e.Message;
                }
                ret += "\nツイートできました";
            }
            return ret;
        }
        #endregion
    }



    public class Kbtter4MessageReceivedEventArgs<T> : EventArgs
    {
        public T Message { get; set; }

        public Kbtter4MessageReceivedEventArgs(T obj)
        {
            Message = obj;
        }
    }
}
