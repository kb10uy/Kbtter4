using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

using System.Data.SQLite;
using System.Data.SQLite.Linq;
using System.Data.Linq;
using System.Data;

using Kbtter4.Models.Caching;
using Kbtter4.Models.Plugin;
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
        private static readonly string CacheFolderName = "cache";
        private static readonly string PluginFolderName = "plugin";

        public static readonly string LoggingFileName = "Kbtter3.log";
        public static readonly string ConfigurationFileName = ConfigurationFolderName + "/config.json";

        private static readonly string CacheDatabaseFileNameSuffix = "-cache.db";
        private static readonly string CacheUserImageFileNameSuffix = "-icon.png";
        private static readonly string CacheUserBackgroundImageFileNameSuffix = "-background.png";
        private static readonly string CacheUserProfileFileNameSuffix = "-profile.json";


        public Tokens Token { get; set; }
        private OAuth.OAuthSession OAuthSession { get; set; }

        private List<IDisposable> StreamManager { get; set; }
        private IConnectableObservable<StreamingMessage> Streaming { get; set; }

        public StatusTimeline HomeStatusTimeline { get; private set; }
        public NotificationTimeline HomeNotificationTimeline { get; private set; }
        public ObservableSynchronizedCollection<StatusTimeline> StatusTimelines { get; private set; }
        public ObservableSynchronizedCollection<NotificationTimeline> NotificationTimelines { get; private set; }
        //public ObservableSynchronizedCollection<DirectMessageTimeline> DirectMessageTimelines { get; private set; }
        public ObservableSynchronizedCollection<User> Users { get; private set; }
        public ObservableSynchronizedCollection<Kbtter4Account> Accounts { get; private set; }


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

        private event Kbtter4StatusReceivedEventHandler OnStatus;
        private event Kbtter4EventReceivedEventHandler OnEvent;
        private event Kbtter4DirectMessageReceivedEventHandler OnDirectMessage;
        private event Kbtter4IdReceivedEventHandler OnId;

        public Kbtter4Setting Setting { get; set; }

        public IList<Kbtter4Plugin> GlobalPlugins { get; private set; }
        public IList<Kbtter4PluginLoader> PluginLoaders { get; private set; }

        public object PluginMonitoringToken { get; private set; }

        public Kbtter3Query GlobalMuteQuery { get; private set; }

        public List<string> Logs { get; set; }

        #region コンストラクタ・デストラクタ
        private Kbtter()
        {
            StreamManager = new List<IDisposable>();

            HomeStatusTimeline = new StatusTimeline("true");
            HomeNotificationTimeline = new NotificationTimeline("true");
            //DirectMessageTimelines = new ObservableSynchronizedCollection<DirectMessageTimeline>();
            StatusTimelines = new ObservableSynchronizedCollection<StatusTimeline>();
            NotificationTimelines = new ObservableSynchronizedCollection<NotificationTimeline>();
            Users = new ObservableSynchronizedCollection<User>();
            Accounts = new ObservableSynchronizedCollection<Kbtter4Account>();

            AuthenticatedUser = new User();

            GlobalPlugins = new List<Kbtter4Plugin>();
            PluginLoaders = new List<Kbtter4PluginLoader>();
            PluginMonitoringToken = new object();

            GlobalMuteQuery = new Kbtter3Query("false");

            Logs = new List<string>();
        }

        /// <summary>
        /// なし
        /// </summary>
        ~Kbtter()
        {
            StopStreaming();
            foreach (var i in GlobalPlugins) i.Dispose();
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
            OnStatus += Kbtter_OnStatus;
            OnEvent += Kbtter_OnEvent;
            OnDirectMessage += Kbtter_OnDirectMessage;
            OnId += Kbtter_OnId;

            CreateFolders();
            LoadSetting();
            InitializePlugins();
        }


        private void CreateFolders()
        {
            if (!Directory.Exists(PluginFolderName)) Directory.CreateDirectory(PluginFolderName);
        }

        private void LoadSetting()
        {
            if (!Directory.Exists(ConfigurationFolderName)) Directory.CreateDirectory(ConfigurationFolderName);
            Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(ConfigurationFileName);
            foreach (var i in Setting.Accounts) Accounts.Add(i);
        }

        public void SaveSetting()
        {
            Setting.SaveJson(ConfigurationFileName);
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
                    Task.Run(() =>
                    {
                        if (p is StatusMessage) OnStatus(this, new Kbtter4MessageReceivedEventArgs<StatusMessage>(p as StatusMessage));
                        if (p is EventMessage) OnEvent(this, new Kbtter4MessageReceivedEventArgs<EventMessage>(p as EventMessage));
                        if (p is IdMessage) OnId(this, new Kbtter4MessageReceivedEventArgs<IdMessage>(p as IdMessage));
                        if (p is DirectMessageMessage) OnDirectMessage(this, new Kbtter4MessageReceivedEventArgs<DirectMessageMessage>(p as DirectMessageMessage));
                    });
                },
                (ex) =>
                {
                    //throw ex;
                },
                () =>
                {
                    Console.WriteLine("Completed!?");
                    throw new InvalidOperationException("何故かUserStreamが切れました");
                }
            ));
            StreamManager.Add(Streaming.Connect());
            foreach (var i in GlobalPlugins) i.OnStartStreaming();
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
            foreach (var i in GlobalPlugins) i.OnStopStreaming();
        }
        #endregion

        #region Streamingイベントハンドラ
        private void Kbtter_OnStatus(object sender, Kbtter4MessageReceivedEventArgs<StatusMessage> e)
        {

            GlobalMuteQuery.ClearVariables();
            GlobalMuteQuery.SetVariable("Status", e.Message.Status);
            if (GlobalMuteQuery.Execute().AsBoolean()) return;

            var s = e.Message;
            foreach (var i in GlobalPlugins) s = i.OnStatusDestructive(s.DeepCopy());

            HomeStatusTimeline.TryAddStatus(s.Status);
            foreach (var tl in StatusTimelines)
            {
                tl.TryAddStatus(s.Status);
            }

            foreach (var i in GlobalPlugins) i.OnStatus(s.DeepCopy());
        }

        private void Kbtter_OnEvent(object sender, Kbtter4MessageReceivedEventArgs<EventMessage> e)
        {
            var s = e.Message;
            foreach (var i in GlobalPlugins) s = i.OnEventDestructive(s.DeepCopy());
            var k4n = new Kbtter4Notification(s);

            HomeNotificationTimeline.TryAddNotification(k4n);
            foreach (var tl in NotificationTimelines)
            {
                tl.TryAddNotification(k4n);
            }

            foreach (var i in GlobalPlugins) i.OnEvent(s.DeepCopy());
        }

        private void Kbtter_OnDirectMessage(object sender, Kbtter4MessageReceivedEventArgs<DirectMessageMessage> e)
        {

        }

        private void Kbtter_OnId(object sender, Kbtter4MessageReceivedEventArgs<IdMessage> e)
        {

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

        public async Task<string> Authenticate(Kbtter4Account ac)
        {
            StopStreaming();
            foreach (var i in GlobalPlugins) i.OnLogout(AuthenticatedUser);

            Token = Tokens.Create(Setting.Consumer.Key, Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);
            try
            {
                var u = await Token.Users.ShowAsync(user_id => ac.UserId);
                AuthenticatedUser = u;
                foreach (var i in GlobalPlugins) i.OnLogin(AuthenticatedUser);
                StartStreaming();
                return "";
            }
            catch (TwitterException e)
            {
                return e.Message;
            }
        }
        #endregion

        #region キャッシュ
        private void UpdateUserInformation(User user)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].Id == user.Id) Users[i] = user;
            }
            if (AuthenticatedUser.Id == user.Id) AuthenticatedUser = user;
        }

        private void AddUserToUsersList(User user)
        {
            if (Users.Any(p => p.Id == user.Id)) return;
            Users.Add(user);
        }
        #endregion

        #region 固有機能
        public void ClearStock()
        {
            foreach (var i in StatusTimelines) i.Statuses.Clear();
            foreach (var i in NotificationTimelines) i.Notifications.Clear();
        }

        public async void InitializeDirectMessages()
        {

        }
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
                foreach (var p in GlobalPlugins) p.Initialize();
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
    }

    public class Kbtter4MessageReceivedEventArgs<T> : EventArgs
    {
        public T Message { get; set; }

        public Kbtter4MessageReceivedEventArgs(T obj)
        {
            Message = obj;
        }
    }

    public delegate void Kbtter4StatusReceivedEventHandler(object sender, Kbtter4MessageReceivedEventArgs<StatusMessage> e);
    public delegate void Kbtter4EventReceivedEventHandler(object sender, Kbtter4MessageReceivedEventArgs<EventMessage> e);
    public delegate void Kbtter4IdReceivedEventHandler(object sender, Kbtter4MessageReceivedEventArgs<IdMessage> e);
    public delegate void Kbtter4DirectMessageReceivedEventHandler(object sender, Kbtter4MessageReceivedEventArgs<DirectMessageMessage> e);
}
