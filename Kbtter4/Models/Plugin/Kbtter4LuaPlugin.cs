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
    class Kbtter4LuaPlugin : Kbtter4Plugin
    {
        Lua lua;
        public Kbtter4LuaPlugin(Lua l)
        {
            lua = l;
        }

        public override string Name
        {
            get
            {
                return lua["Name"] as string;
            }
        }

        public override string CommandName
        {
            get { throw new NotImplementedException(); }
        }

        public override void Initialize()
        {
            try
            {
                var f = lua["Initialize"] as Action;
                if (f != null) f();
            }
            catch { }
        }

        public override void Dispose()
        {
            try
            {
                var f = lua["Dispose"] as Action;
                if (f != null) f();
            }
            catch { }
        }

        public override void OnLogin(User user)
        {
            try
            {
                var f = lua["OnLogin"] as Action<User>;
                if (f != null) f(user);
            }
            catch { }
        }

        public override void OnLogout(User user)
        {
            try
            {
                var f = lua["OnLogout"] as Action<User>;
                if (f != null) f(user);
            }
            catch { }
        }

        public override void OnStartStreaming()
        {
            try
            {
                var f = lua["OnStartStreaming"] as Action;
                if (f != null) f();
            }
            catch { }
        }

        public override void OnStopStreaming()
        {
            try
            {
                var f = lua["OnStopStreaming"] as Action;
                if (f != null) f();
            }
            catch { }
        }

        public override string OnCommand(IList<string> args)
        {
            throw new NotImplementedException();
        }

        public override void OnStatus(StatusMessage mes)
        {
            try
            {
                var f = lua["OnStatus"] as Action<StatusMessage>;
                if (f != null) f(mes);
            }
            catch { }
        }

        public override void OnEvent(EventMessage mes)
        {
            try
            {
                var f = lua["OnEvent"] as Action<EventMessage>;
                if (f != null) f(mes);
            }
            catch { }
        }

        public override void OnIdEvent(IdMessage mes)
        {
            try
            {
                var f = lua["OnIdEvent"] as Action<IdMessage>;
                if (f != null) f(mes);
            }
            catch { }
        }

        public override void OnDirectMessage(DirectMessageMessage mes)
        {
            try
            {
                var f = lua["OnDirectMessage"] as Action<DirectMessageMessage>;
                if (f != null) f(mes);
            }
            catch { }
        }

        public override StatusMessage OnStatusDestructive(StatusMessage mes)
        {
            try
            {
                var f = lua["OnStatusDestructive"] as Func<StatusMessage, StatusMessage>;
                if (f != null)
                {
                    return f(mes);
                }
                else
                {
                    return mes;
                }
            }
            catch
            {
                return mes;
            }
        }

        public override EventMessage OnEventDestructive(EventMessage mes)
        {
            try
            {
                var f = lua["OnEventDestructive"] as Func<EventMessage, EventMessage>;
                if (f != null)
                {
                    return f(mes);
                }
                else
                {
                    return mes;
                }
            }
            catch
            {
                return mes;
            }
        }

        public override IdMessage OnIdEventDestructive(IdMessage mes)
        {
            try
            {
                var f = lua["OnIdEventDestructive"] as Func<IdMessage, IdMessage>;
                if (f != null)
                {
                    return f(mes);
                }
                else
                {
                    return mes;
                }
            }
            catch
            {
                return mes;
            }
        }

        public override DirectMessageMessage OnDirectMessageDestructive(DirectMessageMessage mes)
        {
            try
            {
                var f = lua["OnDirectMessageDestructive"] as Func<DirectMessageMessage, DirectMessageMessage>;
                if (f != null)
                {
                    return f(mes);
                }
                else
                {
                    return mes;
                }
            }
            catch
            {
                return mes;
            }
        }
    }

    public sealed class Kbtter4LuaPluginLoader : Kbtter4PluginLoader
    {

        public override string Language
        {
            get { return "Lua"; }
        }

        public override IEnumerable<Kbtter4Plugin> Load(Kbtter instance, IList<string> filenames)
        {
            var list = new List<Kbtter4LuaPlugin>();
            var files = filenames.Where(p => p.EndsWith(".lua"));
            foreach (var i in files)
            {
                try
                {
                    Lua l = new Lua();
                    l.LoadCLRPackage();
                    l["Kbtter4"] = new Kbtter4PluginProvider(instance);
                    l.DoFile(i);
                    list.Add(new Kbtter4LuaPlugin(l));
                }
                catch (Exception e)
                {
                    instance.LogError("Luaプラグイン読み込み中にエラーが発生しました : " + e.Message);
                }
            }

            return list;
        }
    }
}
