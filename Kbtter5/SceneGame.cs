using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;
using DxLibDLL;
using EasingSharp;
using Kbtter4.Models;

namespace Kbtter5
{
    public class SceneGame : Scene
    {
        private Kbtter Kbtter = Kbtter.Instance;
        private Kbtter5 Parent = Kbtter5.Instance;
        private Tokens tokens;
        private List<IDisposable> streams = new List<IDisposable>();
        public List<DisplayObject> Objects { get; set; }
        private ConcurrentQueue<DisplayObject> adding = new ConcurrentQueue<DisplayObject>();
        private Random rnd = new Random();
        private object uslock = new object();
        private string BackgroundImagePath = Path.Combine(CommonObjects.DataDirectory, "back.png");
        private bool hasback;

        public int TotalScore { get; private set; }
        private int frame = 0;
        private int prevtime = 0;
        private Queue<int> fpsq = new Queue<int>();

        public PlayerUser Player { get; protected set; }
        private Sprite Background { get; set; }
        private NumberSprite NumberFrames { get; set; }
        private NumberSprite NumberFps { get; set; }
        private NumberSprite NumberScore { get; set; }
        private StringSprite StringInfo { get; set; }
        private InformationBox Information { get; set; }

        public SceneGame(Kbtter4Account ac)
        {
            tokens = Tokens.Create(Kbtter.Setting.Consumer.Key, Kbtter.Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);
            Objects = new List<DisplayObject>();

            Player = new PlayerUser(this, tokens.Users.Show(user_id => ac.UserId)) { Layer = 5 };
            Information = new InformationBox(Player.SourceUser)
            {
                X = 0,
                Y = 480
            };
            NumberFrames = new NumberSprite(CommonObjects.ImageNumber24, 12, 24, 4)
            {
                X = 572,
                Y = 456,
                Layer = 12
            };
            NumberFps = new NumberSprite(CommonObjects.ImageNumber12White, 6, 12, 2)
            {
                X = 640 - 12,
                Y = 480 - 12,
                Layer = 12
            };
            NumberScore = new NumberSprite(CommonObjects.ImageNumber32, 16, 32, 8)
            {
                X = 8,
                Y = 8,
                Layer = 12
            };
            StringInfo = new StringSprite(CommonObjects.FontSystem, DX.GetColor(255, 255, 255))
            {
                X = 566,
                Y = 436,
                Value = "Objects"
            };
        }

        ~SceneGame()
        {
            streams.ForEach(p => p.Dispose());
        }




        private void StartConnection()
        {
            var s = tokens.Streaming.StartObservableStream(StreamingType.User, new StreamingParameters(include_entities => "true")).Publish();
            streams.Add(s.OfType<StatusMessage>().Subscribe(ProcessStatus));
            streams.Add(s.Connect());

            if (File.Exists(BackgroundImagePath))
            {
                Background = new Sprite() { Image = DX.LoadGraph(BackgroundImagePath), HomeX = 480, HomeY = 360, X = 320, Y = 240, Alpha = 0.4, Layer = -10 };
                hasback = true;
            }
            else if (Player.SourceUser.IsProfileUseBackgroundImage)
            {
                Task.Run(() =>
                {
                    using (var wc = new WebClient())
                    using (var st = wc.OpenRead(Player.SourceUser.ProfileBackgroundImageUrlHttps))
                    {
                        var bm = new Bitmap(st);
                        var sav = new Bitmap(bm, 960, 720);
                        sav.Save(BackgroundImagePath, ImageFormat.Jpeg);
                    }
                    Background = new Sprite() { Image = DX.LoadGraph(BackgroundImagePath), HomeX = 480, HomeY = 360, X = 320, Y = 240, Alpha = 0.4 };
                    adding.Enqueue(Background);
                    hasback = true;
                });
            }
        }

        private void ProcessStatus(StatusMessage p)
        {
            if (p.Status.RetweetedStatus != null)
            {
                adding.Enqueue(new EnemyUser(this, p.Status, EnemyPatterns.RetweeterMultiCannon));
                adding.Enqueue(new StatusSprite(p.Status));
            }
            else
            {
                adding.Enqueue(new EnemyUser(this, p.Status, EnemyPatterns.Patterns[rnd.Next(EnemyPatterns.Patterns.Length)]));
                adding.Enqueue(new StatusSprite(p.Status));
            }
        }

        public void AddBullet(Bullet b)
        {
            adding.Enqueue(b);
        }

        public void AddObject(DisplayObject obj)
        {
            adding.Enqueue(obj);
        }

        public void Score(int pts)
        {
            TotalScore += pts;
        }

        public bool UseBomb()
        {
            if (Information.Bombs <= 0) return false;
            Information.Popup();
            Information.Bombs--;
            return true;
        }

        public bool Miss()
        {
            if (Information.Players <= 0)
            {
                AddObject(new StringSprite(CommonObjects.FontSystemBig, DX.GetColor(255, 64, 64)) { Value = "ゲームオーバー", X = 164, Y = 100 });
                return false;
            }
            Information.Popup();
            Information.Players--;
            Information.ResetBomb();
            return true;
        }

        public override IEnumerator<bool> Tick()
        {
            StartConnection();

            if (hasback) Objects.Add(Background);
            Objects.Add(Player);
            Objects.Add(Information);
            Objects.Add(StringInfo);
            Objects.Add(NumberFrames);
            Objects.Add(NumberFps);
            Objects.Add(NumberScore);
            Information.Popup();
            prevtime = DX.GetNowCount();
            while (true)
            {
                //FPS計算
                if (frame % 15 == 0 && frame / 15 > 0)
                {
                    var time = DX.GetNowCount();
                    var dist = (time - prevtime);
                    var fps = 1000.0 / (dist / 15.0);
                    fpsq.Enqueue((int)Math.Round(fps));
                    if (fpsq.Count > 4) fpsq.Dequeue();
                    NumberFps.Value = (int)fpsq.Average();
                    prevtime = time;
                }
                //背景のアレ
                if (hasback)
                {
                    int mx, my;
                    DX.GetMousePoint(out mx, out my);
                    Background.X = 320 - (mx - 320) * 0.25;
                    Background.Y = 240 - (my - 240) * 0.25;
                }

                DisplayObject de;
                while (adding.TryDequeue(out de)) Objects.Add(de);
                foreach (var i in Objects)
                {
                    var s = i.TickCoroutine.MoveNext();
                    if (!(s && i.TickCoroutine.Current))
                    {
                        i.IsDead = true;
                    }
                }
                Objects.RemoveAll(p => p.IsDead);

                NumberFrames.Value = Objects.Count;
                NumberScore.Value = TotalScore;
                frame++;
                yield return true;
            }

        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                foreach (var i in Objects.OrderBy(p => p.Layer))
                {
                    var s = i.DrawCoroutine.MoveNext();
                    if (!(s && i.TickCoroutine.Current)) i.IsDead = true;
                }
                Objects.RemoveAll(p => p.IsDead);
                yield return true;
            }
        }
    }

    public class StatusSprite : StringSprite
    {
        private Status status;
        private int down;
        private static int enable = 0;

        public StatusSprite(Status st)
            : base(CommonObjects.FontSystem, DX.GetColor(255, 255, 255))
        {
            status = st;
            Value = status.Text;
            if (status.RetweetedStatus != null) Color = DX.GetColor(200, 255, 200);
            X = 0;
            down = 16 * (enable + 1);
            Y = 480 - down;
        }

        public override IEnumerator<bool> Tick()
        {
            enable++;
            var allx = DX.GetDrawStringWidthToHandle(status.Text, status.Text.Length, FontHandle);
            for (int i = 0; i < 30; i++)
            {
                HomeX = Easing.OutCubic(i, 30, allx, -allx);
                yield return true;
            }
            for (int i = 0; i < 60; i++) yield return true;
            enable--;
            var sy = Y;
            for (int i = 0; i < 30; i++)
            {
                Y = Easing.InCubic(i, 30, sy, down);
                yield return true;
            }
        }
    }

    public class InformationBox : DisplayObject
    {
        public int Players { get; set; }
        public int Bombs { get; set; }
        public int BackColor { get; private set; }
        public int FontColor { get; private set; }
        public IEnumerator<bool> Operation { get; private set; }
        private int defb;

        public InformationBox(User u)
        {
            Players = (int)(Math.Log10(u.FollowersCount) * Math.Log10(u.FriendsCount)) * 4;
            Bombs = (int)(Math.Log10(u.FavouritesCount) + Math.Log10(u.StatusesCount)) * 2;
            defb = Bombs;
            BackColor = DX.GetColor(100, 100, 100);
            FontColor = DX.GetColor(255, 255, 255);
        }

        public void Popup()
        {
            Operation = PopupOperation();
        }

        private IEnumerator<bool> PopupOperation()
        {
            for (int i = 0; i < 20; i++)
            {
                Y = Easing.OutCubic(i, 20, 480, -112);
                yield return true;
            }
            for (int i = 0; i < 120; i++) yield return true;
            for (int i = 0; i < 60; i++)
            {
                Y = Easing.OutCubic(i, 60, 480 - 112, 112);
                yield return true;
            }
        }

        public void ResetBomb()
        {
            Bombs = defb;
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                Operation = (Operation != null && Operation.MoveNext() && Operation.Current) ? Operation : null;
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 100);
                DX.DrawBox((int)X, (int)Y, (int)(X + 400), (int)(Y + 112), BackColor, DX.TRUE);

                DX.DrawStringToHandle((int)(X + 8), (int)(Y + 8), "残り人数", FontColor, CommonObjects.FontSystemBig);
                var ds = Players.ToString();
                var dw = DX.GetDrawStringWidthToHandle(ds, ds.Length, CommonObjects.FontSystemBig);
                DX.DrawStringToHandle((int)(384 - dw), (int)(Y + 8), ds, FontColor, CommonObjects.FontSystemBig);

                DX.DrawStringToHandle((int)(X + 8), (int)(Y + 64), "残りボム", FontColor, CommonObjects.FontSystemBig);
                ds = Bombs.ToString();
                dw = DX.GetDrawStringWidthToHandle(ds, ds.Length, CommonObjects.FontSystemBig);
                DX.DrawStringToHandle((int)(384 - dw), (int)(Y + 64), ds, FontColor, CommonObjects.FontSystemBig);

                yield return true;
            }
        }
    }
}
