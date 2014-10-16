using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Random rnd = new Random();
        private string BackgroundImagePath = Path.Combine(CommonObjects.DataDirectory, "back.png");
        private bool hasback;
        private Stopwatch sw = new Stopwatch();

        public int TotalScore { get; private set; }
        private int frame = 0;
        private int prevtime = 0;
        private Queue<int> fpsq = new Queue<int>();

        public PlayerUser Player { get; protected set; }
        private Sprite Background;
        private NumberSprite NumberFrames;
        private NumberSprite NumberFps;
        private NumberSprite NumberScore;
        private StringSprite StringInfo;
        private InformationBox Information;
        private Sprite ImageScore;

        public SceneGame(Kbtter4Account ac)
        {
            tokens = Tokens.Create(Kbtter.Setting.Consumer.Key, Kbtter.Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);
            Player = new PlayerUser(this, tokens.Users.Show(user_id => ac.UserId), PlayerOperations.MouseOperaiton);
            Information = new InformationBox(Player.SourceUser)
            {
                X = 0,
                Y = 480
            };
            NumberFrames = new NumberSprite(CommonObjects.ImageNumber24, 12, 24, 4)
            {
                X = 572,
                Y = 456,
            };
            NumberFps = new NumberSprite(CommonObjects.ImageNumber12White, 6, 12, 2)
            {
                X = 640 - 12,
                Y = 480 - 12,
            };
            NumberScore = new NumberSprite(CommonObjects.ImageNumber32, 16, 32, 8)
            {
                X = 16 + 128,
                Y = 8,
                FillWithZero = true
            };
            StringInfo = new StringSprite(CommonObjects.FontSystem, CommonObjects.Colors.White)
            {
                X = 566,
                Y = 436,
                Value = "Loading"
            };

            ImageScore = new Sprite() { Image = CommonObjects.ImageScore, X = 8, Y = 8 };
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
                Background = new Sprite() { Image = DX.LoadGraph(BackgroundImagePath), HomeX = 480, HomeY = 360, X = 320, Y = 240, Alpha = 0.4 };
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
                    Manager.Add(Background, (int)GameLayer.Background);
                    hasback = true;
                });
            }
        }

        private void ProcessStatus(StatusMessage p)
        {
            if (DX.GetActiveFlag() != DX.TRUE) return;
            if (p.Status.RetweetedStatus != null)
            {
                Manager.Add(new EnemyUser(this, p.Status, EnemyPatterns.RetweeterMultiCannon), (int)GameLayer.Enemy);
            }
            else
            {
                Manager.Add(new EnemyUser(this, p.Status, EnemyPatterns.Patterns[rnd.Next(EnemyPatterns.Patterns.Length)]), (int)GameLayer.Enemy);
            }
            //Manager.Add(new StatusSprite(p.Status), (int)GameLayer.Information);
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
                Manager.Add(new StringSprite(CommonObjects.FontSystemBig, CommonObjects.Colors.Crimson)
                {
                    Value = "ゲームオーバー",
                    X = 164,
                    Y = 100
                }, (int)GameLayer.Information);
                return false;
            }
            Information.Popup();
            Information.Players--;
            Information.ResetBomb();
            return true;
        }

        public override IEnumerator<bool> Tick()
        {
            Manager.Add(StringInfo, (int)GameLayer.Information);
            while (DX.GetASyncLoadNum() != 0)
            {
                Manager.TickAll();
                yield return true;
            }
            StartConnection();
            StringInfo.Value = "Objects";

            if (hasback) Manager.Add(Background, (int)GameLayer.Background);
            Manager.Add(Player, (int)GameLayer.Player);
            Manager.Add(Information, (int)GameLayer.Information);
            Manager.Add(NumberFrames, (int)GameLayer.Information);
            Manager.Add(NumberFps, (int)GameLayer.Information);
            Manager.Add(NumberScore, (int)GameLayer.Information);
            Manager.Add(ImageScore, (int)GameLayer.Information);
            Information.Popup();
            prevtime = DX.GetNowCount();
            while (true)
            {
                sw.Reset();
                sw.Start();
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
                    Background.X = 320 - (Player.X - 320) * 0.25;
                    Background.Y = 240 - (Player.Y - 240) * 0.25;
                }

                Manager.TickAll();
                sw.Stop();
                NumberFrames.Value = (int)sw.ElapsedMilliseconds;//Manager.Count;
                NumberScore.Value = TotalScore;
                frame++;
                yield return true;
            }

        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                Manager.DrawAll();
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
            : base(CommonObjects.FontSystem, CommonObjects.Colors.White)
        {
            status = st;
            Value = status.Text;
            if (status.RetweetedStatus != null) Color = CommonObjects.Colors.LawnGreen;
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
            BackColor = CommonObjects.Colors.DimGray;
            FontColor = CommonObjects.Colors.White;
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

    public enum GameLayer
    {
        Background = 0,
        EnemyBullet,
        Enemy,
        PlayerBullet,
        Player,
        Effect,
        Information,
    }
}
