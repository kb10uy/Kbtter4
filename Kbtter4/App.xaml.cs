using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;

using Livet;

namespace Kbtter4
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static readonly string RemoteUpdateInformationFileAddress = "http://github.com/kb10uy/Kbtter4/raw/master/updateinfo.txt";

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            try
            {
                if (CheckUpdate())
                {

                    DownloadExtractUpdateFile(File.ReadAllLines("update.txt")[2]);
                    Current.Shutdown();

                }
            }
            catch
            {
                return;
            }
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            //TODO:ロギング処理など
            MessageBox.Show(
                "不明なエラーが発生しました。アプリケーションを終了します。\n@kb10uyにException.txtを送ると、修正されるかもしれません。",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            var ki = Kbtter4.Models.Kbtter.Instance;
            ex.SaveJson("Excepion.txt");
            Environment.Exit(1);
        }

        #region アップデータ

        public bool CheckUpdate()
        {
            var nowv = Convert.ToInt32(File.ReadAllLines("update.txt")[0]);
            if (!GetUpdateInformationFile()) return false;
            var newv = Convert.ToInt32(File.ReadAllLines("update.txt")[0]);
            return newv > nowv;
        }

        public bool GetUpdateInformationFile()
        {
            using (var wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(RemoteUpdateInformationFileAddress, "update.txt");
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public void DownloadExtractUpdateFile(string ad)
        {
            using (var wc = new WebClient())
            {
                try
                {
                    if (Directory.Exists("update")) Directory.Delete("update", true);
                    wc.DownloadFile(ad, "update.zip");
                    ZipFile.ExtractToDirectory("update.zip", "update");
                    Process.Start("update/Kbtter4.Updater.exe");
                }
                catch
                {
                    return;
                }
            }
        }

        #endregion

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
}
