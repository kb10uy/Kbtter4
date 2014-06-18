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
        public ObservableSynchronizedCollection<DirectMessageTimeline> DirectMessageTimelines { get; private set; }
        public ObservableSynchronizedCollection<Kbtter4User> Users { get; private set; }
        public ObservableSynchronizedCollection<Kbtter4Account> Accounts { get; private set; }

        public Kbtter4User AuthenticatedUser { get; private set; }

        private event Kbtter4StatusReceivedEventHandler OnStatus;
        private event Kbtter4EventReceivedEventHandler OnEvent;
        private event Kbtter4DirectMessageReceivedEventHandler OnDirectMessage;
        private event Kbtter4IdReceivedEventHandler OnId;

        public Kbtter4Setting Setting { get; set; }

        public IList<Kbtter4PluginProvider> GlobalPlugins { get; private set; }
        public int PluginErrorCount { get; private set; }
        public object PluginMonitoringToken { get; private set; }

        public Kbtter3Query GlobalMuteQuery { get; private set; }

        public List<string> Logs { get; set; }

        private SQLiteConnection CacheDatabaseConnection { get; set; }
        private DataContext CacheContext { get; set; }
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
            StreamManager = new List<IDisposable>();

            HomeStatusTimeline = new StatusTimeline("true");
            HomeNotificationTimeline = new NotificationTimeline("true");
            DirectMessageTimelines = new ObservableSynchronizedCollection<DirectMessageTimeline>();
            StatusTimelines = new ObservableSynchronizedCollection<StatusTimeline>();
            NotificationTimelines = new ObservableSynchronizedCollection<NotificationTimeline>();
            Users = new ObservableSynchronizedCollection<Kbtter4User>();
            Accounts = new ObservableSynchronizedCollection<Kbtter4Account>();

            AuthenticatedUser = new Kbtter4User();

            GlobalPlugins = new List<Kbtter4PluginProvider>();

            GlobalMuteQuery = new Kbtter3Query("false");

            Logs = new List<string>();
        }

        /// <summary>
        /// なし
        /// </summary>
        ~Kbtter()
        {
            StopStreaming();
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
            OnStatus += Kbtter_OnStatus;
            OnEvent += Kbtter_OnEvent;
            OnDirectMessage += Kbtter_OnDirectMessage;
            OnId += Kbtter_OnId;

            LoadSetting();
        }

        private void LoadSetting()
        {
            if (!Directory.Exists(ConfigurationFolderName)) Directory.CreateDirectory(ConfigurationFolderName);
            Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(ConfigurationFileName);
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

            StreamManager.Add(Streaming.OfType<StatusMessage>().Subscribe(p => OnStatus(this, new Kbtter4MessageReceivedEventArgs<StatusMessage>(p))));
            StreamManager.Add(Streaming.OfType<EventMessage>().Subscribe(p => OnEvent(this, new Kbtter4MessageReceivedEventArgs<EventMessage>(p))));
            StreamManager.Add(Streaming.OfType<DirectMessageMessage>().Subscribe(p => OnDirectMessage(this, new Kbtter4MessageReceivedEventArgs<DirectMessageMessage>(p))));
            StreamManager.Add(Streaming.OfType<IdMessage>().Subscribe(p => OnId(this, new Kbtter4MessageReceivedEventArgs<IdMessage>(p))));

            StreamManager.Add(Streaming.Connect());
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
        }
        #endregion

        #region Streamingイベントハンドラ
        private void Kbtter_OnStatus(object sender, Kbtter4MessageReceivedEventArgs<StatusMessage> e)
        {

        }

        private void Kbtter_OnEvent(object sender, Kbtter4MessageReceivedEventArgs<EventMessage> e)
        {

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

        public async Task RegisterAccount(OAuth.OAuthSession session, string pin)
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
        }

        public async void Authenticate(Kbtter4Account ac)
        {
            Token = Tokens.Create(Setting.Consumer.Key, Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);

            try
            {
                var u = await Token.Users.ShowAsync(user_id => ac.UserId);
                AuthenticatedUser = new Kbtter4User(u);
            }
            catch (TwitterException)
            {

            }
        }
        #endregion

        #region キャッシュ

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

        internal void InitializePlugins()
        {
            Task.Run(() =>
            {
                var asmtypes = GetType().Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(Kbtter4PluginProvider)));
                foreach (var i in asmtypes)
                {
                    GlobalPlugins.Add(Activator.CreateInstance(i) as Kbtter4PluginProvider);
                }

                foreach (var p in GlobalPlugins) p.Initialize(this);

                var pflist = Directory.GetFiles(PluginFolderName);
                PluginErrorCount = 0;
                foreach (var p in GlobalPlugins) PluginErrorCount += p.Load(pflist);
                SaveLog();
                if (PluginErrorCount != 0) RaisePropertyChanged("PluginErrorCount");
                foreach (var p in GlobalPlugins) p.PluginInitialze();
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
