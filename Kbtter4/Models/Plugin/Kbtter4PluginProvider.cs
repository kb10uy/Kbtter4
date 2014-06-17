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

namespace Kbtter4.Models.Plugin
{
    /// <summary>
    /// Kbtter4のプラグインローダーの規約を定義します。
    /// </summary>
    public abstract class Kbtter4PluginProvider
    {
        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        public Kbtter4PluginProvider()
        {

        }

        #region プラグイン情報
        /// <summary>
        /// このローダーが連携している言語名を取得します。
        /// </summary>
        public abstract string ProvidingLanguage { get; }

        /// <summary>
        /// 現在このローダーが読み込んでいるプラグイン数を取得します。
        /// </summary>
        public abstract int ProvidingPluginsCount { get; }
        #endregion

        #region システム関係
        /// <summary>
        /// ローダー自身を初期化します。
        /// </summary>
        /// <param name="kbtter">Kbtter3 Modelのインスタンス</param>
        public abstract void Initialize(Kbtter kbtter);

        /// <summary>
        /// ローダーを開放します。
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// プラグインを読み込みます。
        /// </summary>
        /// <param name="filenames">
        /// プラグインのファイル名リスト。
        /// 全て小文字で返されます。
        /// </param>
        /// <returns>エラーのあったプラグインの数</returns>
        public abstract int Load(IList<string> filenames);

        /// <summary>
        /// プラグインを初期化します。
        /// </summary>
        public abstract void PluginInitialze();

        /// <summary>
        /// Kbtter4側から特殊なメッセージを送信し、処理します。
        /// </summary>
        /// <param name="msg">メッセージ</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract void SystemRequest(string msg, object mon);
        #endregion

        #region 非破壊的メソッド
        /// <summary>
        /// 受信したツイートについて、スクリプトの処理をします。
        /// </summary>
        /// <param name="msg">StatusMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract void StatusUpdate(StatusMessage msg, object mon);

        /// <summary>
        /// 受信したイベントについて、スクリプトの処理をします。
        /// </summary>
        /// <param name="msg">EventMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract void EventUpdate(EventMessage msg, object mon);

        /// <summary>
        /// 受信したIDのイベントについて、スクリプトの処理をします。
        /// </summary>
        /// <param name="msg">IdMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract void IdEventUpdate(IdMessage msg, object mon);

        /// <summary>
        /// 受信したダイレクトメッセージについて、スクリプトの処理をします。
        /// </summary>
        /// <param name="msg">DirectMessageMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract void DirectMessageUpdate(DirectMessageMessage msg, object mon);
        #endregion

        #region 破壊的メソッド
        /// <summary>
        /// 受信したツイートについて、スクリプトの処理をします。
        /// このイベントは、ツイートの表示前に処理されます。
        /// </summary>
        /// <param name="msg">StatusMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract StatusMessage StatusUpdateDestructive(StatusMessage msg, object mon);

        /// <summary>
        /// 受信したイベントについて、スクリプトの処理をします。
        /// このイベントは、ツイートの表示前に処理されます。
        /// </summary>
        /// <param name="msg">EventMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract EventMessage EventUpdateDestructive(EventMessage msg, object mon);

        /// <summary>
        /// 受信したIDのイベントについて、スクリプトの処理をします。
        /// このイベントは、ツイートの表示前に処理されます。
        /// </summary>
        /// <param name="msg">IdMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract IdMessage IdEventUpdateDestructive(IdMessage msg, object mon);

        /// <summary>
        /// 受信したダイレクトメッセージについて、スクリプトの処理をします。
        /// このイベントは、ツイートの表示前に処理されます。
        /// </summary>
        /// <param name="msg">DirectMessageMessage</param>
        /// <param name="mon">非同期的に処理するためのモニター用オブジェクト</param>
        public abstract DirectMessageMessage DirectMessageUpdateDestructive(DirectMessageMessage msg, object mon);
        #endregion


    }

    /// <summary>
    /// 
    /// </summary>
    public class Kbtter4PluginEventProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public Kbtter Instance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public event Action Initialize;

        /// <summary>
        /// 
        /// </summary>
        public event Action<StatusMessage> StatusReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Action<EventMessage> EventReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Action<IdMessage> IdReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Action<DirectMessageMessage> DirectMessageReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Func<StatusMessage, StatusMessage> StatusReceivedDestructive;

        /// <summary>
        /// 
        /// </summary>
        public event Func<EventMessage, EventMessage> EventReceivedDestructive;

        /// <summary>
        /// 
        /// </summary>
        public event Func<IdMessage, IdMessage> IdReceivedDestructive;

        /// <summary>
        /// 
        /// </summary>
        public event Func<DirectMessageMessage, DirectMessageMessage> DirectMessageReceivedDestructive;

        #region 簡易アクセス
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void Tweet(string text)
        {
            try
            {
                Instance.Token.Statuses.UpdateAsync(Status => text);
            }
            catch
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        public void Reply(long id, string text)
        {
            try
            {
                Instance.Token.Statuses.Update(Status => text, in_reply_to_status_id => id);
            }
            catch
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fid"></param>
        public void Favorite(long fid)
        {
            try
            {
                Instance.Token.Favorites.CreateAsync(id => fid);
            }
            catch
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fid"></param>
        public void Unfavorite(long fid)
        {
            try
            {
                Instance.Token.Favorites.DestroyAsync(id => fid);
            }
            catch
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rid"></param>
        public void Retweet(long rid)
        {
            try
            {
                Instance.Token.Statuses.RetweetAsync(id => rid);
            }
            catch
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="did"></param>
        public void Delete(long did)
        {
            try
            {
                Instance.Token.Statuses.DestroyAsync(id => did);
            }
            catch
            { }
        }
        #endregion

        #region イベント励起
        internal void RaiseInitialize()
        {
            if (Initialize == null) return;
            var s = Initialize.GetInvocationList().Select(p => p as Action);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    i();
                }
                catch (Exception e)
                {
                    Instance.LogError("プラグイン初期化時イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
        }

        internal void RaiseStatus(StatusMessage msg)
        {
            var m = msg;
            if (StatusReceived == null) return;
            var s = StatusReceived.GetInvocationList().Select(p => p as Action<StatusMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    i(m);
                }
                catch (Exception e)
                {
                    Instance.LogError("ツイート受信時イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
        }

        internal void RaiseEvent(EventMessage msg)
        {
            if (EventReceived == null) return;
            var s = EventReceived.GetInvocationList().Select(p => p as Action<EventMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    i(msg);
                }
                catch (Exception e)
                {
                    Instance.LogError("イベント受信時イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
        }

        internal void RaiseIdEvent(IdMessage msg)
        {
            if (IdReceived == null) return;
            var s = IdReceived.GetInvocationList().Select(p => p as Action<IdMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    i(msg);
                }
                catch (Exception e)
                {
                    Instance.LogError("IDイベント受信時イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
        }

        internal void RaiseDirectMessage(DirectMessageMessage msg)
        {
            if (DirectMessageReceived == null) return;
            var s = DirectMessageReceived.GetInvocationList().Select(p => p as Action<DirectMessageMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    i(msg);
                }
                catch (Exception e)
                {
                    Instance.LogError("ダイレクトメッセージ受信時イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
        }
        #endregion

        #region 破壊的イベント励起
        internal StatusMessage RaiseStatusDestructive(StatusMessage msg)
        {
            var m = msg;
            if (StatusReceivedDestructive == null) return null;
            var s = StatusReceivedDestructive.GetInvocationList().Select(p => p as Func<StatusMessage, StatusMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    m = i(m);
                }
                catch (Exception e)
                {
                    Instance.LogError("ツイート受信時破壊的イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
            return m;
        }

        internal EventMessage RaiseEventDestructive(EventMessage msg)
        {
            var m = msg;
            if (EventReceivedDestructive == null) return null;
            var s = EventReceivedDestructive.GetInvocationList().Select(p => p as Func<EventMessage, EventMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    m = i(m);
                }
                catch (Exception e)
                {
                    Instance.LogError("イベント受信時破壊的イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
            return m;
        }

        internal IdMessage RaiseIdEventDestructive(IdMessage msg)
        {
            var m = msg;
            if (IdReceivedDestructive == null) return null;
            var s = IdReceivedDestructive.GetInvocationList().Select(p => p as Func<IdMessage, IdMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    m = i(m);
                }
                catch (Exception e)
                {
                    Instance.LogError("IDイベント受信時破壊的イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
            return m;
        }

        internal DirectMessageMessage RaiseDirectMessageDestructive(DirectMessageMessage msg)
        {
            var m = msg;
            if (DirectMessageReceivedDestructive == null) return null;
            var s = DirectMessageReceivedDestructive.GetInvocationList().Select(p => p as Func<DirectMessageMessage, DirectMessageMessage>);
            foreach (var i in s.Where(p => p != null))
            {
                try
                {
                    m = i(m);
                }
                catch (Exception e)
                {
                    Instance.LogError("ダイレクトメッセージ受信時破壊的イベントでエラーが発生しました\n" + e.Message);
                    Instance.SaveLog();
                }
            }
            return m;
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class Kbtter4PluginHandlerRegisterableEventProvider : Kbtter4PluginEventProvider
    {
        #region 非破壊的
        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterInitialize(Action act)
        {
            Initialize += act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterStatusReceived(Action<StatusMessage> act)
        {
            StatusReceived += act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterEventReceived(Action<EventMessage> act)
        {
            EventReceived += act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterEventReceived(Action<IdMessage> act)
        {
            IdReceived += act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterDirectMessageReceived(Action<DirectMessageMessage> act)
        {
            DirectMessageReceived += act;
        }
        #endregion

        #region 破壊的
        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterStatusReceivedDestructive(Func<StatusMessage, StatusMessage> act)
        {
            StatusReceivedDestructive += act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterEventReceivedDestructive(Func<EventMessage, EventMessage> act)
        {
            EventReceivedDestructive += act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterEventReceivedDestructive(Func<IdMessage, IdMessage> act)
        {
            IdReceivedDestructive += act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        public void RegisterDirectMessageReceivedDestructive(Func<DirectMessageMessage, DirectMessageMessage> act)
        {
            DirectMessageReceivedDestructive += act;
        }
        #endregion
    }
}
