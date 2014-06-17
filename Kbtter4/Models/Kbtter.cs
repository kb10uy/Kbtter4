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

using Livet;

namespace Kbtter4.Models
{
    /// <summary>
    /// モデル層を定義します。
    /// このクラスは継承できません。
    /// </summary>
    public sealed class Kbtter : NotificationObject
    {

        /// <summary>
        /// CoreTweet Token
        /// </summary>
        public Tokens Token { get; set; }

        internal List<IDisposable> StreamManager { get; set; }
        internal IConnectableObservable<StreamingMessage> Streaming { get; set; }

        /// <summary>
        /// 起動時以降のツイートのキャッシュ
        /// </summary>
        public SynchronizedCollection<Status> Cache { get; set; }

        /// <summary>
        /// ユーザーのキャッシュ
        /// </summary>
        public IDictionary<string, User> UserCache { get; set; }

        
        /// <summary>
        /// 現在の設定
        /// </summary>
        internal Kbtter4Setting Setting { get; set; }
        

        /// <summary>
        /// ツイート受信時のイベント
        /// </summary>
        public event Action<StatusMessage> OnStatus;

        /// <summary>
        /// イベント受信時のイベント
        /// </summary>
        public event Action<EventMessage> OnEvent;

        /// <summary>
        /// ダイレクトメッセージ受信時のイベント
        /// </summary>
        public event Action<DirectMessageMessage> OnDirectMessage;

        /// <summary>
        /// IdEvent受信時のイベント
        /// </summary>
        public event Action<IdMessage> OnIdEvent;

        /// <summary>
        /// 読み込んでいるプラグインを取得します。
        /// </summary>
        public IList<Kbtter4PluginProvider> Plugins { get; private set; }

        /// <summary>
        /// プラグイン同士で非同期的に処理する際にlock構文やMonitorで用いるオブジェクトを取得します。
        /// </summary>
        /// <returns></returns>
        public object PluginMonitoringToken { get; private set; }

        /// <summary>
        /// ログります。
        /// </summary>
        public List<string> Logs { get; set; }

        /// <summary>
        /// エラーを返したプラグイン数を取得します。
        /// </summary>
        public int PluginErrorCount { get; private set; }

        internal StatusMessage LatestStatus { get; set; }
        internal EventMessage LatestEvent { get; set; }
        internal DirectMessageMessage LatestDirectMessage { get; set; }

        internal OAuth.OAuthSession OAuthSession { get; set; }

        internal Queue<Status> ShowingStatuses { get; private set; }

        /// <summary>
        /// 現在認証しているユーザー
        /// </summary>
        public User AuthenticatedUser { get; set; }

        internal static readonly string CacheDatabaseFileNameSuffix = "-cache.db";
        internal static readonly string CacheUserImageFileNameSuffix = "-icon.png";
        internal static readonly string CacheUserBackgroundImageFileNameSuffix = "-background.png";
        internal static readonly string CacheUserProfileFileNameSuffix = "-profile.json";
        internal static readonly string CacheFolderName = "cache";

        internal static readonly string PluginFolderName = "plugin";

        private SQLiteConnection CacheDatabaseConnection { get; set; }
        internal DataContext CacheContext { get; set; }

        internal Kbtter4SystemData SystemData { get; set; }

        private SQLiteCommand AddFavoriteCacheCommand { get; set; }
        private SQLiteCommand AddRetweetCacheCommand { get; set; }
        private SQLiteCommand RemoveFavoriteCacheCommand { get; set; }
        private SQLiteCommand RemoveRetweetCacheCommand { get; set; }
        private SQLiteCommand IsFavoritedCommand { get; set; }
        private SQLiteCommand IsRetweetedCommand { get; set; }
        private SQLiteCommand IsMyRetweetCommand { get; set; }

        #region コンストラクタ・デストラクタ
        private Kbtter()
        {

        }

        /// <summary>
        /// なし
        /// </summary>
        ~Kbtter()
        {
            StopStreaming();
            foreach (var i in Plugins) i.Release();
            if (CacheDatabaseConnection != null) CacheDatabaseConnection.Dispose();
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

        internal async void Initialize()
        {
            ShowingStatuses = new Queue<Status>();
            Setting = new Kbtter4Setting();
            StreamManager = new List<IDisposable>();
            Plugins = new List<Kbtter4PluginProvider>();
            PluginMonitoringToken = new object();
            Logs = new List<string>();

            if (!Directory.Exists(CacheFolderName)) Directory.CreateDirectory(CacheFolderName);
            if (!Directory.Exists(PluginFolderName)) Directory.CreateDirectory(PluginFolderName);

            Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(App.ConfigurationFileName, Setting);
            OAuthSession = await OAuth.AuthorizeAsync(Setting.Consumer.Key, Setting.Consumer.Secret);
            OnStatus += NotifyStatusUpdate;
            OnEvent += NotifyEventUpdate;
            OnIdEvent += NotifyIdEventUpdate;
            OnDirectMessage += NotifyDirectMessageUpdate;

            LogInformation("Model層初期化完了");
            SaveLog();

            RaisePropertyChanged("AccessTokenRequest");
        }


        #region キャッシュ関係
        private Task InitializeUserCache()
        {
            return Task.Run(async () =>
            {
                if (AuthenticatedUser != null)
                {
                    var upc = new UserProfileCache
                    {
                        Name = AuthenticatedUser.Name,
                        ScreenName = AuthenticatedUser.ScreenName,
                        Description = AuthenticatedUser.Description,
                        Location = AuthenticatedUser.Location,
                        Uri = AuthenticatedUser.Url.ToString(),
                        Statuses = AuthenticatedUser.StatusesCount,
                        Friends = AuthenticatedUser.FriendsCount,
                        Followers = AuthenticatedUser.FollowersCount,
                        Favorites = AuthenticatedUser.FavouritesCount,
                    };
                    upc.SaveJson(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserProfileFileNameSuffix);
                    using (var wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(
                            AuthenticatedUser.ProfileImageUrlHttps,
                            CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserImageFileNameSuffix);
                        await wc.DownloadFileTaskAsync(
                            AuthenticatedUser.ProfileBackgroundImageUrlHttps,
                            CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserBackgroundImageFileNameSuffix);
                    }
                }
                else
                {
                    if (!File.Exists(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserProfileFileNameSuffix)) return;
                    var upc = Kbtter4Extension.LoadJson<UserProfileCache>(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserProfileFileNameSuffix);
                    AuthenticatedUser.Name = upc.Name;
                    AuthenticatedUser.ScreenName = upc.ScreenName;
                    AuthenticatedUser.Description = upc.Description;
                    AuthenticatedUser.Location = upc.Location;
                    AuthenticatedUser.Url = new Uri(upc.Uri);
                    AuthenticatedUser.StatusesCount = upc.Statuses;
                    AuthenticatedUser.FriendsCount = upc.Friends;
                    AuthenticatedUser.FollowersCount = upc.Followers;
                    AuthenticatedUser.FavouritesCount = upc.Favorites;
                    if (File.Exists(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserImageFileNameSuffix))
                    {
                        AuthenticatedUser.ProfileImageUrlHttps = new Uri(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserImageFileNameSuffix, UriKind.Relative);
                    }
                    if (File.Exists(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserBackgroundImageFileNameSuffix))
                    {
                        AuthenticatedUser.ProfileImageUrlHttps = new Uri(CacheFolderName + "/" + AuthenticatedUser.ScreenName + CacheUserBackgroundImageFileNameSuffix, UriKind.Relative);
                    }
                }
            });
        }

        private Task InitializeCacheDatabase(int ai)
        {
            return Task.Run(() =>
            {
                var sb = new SQLiteConnectionStringBuilder()
                {
                    DataSource = CacheFolderName + "/" + Setting.AccessTokens[ai].ScreenName + CacheDatabaseFileNameSuffix,
                    Version = 3,
                    SyncMode = SynchronizationModes.Off,
                    JournalMode = SQLiteJournalModeEnum.Memory
                };
                CacheDatabaseConnection = new SQLiteConnection(sb.ToString());
                CacheDatabaseConnection.Open();
                CacheContext = new DataContext(CacheDatabaseConnection);
                CreateTables();
                CacheUnknown();
                LogInformation("キャッシュ接続・作成完了");
                SaveLog();
            });
        }

        private void CreateTables()
        {
            var c = CacheDatabaseConnection.CreateCommand();

            //TODO : よりスタイリッシュな方法の探索

            CacheContext.ExecuteCommand("CREATE TABLE IF NOT EXISTS Favorites(ID UNIQUE,DATE,NAME);");
            CacheContext.ExecuteCommand("CREATE TABLE IF NOT EXISTS Retweets(ID UNIQUE,ORIGINALID,DATE,NAME);");
            CacheContext.ExecuteCommand("CREATE TABLE IF NOT EXISTS Bookmarks(ID UNIQUE,DATE,SCREENNAME,NAME,STATUS);");

            AddFavoriteCacheCommand = CacheDatabaseConnection.CreateCommand();
            AddFavoriteCacheCommand.CommandText = "INSERT OR IGNORE INTO Favorites(ID,DATE,NAME) VALUES(@Id,@Date,@Name)";
            AddFavoriteCacheCommand.Parameters.Add("Id", DbType.Int64);
            AddFavoriteCacheCommand.Parameters.Add("Date", DbType.DateTime);
            AddFavoriteCacheCommand.Parameters.Add("Name", DbType.String);

            AddRetweetCacheCommand = CacheDatabaseConnection.CreateCommand();
            AddRetweetCacheCommand.CommandText = "INSERT OR IGNORE INTO Retweets(ID,ORIGINALID,DATE,NAME) VALUES(@Id,@OriginalId,@Date,@Name)";
            AddRetweetCacheCommand.Parameters.Add("Id", DbType.Int64);
            AddRetweetCacheCommand.Parameters.Add("OriginalId", DbType.Int64);
            AddRetweetCacheCommand.Parameters.Add("Date", DbType.DateTime);
            AddRetweetCacheCommand.Parameters.Add("Name", DbType.String);

            RemoveFavoriteCacheCommand = CacheDatabaseConnection.CreateCommand();
            RemoveFavoriteCacheCommand.CommandText = "DELETE FROM Favorites WHERE ID=@Id";
            RemoveFavoriteCacheCommand.Parameters.Add("Id", DbType.Int64);

            RemoveRetweetCacheCommand = CacheDatabaseConnection.CreateCommand();
            RemoveRetweetCacheCommand.CommandText = "DELETE FROM Retweets WHERE ID=@Id";
            RemoveRetweetCacheCommand.Parameters.Add("Id", DbType.Int64);

            IsFavoritedCommand = CacheDatabaseConnection.CreateCommand();
            IsFavoritedCommand.CommandText = "SELECT * FROM Favorites WHERE ID=@Id";
            IsFavoritedCommand.Parameters.Add("Id", DbType.Int64);

            IsRetweetedCommand = CacheDatabaseConnection.CreateCommand();
            IsRetweetedCommand.CommandText = "SELECT * FROM Retweets WHERE ORIGINALID=@OriginalId";
            IsRetweetedCommand.Parameters.Add("OriginalId", DbType.Int64);

            IsMyRetweetCommand = CacheDatabaseConnection.CreateCommand();
            IsMyRetweetCommand.CommandText = "SELECT * FROM Retweets WHERE ID=@Id";
            IsMyRetweetCommand.Parameters.Add("Id", DbType.Int64);
        }

        private async void CacheUnknown()
        {
            var ls = await Token.Favorites.ListAsync(count => 200);
            foreach (var i in ls) AddFavoriteCache(i);

            //foreach (var i in rs) AddRetweetCache(i);

            CacheContext.SubmitChanges();
        }

        /// <summary>
        /// ツイートをお気に入りのキャッシュに追加します。
        /// </summary>
        /// <param name="st">ツイート</param>
        public void AddFavoriteCache(Status st)
        {
            if (AddFavoriteCacheCommand == null) return;
            AddFavoriteCacheCommand.Parameters["Id"].Value = st.Id;
            AddFavoriteCacheCommand.Parameters["Date"].Value = st.CreatedAt.DateTime;
            AddFavoriteCacheCommand.Parameters["Name"].Value = st.User.ScreenName;
            AddFavoriteCacheCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// ツイートをリツイートのキャッシュに追加します。
        /// </summary>
        /// <param name="st">ツイート。リツイートした元ツイートではなく、リツイート自体を指定しください。</param>
        public void AddRetweetCache(Status st)
        {
            if (AddRetweetCacheCommand == null) return;
            AddRetweetCacheCommand.Parameters["Id"].Value = st.Id;
            AddRetweetCacheCommand.Parameters["OriginalId"].Value = st.RetweetedStatus.Id;
            AddRetweetCacheCommand.Parameters["Date"].Value = st.CreatedAt.DateTime;
            AddRetweetCacheCommand.Parameters["Name"].Value = st.User.ScreenName;
            AddRetweetCacheCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// ツイートをお気に入りのキャッシュから削除します。
        /// </summary>
        /// <param name="st">ツイート</param>
        public void RemoveFavoriteCache(Status st)
        {
            if (RemoveFavoriteCacheCommand == null) return;
            RemoveFavoriteCacheCommand.Parameters["Id"].Value = st.Id;
            RemoveFavoriteCacheCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// ツイートをリツイートのキャッシュから削除します。
        /// </summary>
        /// <param name="st">ツイート。リツイートした元ツイートではなく、リツイート自体を指定しください。</param>
        public void RemoveRetweetCache(Status st)
        {
            if (RemoveRetweetCacheCommand == null) return;
            RemoveRetweetCacheCommand.Parameters["Id"].Value = st.Id;
            RemoveRetweetCacheCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// ツイートをリツイートのキャッシュに追加します。
        /// </summary>
        /// <param name="stid">リツイートのID。リツイートした元ツイートではなく、リツイート自体を指定しください。</param>
        public void RemoveRetweetCache(long stid)
        {
            if (RemoveRetweetCacheCommand == null) return;
            RemoveRetweetCacheCommand.Parameters["Id"].Value = stid;
            RemoveRetweetCacheCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// 指定したツイートがお気に入りとしてキャッシュされているか検証します。
        /// </summary>
        /// <param name="st">ツイート</param>
        /// <returns>存在した場合はtrue</returns>
        public bool IsFavoritedInCache(Status st)
        {
            if (IsFavoritedCommand == null) return false;
            IsFavoritedCommand.Parameters["Id"].Value = st.Id;
            using (var dr = IsFavoritedCommand.ExecuteReader())
            {
                return dr.Read();
            }
        }

        /// <summary>
        /// 指定したリツイートの元ツイートを現在のキャッシュのユーザーがリツイートしているか検証します。
        /// </summary>
        /// <param name="st">ツイート。リツイートした元ツイートではなく、リツイート自体を指定しください。</param>
        /// <returns>存在した場合はtrue</returns>
        public bool IsRetweetedInCache(Status st)
        {
            if (IsRetweetedCommand == null) return false;
            IsRetweetedCommand.Parameters["OriginalId"].Value = st.RetweetedStatus.Id;
            using (var dr = IsRetweetedCommand.ExecuteReader())
            {
                return dr.Read();
            }
        }

        /// <summary>
        /// 指定したリツイートが自分のものか検証します。
        /// </summary>
        /// <param name="stid">リツイートのID。リツイートした元ツイートではなく、リツイート自体を指定しください。</param>
        /// <returns>存在した場合はtrue</returns>
        public bool IsMyRetweetInCache(long stid)
        {
            if (IsMyRetweetCommand == null) return false;
            IsMyRetweetCommand.Parameters["Id"].Value = stid;
            using (var dr = IsMyRetweetCommand.ExecuteReader())
            {
                return dr.Read();
            }
        }

        /// <summary>
        /// C#ﾋﾞｰﾑﾋﾞﾋﾞﾋﾞﾋﾞﾋﾞwwwwww
        /// </summary>
        /// <param name="beam">なまえ</param>
        public void FireBeam(string beam)
        {
            if (Token == null) return;
            if (!Setting.System.BeamCount.ContainsKey(beam)) Setting.System.BeamCount[beam] = 1;
            try
            {
                Token.Statuses.UpdateAsync(status => String.Format("{0}ﾋﾞｰﾑﾋﾞﾋﾞﾋﾞﾋﾞﾋﾞwwwwww({1}回目) #Kbtter4", beam, Setting.System.BeamCount[beam]));
            }
            catch
            {
            }
            Task.Run(() =>
            {
                var p = ++Setting.System.BeamCount[beam];
                Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(App.ConfigurationFileName);
                Setting.System.BeamCount[beam] = p;
                Setting.SaveJson(App.ConfigurationFileName);
            });
        }

        /// <summary>
        /// Javaはクソ。
        /// </summary>
        /// <param name="beam">なまえ</param>
        public void Hate(string beam)
        {
            if (Token == null) return;
            if (!Setting.System.HateCount.ContainsKey(beam)) Setting.System.HateCount[beam] = 1;
            try
            {
                Token.Statuses.UpdateAsync(status => String.Format("{0}はクソ。({1}回目) #Kbtter4", beam, Setting.System.HateCount[beam]));
            }
            catch
            {
            }
            Task.Run(() =>
            {
                var p = ++Setting.System.HateCount[beam];
                Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(App.ConfigurationFileName);
                Setting.System.HateCount[beam] = p;
                Setting.SaveJson(App.ConfigurationFileName);
            });
        }

        /// <summary>
        /// GO is GOD
        /// </summary>
        /// <param name="beam">なまえ</param>
        public void God(string beam)
        {
            if (Token == null) return;
            if (!Setting.System.GodCount.ContainsKey(beam)) Setting.System.GodCount[beam] = 1;
            try
            {
                Token.Statuses.UpdateAsync(status => String.Format("{0} is GOD({1}回目) #Kbtter4", beam, Setting.System.GodCount[beam]));
            }
            catch
            {
            }
            Task.Run(() =>
            {
                var p = ++Setting.System.GodCount[beam];
                Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(App.ConfigurationFileName);
                Setting.System.GodCount[beam] = p;
                Setting.SaveJson(App.ConfigurationFileName);
            });

        }

        /// <summary>
        /// いまからおもしろいこといいます
        /// </summary>
        /// <param name="s">ダジャレ</param>
        public async void SayDajare(string s)
        {
            if (Token == null) return;
            try
            {
                await Token.Statuses.UpdateAsync(status => "いまからおもしろいこといいます");
                await Token.Statuses.UpdateAsync(status => s);
                await Token.Statuses.UpdateAsync(status => "ありがとうございました");
            }
            catch
            {
            }
        }

        /// <summary>
        /// ユーザーを取得してみる
        /// </summary>
        /// <param name="sn">SN</param>
        /// <returns>User</returns>
        public Task<User> GetUser(string sn)
        {
            return Task<User>.Run(async () =>
            {
                if (UserCache.ContainsKey(sn)) return UserCache[sn];
                var us = await Token.Users.ShowAsync(screen_name => sn);
                UserCache[sn] = us;
                return us;
            });
        }

        #endregion

        #region 認証
        /// <summary>
        /// 指定番号のAccessTokenを用いてログインします。
        /// </summary>
        /// <param name="ai">AccessTokenのインデックス</param>
        public async void AuthenticateWith(int ai)
        {
            Token = Tokens.Create(
                Setting.Consumer.Key,
                Setting.Consumer.Secret,
                Setting.AccessTokens[ai].Token,
                Setting.AccessTokens[ai].TokenSecret
                );
            Cache = new SynchronizedCollection<Status>();
            UserCache = new Dictionary<string, User>();
            AuthenticatedUser = await Token.Account.VerifyCredentialsAsync(include_entities => true);
            InitializePlugins();
            await InitializeCacheDatabase(ai);
            await InitializeUserCache();
            UserCache[AuthenticatedUser.ScreenName] = AuthenticatedUser;
            RaisePropertyChanged(() => AuthenticatedUser);
            SaveLog();
        }

        /// <summary>
        /// PINコードを用いて認証します。
        /// </summary>
        /// <param name="pin">PINコード</param>
        /// <returns>成功した場合はTokens</returns>
        public Tokens AuthorizeToken(string pin)
        {
            try
            {
                var t = OAuthSession.GetTokens(pin);
                return t;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Streaming
        /// <summary>
        /// Streamingを開始します。
        /// </summary>
        public void StartStreaming()
        {
            GetDirectMessages();
            RestartStreaming();
        }

        /// <summary>
        /// Streamingを再接続します。
        /// </summary>
        public void RestartStreaming()
        {
            StopStreaming();

            Streaming = Token.Streaming
                .StartObservableStream(
                    StreamingType.User,
                    new StreamingParameters(include_entities => "true", include_followings_activity => "true"))
                .Publish();

            StreamManager.Add(Streaming.Subscribe(
                (p) =>
                {
                    Task.Run(() =>
                    {
                        if (p is StatusMessage) OnStatus((StatusMessage)p);
                        if (p is EventMessage) OnEvent((EventMessage)p);
                        if (p is IdMessage) OnIdEvent((IdMessage)p);
                        if (p is DirectMessageMessage) OnDirectMessage((DirectMessageMessage)p);
                    });
                },
                (Action<Exception>)((ex) =>
                {
                    //throw ex;
                }),
                (Action)(() =>
                {
                    Console.WriteLine("Completed!?");
                    throw new InvalidOperationException("何故かUserStreamが切れました");
                })
            ));

            StreamManager.Add(Streaming.Connect());
        }

        private async void GetDirectMessages()
        {
            var r = (await Token.DirectMessages.ReceivedAsync(count => 200)).ToList();
            r.RemoveAll(p => p.Sender.Id == AuthenticatedUser.Id);
            var s = (await Token.DirectMessages.SentAsync(count => 200)).ToList();
            s.AddRange(r);
            s.Sort((x, y) => DateTime.Compare(x.CreatedAt.LocalDateTime, y.CreatedAt.LocalDateTime));
            foreach (var i in s)
            {
                OnDirectMessage(new DirectMessageMessage { DirectMessage = i });
            }
        }

        private async void NotifyStatusUpdate(StatusMessage msg)
        {
            CacheStatuses(msg);
            if (msg.Type != MessageType.Create) return;
            LatestStatus = await ProcessStatuses(msg);
            ShowingStatuses.Enqueue(LatestStatus.Status);
            RaisePropertyChanged("Status");
            await Task.Run(() =>
            {
                foreach (var p in Plugins) p.StatusUpdate(msg.DeepCopy(), PluginMonitoringToken);
            });
        }

        private async void NotifyEventUpdate(EventMessage msg)
        {
            CacheEvents(msg);
            LatestEvent = await ProcessEvents(msg);
            RaisePropertyChanged("Event");
            await Task.Run(() =>
            {
                foreach (var p in Plugins) p.EventUpdate(msg.DeepCopy(), PluginMonitoringToken);
            });
        }

        private async void NotifyIdEventUpdate(IdMessage msg)
        {
            CacheIdEvents(msg);
            await ProcessIdEvents(msg);
            RaisePropertyChanged("IdEvent");
            await Task.Run(() =>
            {
                foreach (var p in Plugins) p.IdEventUpdate(msg.DeepCopy(), PluginMonitoringToken);
            });
        }

        private async void NotifyDirectMessageUpdate(DirectMessageMessage msg)
        {
            CacheDirectMessage(msg);
            LatestDirectMessage = await ProcessDirectMessages(msg);
            RaisePropertyChanged("DirectMessage");
            await Task.Run(() =>
            {
                foreach (var p in Plugins) p.DirectMessageUpdate(msg.DeepCopy(), PluginMonitoringToken);
            });
        }

        private void CacheEvents(EventMessage msg)
        {
            Task.Run(() =>
            {
                UserCache[msg.Source.ScreenName] = msg.Source;
                UserCache[msg.Target.ScreenName] = msg.Target;
                if (AuthenticatedUser == null) return;
                switch (msg.Event)
                {

                    case EventCode.Favorite:
                        if (msg.Source.Id == AuthenticatedUser.Id)
                        {
                            AddFavoriteCache(msg.TargetStatus);
                            CacheContext.SubmitChanges();
                            AuthenticatedUser = msg.Source;
                            //AuthenticatedUser.FavouritesCount++;
                            RaisePropertyChanged(() => AuthenticatedUser);
                            break;
                        }
                        else
                        {

                        }
                        break;
                    case EventCode.Unfavorite:
                        if (msg.Source.Id == AuthenticatedUser.Id)
                        {
                            RemoveFavoriteCache(msg.TargetStatus);
                            CacheContext.SubmitChanges();
                            AuthenticatedUser = msg.Source;
                            //AuthenticatedUser.FavouritesCount--;
                            RaisePropertyChanged(() => AuthenticatedUser);
                            break;
                        }
                        else
                        {

                        }
                        break;
                }

            });
        }

        private void CacheStatuses(StatusMessage msg)
        {
            Task.Run(() =>
            {

                if (AuthenticatedUser == null) return;
                var mst = msg.Status;
                UserCache[mst.User.ScreenName] = mst.User;
                switch (msg.Type)
                {
                    case MessageType.Create:
                        Cache.Add(msg.Status);
                        if (msg.Status.User.Id == AuthenticatedUser.Id)
                        {
                            AuthenticatedUser = msg.Status.User;
                            //AuthenticatedUser.StatusesCount++;
                            RaisePropertyChanged(() => AuthenticatedUser);
                            if (mst.RetweetedStatus != null)
                            {
                                AddRetweetCache(mst);
                                CacheContext.SubmitChanges();
                            }
                        }
                        break;

                }

            });
        }

        private void CacheDirectMessage(DirectMessageMessage msg)
        {
            Task.Run(() =>
            {

            });
        }

        private void CacheIdEvents(IdMessage msg)
        {
            Task.Run(() =>
            {
                if (AuthenticatedUser == null) return;
                switch (msg.Type)
                {
                    case MessageType.DeleteStatus:
                        if (msg.UserId == AuthenticatedUser.Id)
                        {
                            AuthenticatedUser.StatusesCount--;
                            RaisePropertyChanged(() => AuthenticatedUser);
                            if (IsMyRetweetInCache(msg.UpToStatusId ?? 0))
                            {
                                RemoveRetweetCache(msg.UpToStatusId ?? 0);
                                CacheContext.SubmitChanges();
                            }
                        }
                        break;
                }

            });
        }

        private Task<StatusMessage> ProcessStatuses(StatusMessage msg)
        {
            return Task.Run(() =>
            {
                var ret = msg.DeepCopy();
                if (ret == null) return msg;
                foreach (var i in Plugins)
                {
                    ret = i.StatusUpdateDestructive(ret, PluginMonitoringToken) ?? ret;
                }
                return ret;
            });
        }

        private Task<EventMessage> ProcessEvents(EventMessage msg)
        {
            return Task.Run(() =>
            {
                var ret = msg.DeepCopy();
                if (ret == null) return msg;
                foreach (var i in Plugins) ret = i.EventUpdateDestructive(ret, PluginMonitoringToken) ?? ret;
                return ret;
            });
        }

        private Task<IdMessage> ProcessIdEvents(IdMessage msg)
        {
            return Task.Run(() =>
            {
                var ret = msg.DeepCopy();
                if (ret == null) return msg;
                foreach (var i in Plugins) ret = i.IdEventUpdateDestructive(ret, PluginMonitoringToken) ?? ret;
                return ret;
            });
        }

        private Task<DirectMessageMessage> ProcessDirectMessages(DirectMessageMessage msg)
        {
            return Task.Run(() =>
            {
                var ret = msg.DeepCopy();
                if (ret == null) return msg;
                foreach (var i in Plugins) ret = i.DirectMessageUpdateDestructive(ret, PluginMonitoringToken) ?? ret;
                return ret;
            });
        }

        /// <summary>
        /// Streamingを停止します。
        /// </summary>
        public void StopStreaming()
        {
            StreamManager.ForEach(p => p.Dispose());
            StreamManager.Clear();
        }

        #endregion

        #region プラグイン読み込みとか

        internal void InitializePlugins()
        {
            Task.Run(() =>
            {
                var asmtypes = GetType().Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(Kbtter4PluginProvider)));
                foreach (var i in asmtypes)
                {
                    Plugins.Add(Activator.CreateInstance(i) as Kbtter4PluginProvider);
                }

                foreach (var p in Plugins) p.Initialize(this);

                var pflist = Directory.GetFiles(PluginFolderName);
                PluginErrorCount = 0;
                foreach (var p in Plugins) PluginErrorCount += p.Load(pflist);
                SaveLog();
                if (PluginErrorCount != 0) RaisePropertyChanged("PluginErrorCount");
                foreach (var p in Plugins) p.PluginInitialze();
            });
        }

        #endregion

        #region コンフィグ用メソッド

        /// <summary>
        /// 指定したTokensを使用して、AccessTokenを作成し、追加します。
        /// </summary>
        /// <param name="t">Tokens</param>
        public void AddToken(Tokens t)
        {
            Setting.AccessTokens.Add(new AccessToken
            {
                ScreenName = t.ScreenName,
                Token = t.AccessToken,
                TokenSecret = t.AccessTokenSecret,
                ConsumerVerifyHash = Setting.Consumer.GetHash()
            });
            Setting.SaveJson(App.ConfigurationFileName);
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
                File.WriteAllLines(App.LoggingFileName, Logs);
            }
        }
        #endregion
    }

    #region json保存用クラスとか




    /// <summary>
    /// ユーザー情報のキャッシュを定義します。
    /// </summary>
    public class UserProfileCache
    {
        /// <summary>
        /// ユーザー名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// スクリーンネーム
        /// </summary>
        public string ScreenName { get; set; }

        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 場所
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// ツイート数
        /// </summary>
        public int Statuses { get; set; }

        /// <summary>
        /// フォロー数
        /// </summary>
        public int Friends { get; set; }

        /// <summary>
        /// フォロワー数
        /// </summary>
        public int Followers { get; set; }

        /// <summary>
        /// お気に入り数
        /// </summary>
        public int Favorites { get; set; }
    }

    #endregion
}
