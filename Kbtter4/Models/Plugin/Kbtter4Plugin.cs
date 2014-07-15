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
        public abstract string CommandName { get; }

        public abstract void Initialize();
        public abstract void Dispose();

        public abstract void OnLogin(User user);
        public abstract void OnLogout(User user);
        public abstract void OnStartStreaming();
        public abstract void OnStopStreaming();

        public abstract string OnCommand(IList<string> args);

        public abstract void OnStatus(StatusMessage mes);
        public abstract void OnEvent(EventMessage mes);
        public abstract void OnIdEvent(IdMessage mes);
        public abstract void OnDirectMessage(DirectMessageMessage mes);
        public abstract StatusMessage OnStatusDestructive(StatusMessage mes);
        public abstract EventMessage OnEventDestructive(EventMessage mes);
        public abstract IdMessage OnIdEventDestructive(IdMessage mes);
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

        public void SaveString(string key, string value)
        {
            Instance.PluginData[key] = value;
        }

        public string LoadString(string key)
        {
            return Instance.PluginData.ContainsKey(key) ? Instance.PluginData[key] : "";
        }

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

        public IDictionary<string, object> CreateParameter()
        {
            return new Dictionary<string, object>();
        }

        public void Tweet(IDictionary<string, object> prms)
        {
            Instance.Token.Statuses.UpdateAsync(prms);
        }

    }
}
