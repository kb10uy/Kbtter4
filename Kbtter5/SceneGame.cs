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
        public double ShowingScore { get; private set; }
        private int frame = 0;
        private int prevtime = 0;
        private Queue<int> fpsq = new Queue<int>();

        public PlayerUser Player { get; protected set; }
        private Sprite Background;
        private StringSprite StringInfo;
        private InformationBox Information;

        public SceneGame(Kbtter4Account ac)
        {
            tokens = Tokens.Create(Kbtter.Setting.Consumer.Key, Kbtter.Setting.Consumer.Secret, ac.AccessToken, ac.AccessTokenSecret);
            Player = new PlayerUser(this, tokens.Users.Show(user_id => ac.UserId), PlayerOperations.MouseOperaiton);
            Information = new InformationBox(Player.SourceUser, Player)
            {
                X = 0,
                Y = 480
            };
            StringInfo = new StringSprite(CommonObjects.FontBullet, CommonObjects.Colors.White)
            {
                X = 280,
                Y = 240,
                Value = "Loading"
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
            //if (DX.GetActiveFlag() != DX.TRUE) return;
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
            Information.Bombs--;
            return true;
        }

        public void Graze()
        {
            Information.NumberGraze.Value++;
        }

        public void DestroyEnemy()
        {
            Information.NumberDestroy.Value++;
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
            StringInfo.IsDead = true;
            StartConnection();

            if (hasback) Manager.Add(Background, (int)GameLayer.Background);
            Manager.Add(Player, (int)GameLayer.Player);
            Manager.Add(Information, (int)GameLayer.Information);
            Information.Popup();
            prevtime = DX.GetNowCount();
            while (true)
            {
                //sw.Reset();
                //sw.Start();
                //FPS計算
                if (frame % 15 == 0 && frame / 15 > 0)
                {
                    var time = DX.GetNowCount();
                    var dist = (time - prevtime);
                    var fps = 1000.0 / (dist / 15.0);
                    fpsq.Enqueue((int)Math.Round(fps));
                    if (fpsq.Count > 4) fpsq.Dequeue();
                    Information.NumberFps.Value = (int)fpsq.Average();
                    prevtime = time;
                }
                //背景のアレ
                if (hasback)
                {
                    Background.X = 320 - (Player.X - 320) * 0.25;
                    Background.Y = 240 - (Player.Y - 240) * 0.25;
                }

                Manager.TickAll();
                //sw.Stop();
                Information.NumberFrames.Value = Manager.Count;
                if (ShowingScore < TotalScore)
                {
                    ShowingScore = Math.Min(TotalScore, ShowingScore + (((TotalScore - ShowingScore) / 30.0) + 7));
                }
                Information.NumberScore.Value = (int)ShowingScore;
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
        public ObjectManager Manager { get; private set; }
        private int defb;
        public NumberSprite NumberFrames { get; set; }
        public NumberSprite NumberFps { get; set; }
        public NumberSprite NumberScore { get; set; }
        public NumberSprite NumberPlayers { get; set; }
        public NumberSprite NumberBombs { get; set; }
        public NumberSprite NumberDestroy { get; set; }
        public NumberSprite NumberGraze { get; set; }
        private PlayerUser user;
        private double pux;

        public InformationBox(User u, PlayerUser pu)
        {
            Players = (int)(Math.Log10(u.FollowersCount) * Math.Log10(u.FriendsCount)) * 4;
            Bombs = (int)(Math.Log10(u.FavouritesCount) + Math.Log10(u.StatusesCount)) * 2;
            defb = Bombs;
            BackColor = CommonObjects.Colors.DimGray;
            FontColor = CommonObjects.Colors.White;
            Manager = new ObjectManager(2);
            user = pu;
            Y = 432;

            NumberScore = new NumberSprite(CommonObjects.ImageNumber32, 16, 32, 8)
            {
                X = 16 + 128,
                Y = 8,
                FillWithZero = true
            };
            NumberFrames = new NumberSprite(CommonObjects.ImageNumber24, 12, 24, 4)
            {
                X = 572,
                Y = 20,
            };
            NumberFps = new NumberSprite(CommonObjects.ImageNumber12White, 6, 12, 2)
            {
                X = 640 - 12,
                Y = 36,
            };

            NumberPlayers = new NumberSprite(CommonObjects.ImageNumber16, 8, 16, 3)
            {
                X = 384,
                Y = 4,
                FillWithZero = true
            };
            NumberBombs = new NumberSprite(CommonObjects.ImageNumber16, 8, 16, 3)
            {
                X = 524,
                Y = 4,
                FillWithZero = true
            };
            NumberDestroy = new NumberSprite(CommonObjects.ImageNumber16, 8, 16, 4)
            {
                X = 376,
                Y = 28,
                FillWithZero = true
            };
            NumberGraze = new NumberSprite(CommonObjects.ImageNumber16, 8, 16, 5)
            {
                X = 508,
                Y = 28,
                FillWithZero = true
            };
        }

        public void Popup()
        {
            Operation = PopupOperation();
        }

        public void Popdown()
        {
            Operation = PopdownOperation();
        }

        private IEnumerator<bool> PopupOperation()
        {
            var y = Y;
            for (int i = 0; i < 30; i++)
            {
                Y = Easing.OutCubic(i, 30, y, 432 - y);
                yield return true;
            }
        }

        private IEnumerator<bool> PopdownOperation()
        {
            var y = Y;
            for (int i = 0; i < 30; i++)
            {
                Y = Easing.OutCubic(i, 30, y, 480 - y);
                yield return true;
            }
        }

        public void ResetBomb()
        {
            Bombs = defb;
        }

        public override IEnumerator<bool> Tick()
        {
            Manager.Add(new Sprite() { Image = CommonObjects.ImageScore, X = 8, Y = 8 }, 1);
            Manager.Add(new StringSprite(CommonObjects.FontBullet, CommonObjects.Colors.White) { Value = "残り人数", X = 280, Y = 4 }, 1);
            Manager.Add(new StringSprite(CommonObjects.FontBullet, CommonObjects.Colors.White) { Value = "残りボム", X = 420, Y = 4 }, 1);
            Manager.Add(new StringSprite(CommonObjects.FontBullet, CommonObjects.Colors.White) { Value = "撃墜数", X = 280, Y = 28 }, 1);
            Manager.Add(new StringSprite(CommonObjects.FontBullet, CommonObjects.Colors.White) { Value = "グレイズ", X = 420, Y = 28 }, 1);
            Manager.Add(new StringSprite(CommonObjects.FontSystem, CommonObjects.Colors.White) { Value = "描画総数", X = 566, Y = 4 }, 1);
            Manager.Add(NumberScore, 1);
            Manager.Add(NumberFrames, 1);
            Manager.Add(NumberFps, 1);
            Manager.Add(NumberPlayers, 1);
            Manager.Add(NumberBombs, 1);
            Manager.Add(NumberDestroy, 1);
            Manager.Add(NumberGraze, 1);

            pux = user.Y;
            while (true)
            {
                Manager.OffsetX = X;
                Manager.OffsetY = Y;
                NumberPlayers.Value = Players;
                NumberBombs.Value = Bombs;
                Manager.TickAll();
                if (pux < 432 && user.Y >= 432) Popdown();
                if (pux >= 432 && user.Y < 432) Popup();
                Operation = (Operation != null && Operation.MoveNext() && Operation.Current) ? Operation : null;
                pux = user.Y;
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 100);
                DX.DrawBox((int)X, (int)Y, (int)(X + 640), (int)(Y + 48), BackColor, DX.TRUE);

                Manager.DrawAll();
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
