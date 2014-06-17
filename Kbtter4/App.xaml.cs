using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.IO;

using Livet;

namespace Kbtter4
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// コンフィグファイルが保存されているフォルダの名前を取得します。
        /// </summary>
        public static readonly string ConfigurationFolderName = "config";

        public static readonly string LoggingFileName = "Kbtter3.log";

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        public static readonly string ConfigurationFileName = ConfigurationFolderName + "/config.json";

        internal readonly string ConsumerDefaultKey = "5bI3XiTNEMHiamjMV5Acnqkex";
        internal readonly string ConsumerDefaultSecret = "ni2jGjwKTLcdpp1x6nr3yFo9bRrSWRdZfYbzEAZLhKz4uDDErN";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            if (!Directory.Exists(ConfigurationFolderName)) Directory.CreateDirectory(ConfigurationFolderName);

            var Setting = new Kbtter4Setting();
            Setting.Consumer = new ConsumerToken { Key = ConsumerDefaultKey, Secret = ConsumerDefaultSecret };
            Setting = Kbtter4Extension.LoadJson<Kbtter4Setting>(App.ConfigurationFileName, Setting);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        //集約エラーハンドラ
        //private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    //TODO:ロギング処理など
        //    MessageBox.Show(
        //        "不明なエラーが発生しました。アプリケーションを終了します。",
        //        "エラー",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Error);
        //
        //    Environment.Exit(1);
        //}


    }
    internal static class Kbtter4Extension
    {
        public static T LoadJson<T>(string filename)
            where T : new()
        {
            if (!File.Exists(filename))
            {
                var o = new T();
                File.WriteAllText(filename, JsonConvert.SerializeObject(o, Formatting.Indented));
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
        }

        public static T LoadJson<T>(string filename, T def)
        {
            if (!File.Exists(filename))
            {
                File.WriteAllText(filename, JsonConvert.SerializeObject(def, Formatting.Indented));
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
        }

        public static void SaveJson<T>(this T obj, string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        public static bool EndsWith(this string t, string es)
        {
            return t.IndexOf(es) == (t.Length - es.Length);
        }

        public static T CloneViaJson<T>(T obj)
            where T : class
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj)) as T;
        }

        //http://d.hatena.ne.jp/hilapon/20120301/1330569751
        public static T DeepCopy<T>(this T source) where T : class
        {
            T result;
            try
            {
                var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                using (var mem = new System.IO.MemoryStream())
                {
                    serializer.WriteObject(mem, source);
                    mem.Position = 0;
                    result = serializer.ReadObject(mem) as T;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }
    }

    /// <summary>
    /// TwitterにOAuthでログインする際に必要なAccessTokenを
    /// 定義します。
    /// </summary>
    public class AccessToken
    {
        /// <summary>
        /// スクリーンネーム
        /// </summary>
        public string ScreenName { get; set; }

        /// <summary>
        /// Access Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Access Token Secret
        /// </summary>
        public string TokenSecret { get; set; }

        /// <summary>
        /// このAccessTokenが作成された時のConsumerTokenのHash。
        /// </summary>
        public string ConsumerVerifyHash { get; set; }
    }

    /// <summary>
    /// TwitterにOAuthでログインする際に必要なConsumerKey/Secretの組を
    /// 定義します。
    /// </summary>
    public class ConsumerToken : NotificationObject
    {

        #region Key変更通知プロパティ
        private string _Key;

        /// <summary>
        /// Consumer Key
        /// </summary>
        public string Key
        {
            get
            { return _Key; }
            set
            {
                if (_Key == value)
                    return;
                _Key = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Secret変更通知プロパティ
        private string _Secret;

        /// <summary>
        /// Consumer Secret
        /// </summary>
        /// <returns></returns>
        public string Secret
        {
            get
            { return _Secret; }
            set
            {
                if (_Secret == value)
                    return;
                _Secret = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// 使うな
        /// </summary>
        /// <returns>なし</returns>
        public string GetHash()
        {
            return new String((Key + Secret).Where((p, i) => i % 7 == 0 || i % 5 == 0).ToArray());
        }
    }

    internal class Kbtter4SystemData
    {
        public long LastFavoritedStatusId { get; set; }
        public long LastRetweetedStatusId { get; set; }
        public IDictionary<string, int> BeamCount { get; set; }
        public IDictionary<string, int> HateCount { get; set; }
        public IDictionary<string, int> GodCount { get; set; }

        public Kbtter4SystemData()
        {
            LastFavoritedStatusId = 0;
            LastRetweetedStatusId = 0;
            BeamCount = new Dictionary<string, int>();
            HateCount = new Dictionary<string, int>();
            GodCount = new Dictionary<string, int>();
        }
    }

    internal class UserProfilePageSetting : NotificationObject
    {

        #region StatusesShowCount変更通知プロパティ
        private int _StatusesShowCount;

        public int StatusesShowCount
        {
            get
            { return _StatusesShowCount; }
            set
            {
                if (_StatusesShowCount == value)
                    return;
                _StatusesShowCount = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public UserProfilePageSetting()
        {
            StatusesShowCount = 20;
        }
    }

    internal class Kbtter4Setting : NotificationObject
    {

        #region System変更通知プロパティ
        private Kbtter4SystemData _System;

        public Kbtter4SystemData System
        {
            get
            { return _System; }
            set
            {
                if (_System == value)
                    return;
                _System = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AccessTokens変更通知プロパティ
        private ObservableCollection<AccessToken> _AccessTokens;

        public ObservableCollection<AccessToken> AccessTokens
        {
            get
            { return _AccessTokens; }
            set
            {
                if (_AccessTokens == value)
                    return;
                _AccessTokens = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Consumer変更通知プロパティ
        private ConsumerToken _Consumer;

        public ConsumerToken Consumer
        {
            get
            { return _Consumer; }
            set
            {
                if (_Consumer == value)
                    return;
                _Consumer = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public Kbtter4Setting()
        {
            System = new Kbtter4SystemData();
            AccessTokens = new ObservableCollection<AccessToken>();
            Consumer = new ConsumerToken();
        }
    }
}
