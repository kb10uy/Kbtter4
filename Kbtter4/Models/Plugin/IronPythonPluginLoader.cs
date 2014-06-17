using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using CoreTweet;
using CoreTweet.Core;
using CoreTweet.Rest;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;

using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting.Providers;

namespace Kbtter4.Models.Plugin
{
    internal sealed class IronPythonPluginLoader : Kbtter4PluginProvider
    {

        Kbtter4PluginEventProvider ep;

        public override string ProvidingLanguage
        {
            get
            {
                return "IronPython";
            }
        }

        int count = 0;
        public override int ProvidingPluginsCount
        {
            get { return count; }
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
            ep = new Kbtter4PluginEventProvider { Instance = kbtter };
        }

        public override void Release()
        {

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
            var list = filenames.Where(p => p.EndsWith(".py"));
            var err = 0;

            var eng = Python.CreateEngine();
            var context = HostingHelpers.GetLanguageContext(eng) as PythonContext;
            var path = context.GetSearchPaths();
            path.Add(Environment.CurrentDirectory+"\\");
            eng.SetSearchPaths(path);

            eng.Runtime.LoadAssembly(typeof(Status).Assembly);

            var scp = eng.CreateScope();
            scp.SetVariable("Kbtter4", ep);
            foreach (var i in list)
            {
                var src = eng.CreateScriptSourceFromFile(i);
                try
                {
                    src.Execute(scp);
                }
                catch (Exception e)
                {
                    Kbtter.Instance.LogError(string.Format("IronPythonプラグインローダー\n{0}の読み込み中にエラーが発生しました \n{1}", i, e.Message));
                    err++;
                }

            }
            count = list.Count();
            return err;
        }
    }
}
