﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet.Streaming;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting.Providers;


using CoreTweet;

namespace Kbtter4.Models.Plugin
{
    class Kbtter4IronPythonPlugin : Kbtter4Plugin
    {
        ScriptScope scope;
        public Kbtter4IronPythonPlugin(ScriptScope code)
        {
            this.scope = code;
        }

        public override string Name
        {
            get
            {
                string ret;
                var t = scope.TryGetVariable("Name", out ret);
                return t ? ret : "(No Name)";
            }
        }

        string cn = null;
        public override string CommandName
        {
            get
            {
                if (cn == null)
                {
                    var t = scope.TryGetVariable("CommandName", out cn);
                    if (!t) cn = "";
                }
                return cn;
            }
        }

        public override void Dispose()
        {
            Action a;
            if (scope.TryGetVariable<Action>("Dispose", out a))
            {
                a();
            }
        }

        public override void Initialize()
        {
            Action a;
            if (scope.TryGetVariable<Action>("Initialize", out a))
            {
                a();
            }
        }

        Action<User> login;
        bool? haslogin;
        public override void OnLogin(User user)
        {
            if (haslogin == null)
            {
                haslogin = scope.TryGetVariable("OnLogin", out login);
            }
            if (haslogin ?? false) login(user);
        }

        Action<User> logout;
        bool? haslogout;
        public override void OnLogout(User user)
        {
            if (haslogout == null)
            {
                haslogout = scope.TryGetVariable("OnLogout", out logout);
            }
            if (haslogout ?? false) logout(user);
        }

        Action bstream;
        bool? hasbstream;
        public override void OnStartStreaming()
        {
            if (hasbstream == null)
            {
                hasbstream = scope.TryGetVariable("OnStartStreaming", out bstream);
            }
            if (hasbstream ?? false) bstream();
        }

        Action fstream;
        bool? hasfstream;
        public override void OnStopStreaming()
        {
            if (hasfstream == null)
            {
                hasfstream = scope.TryGetVariable("OnStopStreaming", out fstream);
            }
            if (hasfstream ?? false) fstream();
        }

        Func<IList<string>, string> cmd;
        bool? hascmd;
        public override string OnCommand(IList<string> args)
        {
            if (hascmd == null)
            {
                hascmd = scope.TryGetVariable("OnCommand", out cmd);
            }

            if (hascmd ?? false)
            {
                return cmd(args);
            }
            else
            {
                return null;
            }
        }


        Action<DirectMessageMessage> dm;
        bool? hasdm;
        public override void OnDirectMessage(DirectMessageMessage mes)
        {
            if (hasdm == null)
            {
                hasdm = scope.TryGetVariable("OnDirectMessage", out dm);
            }
            if (hasdm ?? false) dm(mes);
        }

        Func<DirectMessageMessage, DirectMessageMessage> dmD;
        bool? hasdmD;
        public override DirectMessageMessage OnDirectMessageDestructive(DirectMessageMessage mes)
        {
            if (hasdmD == null)
            {
                hasdmD = scope.TryGetVariable("OnDirectMessageDestructive", out dmD);
            }
            if (hasdmD ?? false)
            {
                return dmD(mes);
            }
            else
            {
                return null;
            }
        }

        Action<EventMessage> ev;
        bool? hasev;
        public override void OnEvent(EventMessage mes)
        {
            if (hasev == null)
            {
                hasev = scope.TryGetVariable("OnEvent", out ev);
            }
            if (hasev ?? false) ev(mes);
        }

        Func<EventMessage, EventMessage> evD;
        bool? hasevD;
        public override EventMessage OnEventDestructive(EventMessage mes)
        {
            if (hasevD == null)
            {
                hasevD = scope.TryGetVariable("OnEventDestructive", out evD);
            }
            if (hasevD ?? false)
            {
                return evD(mes);
            }
            else
            {
                return null;
            }
        }

        Action<IdMessage> id;
        bool? hasid;
        public override void OnIdEvent(IdMessage mes)
        {
            if (hasid == null)
            {
                hasid = scope.TryGetVariable("OnIdEvent", out id);
            }
            if (hasid ?? false) id(mes);
        }

        Func<IdMessage, IdMessage> idD;
        bool? hasidD;
        public override IdMessage OnIdEventDestructive(IdMessage mes)
        {
            if (hasidD == null)
            {
                hasidD = scope.TryGetVariable("OnIdEventDestructive", out idD);
            }
            if (hasidD ?? false)
            {
                return idD(mes);
            }
            else
            {
                return null;
            }
        }

        Action<StatusMessage> st;
        bool? hasst;
        public override void OnStatus(StatusMessage mes)
        {
            if (hasst == null)
            {
                hasst = scope.TryGetVariable("OnStatus", out st);
            }
            if (hasst ?? false) st(mes);
        }

        Func<StatusMessage, StatusMessage> stD;
        bool? hasstD;
        public override StatusMessage OnStatusDestructive(StatusMessage mes)
        {
            if (hasstD == null)
            {
                hasstD = scope.TryGetVariable("OnStatusDestructive", out stD);
            }
            if (hasstD ?? false)
            {
                return stD(mes);
            }
            else
            {
                return null;
            }
        }

    }

    public class Kbtter4IronPythonPluginLoader : Kbtter4PluginLoader
    {
        ScriptEngine engine;

        public override string Language
        {
            get
            {
                return "IronPython";
            }
        }

        public override IEnumerable<Kbtter4Plugin> Load(Kbtter instance, IList<string> filenames)
        {
            var files = filenames.Where(p => p.EndsWith(".py"));
            var ret = new List<Kbtter4IronPythonPlugin>();

            engine = Python.CreateEngine();

            var context = HostingHelpers.GetLanguageContext(engine) as PythonContext;
            var path = context.GetSearchPaths();
            path.Add(Environment.CurrentDirectory + "\\");
            engine.SetSearchPaths(path);

            engine.Runtime.LoadAssembly(typeof(Status).Assembly);

            foreach (var i in files)
            {
                try
                {
                    var scope = engine.CreateScope();
                    scope.SetVariable("Kbtter4", new Kbtter4PluginProvider(instance));
                    var src = engine.CreateScriptSourceFromFile(i);
                    var code = src.Compile();
                    code.Execute(scope);
                    var p = new Kbtter4IronPythonPlugin(scope);
                    ret.Add(p);
                }
                catch (Exception e)
                {
                    instance.LogError(String.Format("プラグイン読み込み中にエラーが発生しました : {0}\n{1}", i, e.Message));
                    
                }
            }

            return ret;
        }
    }
}