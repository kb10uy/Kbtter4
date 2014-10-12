using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasingSharp;
using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;
using Kbtter4.Models;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DxLibDLL;

namespace Kbtter5
{
    public class SceneGame : Scene
    {
        Kbtter Kbtter = Kbtter.Instance;
        Kbtter5 Parent = Kbtter5.Instance;
        Tokens tokens;
        List<IDisposable> streams = new List<IDisposable>();
        HashSet<DisplayObject> drawlist = new HashSet<DisplayObject>();

        public SceneGame(Kbtter4Account ac)
        {
            tokens = Tokens.Create(Kbtter.Setting.Consumer.Key, Kbtter.Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);
        }

        ~SceneGame()
        {
            streams.ForEach(p => p.Dispose());
        }

        private void StartConnection()
        {
            var s = tokens.Streaming.StartObservableStream(StreamingType.User, new StreamingParameters(include_entities => "true")).Publish();
            streams.Add(s.OfType<StatusMessage>().Subscribe(p =>
            {
                DX.SetWindowText(p.Status.Text);
                lock (drawlist) drawlist.Add(new EnemyUser(this, p.Status.User, EnemyPatterns.GoDownAndAway) { X = 40, Y = -20 });
            }));
            streams.Add(s.Connect());
        }

        public void AddBullet(Bullet b)
        {
            drawlist.Add(b);
        }

        public override IEnumerator<bool> Tick()
        {
            StartConnection();
            while (true)
            {
                lock (drawlist)
                {
                    foreach (var i in drawlist)
                    {
                        var s = i.TickCoroutine.MoveNext();
                        if (!s || !i.TickCoroutine.Current)
                        {
                            i.IsDead = true;
                        }
                    }
                }
                drawlist.RemoveWhere(p => p.IsDead);
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                foreach (var i in drawlist)
                {
                    var s = i.DrawCoroutine.MoveNext();
                    if (!s || !i.DrawCoroutine.Current) i.IsDead = true;
                }
                drawlist.RemoveWhere(p => p.IsDead);
                yield return true;
            }
        }
    }
}
