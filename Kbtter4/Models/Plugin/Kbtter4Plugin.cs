using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kbtter4.Tenko;
using CoreTweet;
using CoreTweet.Streaming;

namespace Kbtter4.Models.Plugin
{
    public abstract class Kbtter4Plugin : IDisposable
    {
        public abstract string Name { get; }

        public abstract void Initialize();
        public abstract void Dispose();

        public abstract void OnLogin(User user);
        public abstract void OnLogout(User user);
        public abstract void OnStartStreaming();
        public abstract void OnStopStreaming();

        public abstract void OnStatus(StatusMessage mes);
        public abstract void OnEvent(EventMessage mes);
        public abstract void OnDelete(DeleteMessage mes);
        public abstract void OnDirectMessage(DirectMessageMessage mes);
        public abstract StatusMessage OnStatusDestructive(StatusMessage mes);
        public abstract EventMessage OnEventDestructive(EventMessage mes);
        public abstract DeleteMessage OnDeleteDestructive(DeleteMessage mes);
        public abstract DirectMessageMessage OnDirectMessageDestructive(DirectMessageMessage mes);

    }

    public abstract class Kbtter4PluginLoader
    {
        public abstract string Language { get; }

        public abstract IEnumerable<Kbtter4Plugin> Load(Kbtter instance, IList<string> filenames);
    }

    public sealed class Kbtter4PluginProvider
    {
        public Kbtter4PluginProvider(Kbtter ins)
        {
            Instance = ins;
        }

        public Kbtter Instance { get; private set; }

        #region データ保存関係
        public void SaveString(string key, string value)
        {
            Instance.PluginData[key] = value;
        }

        public string LoadString(string key)
        {
            return Instance.PluginData.ContainsKey(key) ? Instance.PluginData[key] : "";
        }
        #endregion


        #region コマンド追加
        public void AddCommand(Kbtter4Command cmd)
        {
            Instance.CommandManager.AddCommand(cmd);
        }

        public Kbtter4Command CreateCommand()
        {
            return new Kbtter4Command();
        }

        public Kbtter4CommandParameter CreateCommandParameter(string name, bool req)
        {
            return new Kbtter4CommandParameter(name, req);
        }

        public Kbtter4Command CreateCommand(string name, string desc, IEnumerable<string> prm, Func<IDictionary<string, object>, string> func)
        {
            var ret = new Kbtter4Command();
            ret.Name = name;
            ret.Description = desc;
            ret.Function = func;
            foreach (var i in prm)
            {
                var cn = i.TrimStart('!');
                var cp = new Kbtter4CommandParameter(cn, i.StartsWith("!"));
            }
            return ret;
        }
        #endregion


        #region メニュー追加

        public void AddStatusMenu(string text, Action<Status> action)
        {
            Instance.StatusMenus.Add(new Kbtter4PluginStatusMenu(text, action, null));
        }

        public void AddStatusMenu(string text, Action<Status> action, Predicate<Status> pred)
        {
            Instance.StatusMenus.Add(new Kbtter4PluginStatusMenu(text, action, pred));
        }

        #endregion


        #region 簡単

        public void UpdateStatus(string text)
        {
            Instance.Token.Statuses.UpdateAsync(status => text);
        }

        public void Reply(string text, long id)
        {
            Instance.Token.Statuses.UpdateAsync(status => text, in_reply_to_status_id => id);
        }

        public void Favorite(long sid)
        {
            Instance.Token.Favorites.CreateAsync(id => sid);
        }

        public void Unfavorite(long sid)
        {
            Instance.Token.Favorites.DestroyAsync(id => sid);
        }

        public void Retweet(long sid)
        {
            Instance.Token.Statuses.RetweetAsync(id => sid);
        }

        public void DeleteStatus(long sid)
        {
            Instance.Token.Statuses.DestroyAsync(id => sid);
        }

        #endregion


        public void Notify(string text)
        {
            Instance.NotifyToView(text);
        }


    }

    public sealed class Kbtter4PluginStatusMenu
    {
        public string Text { get; set; }

        public Action<Status> Action { get; set; }

        public Predicate<Status> Predicate { get; set; }

        public Kbtter4PluginStatusMenu(string t, Action<Status> a, Predicate<Status> p)
        {
            Text = t;
            Action = a;
            Predicate = p != null ? p : (Predicate<Status>)(q => true);
        }
    }

}
