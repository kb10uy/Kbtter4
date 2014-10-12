using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        List<DisplayObject> drawlist = new List<DisplayObject>();
        ConcurrentQueue<DisplayObject> adding = new ConcurrentQueue<DisplayObject>();
        Random rnd = new Random();
        object uslock = new object();

        public PlayerUser Player { get; protected set; }

        public SceneGame(Kbtter4Account ac)
        {
            tokens = Tokens.Create(Kbtter.Setting.Consumer.Key, Kbtter.Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);
            Player = new PlayerUser(this, tokens.Users.Show(user_id => ac.UserId));
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
                adding.Enqueue(new EnemyUser(this, p.Status, EnemyPatterns.Patterns[rnd.Next(EnemyPatterns.Patterns.Length)]));
            }));
            streams.Add(s.Connect());
        }

        public void AddBullet(Bullet b)
        {
            adding.Enqueue(b);
        }

        public override IEnumerator<bool> Tick()
        {
            StartConnection();
            drawlist.Add(Player);
            while (true)
            {
                DisplayObject de;
                while (adding.TryDequeue(out de)) drawlist.Add(de);
                foreach (var i in drawlist)
                {
                    var s = i.TickCoroutine.MoveNext();
                    if (!(s && i.TickCoroutine.Current))
                    {
                        i.IsDead = true;
                    }
                }
                drawlist.RemoveAll(p => p.IsDead);
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
                    if (!(s && i.TickCoroutine.Current)) i.IsDead = true;
                }
                drawlist.RemoveAll(p => p.IsDead);
                yield return true;
            }
        }
    }
}
