using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet.Streaming;

using CoreTweet;
using NLua;

namespace Kbtter4.Models.Plugin
{
    internal class LuaPluginLoader : Kbtter4PluginProvider
    {
        Kbtter4PluginHandlerRegisterableEventProvider ep;
        Kbtter kbtter;
        List<Lua> luas;

        public override string ProvidingLanguage
        {
            get { return "Lua"; }
        }

        public override int ProvidingPluginsCount
        {
            get { return luas.Count; }
        }

        public override void StatusUpdate(StatusMessage msg, object mon)
        {
            ep.RaiseStatus(msg);
        }

        public override void DirectMessageUpdate(DirectMessageMessage msg, object mon)
        {
            ep.RaiseDirectMessage(msg);
        }

        public override void EventUpdate(EventMessage msg, object mon)
        {
            ep.RaiseEvent(msg);
        }

        public override void IdEventUpdate(IdMessage msg, object mon)
        {
            ep.RaiseIdEvent(msg);
        }

        public override StatusMessage StatusUpdateDestructive(StatusMessage msg, object mon)
        {
            return ep.RaiseStatusDestructive(msg);
        }

        public override EventMessage EventUpdateDestructive(EventMessage msg, object mon)
        {
            return ep.RaiseEventDestructive(msg);
        }

        public override IdMessage IdEventUpdateDestructive(IdMessage msg, object mon)
        {
            return ep.RaiseIdEventDestructive(msg);
        }

        public override DirectMessageMessage DirectMessageUpdateDestructive(DirectMessageMessage msg, object mon)
        {
            return ep.RaiseDirectMessageDestructive(msg);
        }

        public override void Initialize(Kbtter kbtter)
        {
            this.kbtter = kbtter;
            luas = new List<Lua>();
            ep = new Kbtter4PluginHandlerRegisterableEventProvider { Instance = kbtter };
        }


        public override void Release()
        {
            luas.ForEach(p => p.Dispose());
        }

        public override void SystemRequest(string msg, object mon)
        {

        }

        public override void PluginInitialze()
        {
            ep.RaiseInitialize();
        }

        public override int Load(IList<string> filenames)
        {
            var err = 0;
            var list = filenames.Where(p => p.EndsWith(".lua"));
            foreach (var i in list)
            {
                var l = new Lua();
                l.LoadCLRPackage();
                l["Kbtter4"] = ep;
                try
                {
                    luas.Add(l);
                    l.DoFile(i);
                }
                catch (Exception e)
                {
                    kbtter.LogError(string.Format("Luaプラグインローダー\n{0}の読み込み中にエラーが発生しました \n{1}", i, e.Message));
                    err++;
                }
            }
            return err;
        }
    }
}
