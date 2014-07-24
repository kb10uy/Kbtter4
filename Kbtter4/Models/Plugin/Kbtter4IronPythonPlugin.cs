using System;
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
        Kbtter ins;
        public Kbtter4IronPythonPlugin(ScriptScope code,Kbtter k)
        {
            ins = k;
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

        public override void Dispose()
        {
            try
            {
                Action a;
                if (scope.TryGetVariable<Action>("Dispose", out a))
                {
                    a();
                }
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        public override void Initialize()
        {
            try
            {
                Action a;
                if (scope.TryGetVariable<Action>("Initialize", out a))
                {
                    a();
                }
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        Action<User> login;
        bool? haslogin;
        public override void OnLogin(User user)
        {
            try
            {
                if (haslogin == null)
                {
                    haslogin = scope.TryGetVariable("OnLogin", out login);
                }
                if (haslogin ?? false) login(user);
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        Action<User> logout;
        bool? haslogout;
        public override void OnLogout(User user)
        {
            try
            {
                if (haslogout == null)
                {
                    haslogout = scope.TryGetVariable("OnLogout", out logout);
                }
                if (haslogout ?? false) logout(user);
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
            }
        }

        Action bstream;
        bool? hasbstream;
        public override void OnStartStreaming()
        {
            try
            {
                if (hasbstream == null)
                {
                    hasbstream = scope.TryGetVariable("OnStartStreaming", out bstream);
                }
                if (hasbstream ?? false) bstream();
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        Action fstream;
        bool? hasfstream;
        public override void OnStopStreaming()
        {
            try
            {
                if (hasfstream == null)
                {
                    hasfstream = scope.TryGetVariable("OnStopStreaming", out fstream);
                }
                if (hasfstream ?? false) fstream();
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }


        Action<DirectMessageMessage> dm;
        bool? hasdm;
        public override void OnDirectMessage(DirectMessageMessage mes)
        {
            try
            {
                if (hasdm == null)
                {
                    hasdm = scope.TryGetVariable("OnDirectMessage", out dm);
                }
                if (hasdm ?? false) dm(mes);
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        Func<DirectMessageMessage, DirectMessageMessage> dmD;
        bool? hasdmD;
        public override DirectMessageMessage OnDirectMessageDestructive(DirectMessageMessage mes)
        {
            try
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
                    return mes;
                }
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
                return mes;
            }
        }

        Action<EventMessage> ev;
        bool? hasev;
        public override void OnEvent(EventMessage mes)
        {
            try
            {
                if (hasev == null)
                {
                    hasev = scope.TryGetVariable("OnEvent", out ev);
                }
                if (hasev ?? false) ev(mes);
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        Func<EventMessage, EventMessage> evD;
        bool? hasevD;
        public override EventMessage OnEventDestructive(EventMessage mes)
        {
            try
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
                    return mes;
                }
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
                return mes;
            }
        }

        Action<DeleteMessage> id;
        bool? hasid;
        public override void OnDelete(DeleteMessage mes)
        {
            try
            {
                if (hasid == null)
                {
                    hasid = scope.TryGetVariable("OnDelete", out id);
                }
                if (hasid ?? false) id(mes);
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        Func<DeleteMessage, DeleteMessage> idD;
        bool? hasidD;
        public override DeleteMessage OnDeleteDestructive(DeleteMessage mes)
        {
            try
            {
                if (hasidD == null)
                {
                    hasidD = scope.TryGetVariable("OnDeleteDestructive", out idD);
                }
                if (hasidD ?? false)
                {
                    return idD(mes);
                }
                else
                {
                    return mes;
                }
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
                return mes;
            }
        }

        Action<StatusMessage> st;
        bool? hasst;
        public override void OnStatus(StatusMessage mes)
        {
            try
            {
                if (hasst == null)
                {
                    hasst = scope.TryGetVariable("OnStatus", out st);
                }
                if (hasst ?? false) st(mes);
            }
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
            }
        }

        Func<StatusMessage, StatusMessage> stD;
        bool? hasstD;
        public override StatusMessage OnStatusDestructive(StatusMessage mes)
        {
            try
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
            catch (Exception e)
            {
                ins.LogError("プラグイン " + Name + "でエラーが発生しました : " + e.Message);
                ins.SaveLog();
                return mes;
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
                    var p = new Kbtter4IronPythonPlugin(scope,instance);
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
