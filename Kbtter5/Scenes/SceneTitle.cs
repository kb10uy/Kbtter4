using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;
using EasingSharp;
using Kbtter4.Models;
using CoreTweet;
using System.IO;
using System.Net;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Kbtter5.Scenes
{
    class SceneTitle : Scene
    {
        private Kbtter5 Kbtter5 = Kbtter5.Instance;
        private IEnumerator<bool> Operation;
        private GamepadState state, tstate, prevstate;
        private AdditionalCoroutineSprite[] menu;
        private int selmenu = 0;
        private int modestate = 0;
        private MultiAdditionalCoroutineSprite[] lrc;

        private AccountInformation info;

        public SceneTitle()
        {
            Operation = Execute();
            var path = Path.Combine(CommonObjects.DataDirectory, "user");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public IEnumerator<bool> Execute()
        {
            DX.SetDrawMode(DX.DX_DRAWMODE_BILINEAR);
            info = new AccountInformation(Kbtter.Instance.Setting.Accounts.ToArray());
            //The Kbtter Project
            var kbp = new Sprite() { Image = CommonObjects.ImageKbtterProject, HomeX = 256, HomeY = 32, X = 320, Y = 240 };
            Manager.Add(kbp, 0);
            for (int i = 0; i < 120; i++)
            {
                kbp.ScaleX = kbp.ScaleY = Easing.OutBounce(i, 120, 4, -3);
                kbp.Alpha = Easing.OutSine(i, 120, 0, 1);
                yield return true;
            }
            kbp.ScaleX = kbp.ScaleY = 1;
            kbp.Alpha = 1;
            for (int i = 0; i < 120; i++) yield return true;
            for (int i = 0; i < 60; i++)
            {
                kbp.Alpha = Easing.OutSine(i, 60, 1, -1);
                yield return true;
            }
            kbp.IsDead = true;

            //タイトル画面
            var back = new RectangleObject() { Width = 640, Height = 480, Color = CommonObjects.Colors.White, AllowFill = true };
            Manager.Add(back, 0);
            for (int i = 0; i < 60; i++)
            {
                back.Alpha = Easing.OutSine(i, 60, 0, 1);
                yield return true;
            }
            back.Alpha = 1;

            var logo = new Sprite() { Image = CommonObjects.ImageLogo, HomeX = 240, HomeY = 80, X = 320 };
            Manager.Add(logo, 1);
            for (int i = 0; i < 60; i++)
            {
                logo.Y = Easing.OutBounce(i, 60, -160, 320);
                yield return true;
            }

            //Press Z
            var pz = new StringSprite(CommonObjects.FontSystemBig, CommonObjects.Colors.Black) { Value = "Press Button", X = 320, Y = 400, HomeX = 160 };
            var pzdx = DX.GetDrawStringWidthToHandle("Press Button", 12, CommonObjects.FontSystemBig);
            pz.HomeX = pzdx / 2;
            Manager.Add(pz, 1);
            do
            {
                for (int i = 0; i < 30 && !tstate.Buttons.Any(p => p); i++)
                {
                    pz.Alpha = Easing.Linear(i, 30, 0, 1);
                    yield return true;
                }
                for (int i = 0; i < 30 && !tstate.Buttons.Any(p => p); i++)
                {
                    pz.Alpha = Easing.Linear(i, 30, 1, -1);
                    yield return true;
                }
            } while (!tstate.Buttons.Any(p => p));
            var st = pz.Alpha;
            for (int i = 0; i < 30; i++)
            {
                pz.Alpha = Easing.Linear(i, 30, st, -st);
                yield return true;
            }
            pz.IsDead = true;


            //menu
            menu = new[] 
            {
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=64,Image=CommonObjects.ImageTitleMenuStart,Y=720},
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=64,Image=CommonObjects.ImageTitleMenuQuick,Y=720},
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=64,Image=CommonObjects.ImageTitleMenuOption,Y=720},
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=64,Image=CommonObjects.ImageTitleMenuRanking,Y=720},
            };
            Manager.AddRangeTo(menu, 1);
            for (int i = 0; i < menu.Length; i++)
            {
                menu[i].X = 320 + 256 * i;
                if (i != selmenu)
                {
                    menu[i].Alpha = 0.5;
                    menu[i].ScaleX = menu[i].ScaleY = 0.8;
                }
                menu[i].ApplyOperation(SpritePatterns.MenuIntro(i * 15, 60, 400));
            }

            lrc = new[] 
            {
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.5,ScaleY=0.8,Image=CommonObjects.ImageCursor128[0],X=120,Y=400},
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.5,ScaleY=0.8,Image=CommonObjects.ImageCursor128[1],X=520,Y=400},
            };
            Manager.AddRangeTo(lrc, 2);
            foreach (var i in lrc) i.AddOperation(SpritePatterns.Blink(30, 0.8, Easing.Linear));

            while (true)
            {
                switch (modestate)
                {
                    case 0:
                        //メニュー選択
                        while (!menu.All(p => p.SpecialOperation == null)) yield return true;
                        while (true)
                        {
                            if ((tstate.Direction & GamepadDirection.Left) != 0)
                            {
                                selmenu = (selmenu + menu.Length - 1) % menu.Length;
                                RefreshMenuPosition();
                            }
                            if ((tstate.Direction & GamepadDirection.Right) != 0)
                            {
                                selmenu = (selmenu + 1) % menu.Length;
                                RefreshMenuPosition();
                            }
                            if (tstate.Buttons[0])
                            {
                                //救済措置
                                if (selmenu == 0)
                                {
                                    for (int i = 0; i < menu.Length; i++)
                                    {
                                        menu[i].ApplyOperation(SpritePatterns.MenuIntro(0, 60, 600));
                                    }
                                    foreach (var i in lrc) i.AddOperation(SpritePatterns.MenuOutro(0, 60, 600));
                                    break;
                                }
                            }
                            yield return true;
                        }
                        switch (selmenu)
                        {
                            case 0:
                                //Start
                                Children.AddChildScene(new TitleChildSceneAccountSelect(info));
                                break;
                            case 1:
                                //Quick
                                break;
                            case 2:
                                //Option
                                break;
                            case 3:
                                //Ranking
                                break;
                        }
                        modestate = 1;
                        break;
                    case 1:
                        //サブタスク動作
                        yield return true;
                        break;
                }
            }

            while (true) yield return true;
        }

        public override void SendChildMessage(string mes)
        {
            switch (mes)
            {
                case "ReturnToMenuSelect":
                    modestate = 0;
                    for (int i = 0; i < menu.Length; i++)
                    {
                        menu[i].ApplyOperation(SpritePatterns.MenuIntro(i * 15, 60, 400));
                    }
                    foreach (var i in lrc) i.AddOperation(SpritePatterns.MenuOutro(0, 60, 400));
                    break;
            }
        }

        public void RefreshMenuPosition()
        {
            for (int i = 0; i < menu.Length; i++)
            {
                if (i == selmenu)
                {
                    menu[i].ApplyOperation(SpritePatterns.MenuEnable());
                }
                else
                {
                    menu[i].ApplyOperation(SpritePatterns.MenuDisable(320 + 256 * (i - selmenu)));
                }
            }
        }

        public override IEnumerator<bool> Tick()
        {
            prevstate = Gamepad.GetState();
            while (true)
            {
                state = Gamepad.GetState();
                tstate = state.GetTriggerStateWith(prevstate);
                Operation.MoveNext();
                Manager.TickAll();
                Children.TickAll();
                prevstate = Gamepad.GetState();
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                Manager.DrawAll();
                Children.DrawAll();
                yield return true;
            }
        }
    }

    public class TitleChildSceneAccountSelect : ChildScene
    {
        private Kbtter Kbtter = Kbtter.Instance;
        private GamepadState state, tstate, prevstate;
        private MultiAdditionalCoroutineSprite[] udc;
        private AccountInformation ainfo;
        private UserInformation[] uinfo;
        private UserInformationPanel[] uips;

        public TitleChildSceneAccountSelect(AccountInformation ai)
        {
            ainfo = ai;
        }

        public override IEnumerator<bool> Execute()
        {
            Manager.OffsetX = 640;
            Manager.OffsetY = 240;

            if (ainfo.Accounts.Length == 0)
            {
                Manager.Add(new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Red) { X = 16, Y = 120, Value = "アカウントがありません。Kbtter4で登録してください。" }, 2);
                for (int i = 0; i < 40; i++)
                {
                    Manager.OffsetX = Easing.OutQuad(i, 40, 640, -640);
                    yield return true;
                }
                Manager.OffsetX = 0;
                while (true) yield return true;
            }

            LoadUsers();
            var sel = 0;
            Manager.AddRangeTo(uips, 1);
            for (int i = 0; i < uips.Length; i++)
            {
                uips[i].X = 640 * i;
            }

            Manager.Add(new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Black) { Value = "アカウント選択", X = 8, Y = 8 }, 0);
            udc = new[] 
            {
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.8,ScaleY=0.4,Image=CommonObjects.ImageCursor128[2],X=64,Y=48},
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.8,ScaleY=0.4,Image=CommonObjects.ImageCursor128[3],X=64,Y=208},
            };
            Manager.AddRangeTo(udc, 2);
            foreach (var i in udc) i.AddOperation(SpritePatterns.Blink(30, 0.8, Easing.Linear));

            for (int i = 0; i < 40; i++)
            {
                Manager.OffsetX = Easing.OutQuad(i, 40, 640, -640);
                yield return true;
            }
            Manager.OffsetX = 0;

            prevstate = Gamepad.GetState();
            while (true)
            {
                state = Gamepad.GetState();
                tstate = state.GetTriggerStateWith(prevstate);
                if (tstate.Buttons[1])
                {
                    Parent.SendChildMessage("ReturnToMenuSelect");
                    for (int i = 0; i < 40; i++)
                    {
                        Manager.OffsetX = Easing.OutQuad(i, 40, 0, 640);
                        yield return true;
                    }
                    break;
                }

                prevstate = Gamepad.GetState();
                yield return true;
            }
        }

        public void LoadUsers()
        {
            uinfo = new UserInformation[ainfo.Accounts.Length];
            uips = new UserInformationPanel[ainfo.Accounts.Length];
            for (int i = 0; i < uips.Length; i++) uips[i] = new UserInformationPanel();

            for (int i = 0; i < ainfo.Accounts.Length; i++)
            {
                if (!ainfo.HasGot[i])
                {
                    ainfo.HasGot[i] = true;
                    var t = Tokens.Create(
                        Kbtter.Instance.Setting.Consumer.Key,
                        Kbtter.Instance.Setting.Consumer.Secret,
                        ainfo.Accounts[i].AccessToken,
                        ainfo.Accounts[i].AccessTokenSecret);
                    var ta = i;
                    t.Users.ShowAsync(user_id => ainfo.Accounts[ta].UserId)
                        .ContinueWith(p =>
                        {
                            if (p.IsFaulted) return;
                            ainfo.Users[ta] = p.Result;
                            ainfo.HasGot[ta] = true;
                            uinfo[ta] = new UserInformation(p.Result);
                            uips[ta].RefreshUserInfo(uinfo[ta]);
                        });
                }
                else
                {
                    uinfo[i] = new UserInformation(ainfo.Users[i]);
                    uips[i].RefreshUserInfo(uinfo[i]);
                }
            }
        }
    }

    public class UserInformation
    {
        public User SourceUser { get; private set; }
        public int ShotStrength { get; private set; }
        public int BombStrength { get; private set; }
        public int DefaultPlayers { get; private set; }
        public int DefaultBombs { get; private set; }
        public int GrazePoints { get; private set; }
        public double CollisionRadius { get; private set; }

        public UserInformation(User user)
        {
            SourceUser = user;
            ShotStrength = (SourceUser.StatusesCount + (DateTime.Now - SourceUser.CreatedAt.LocalDateTime).Days * (int)Math.Log10(SourceUser.StatusesCount)) / 25;
            GrazePoints = (SourceUser.StatusesCount / SourceUser.FollowersCount) / 20 + 10;
            CollisionRadius = 4.0 * (SourceUser.FriendsCount / SourceUser.FollowersCount);
            DefaultPlayers = (int)(Math.Log10(SourceUser.FollowersCount) * Math.Log10(SourceUser.FriendsCount)) * 4;
            DefaultBombs = (int)(Math.Log10(SourceUser.FavouritesCount) + Math.Log10(SourceUser.StatusesCount)) * 2;
            BombStrength = SourceUser.StatusesCount;
        }
    }

    public class UserInformationPanel : MultiAdditionalCoroutineSprite
    {
        private UserInformation info;
        private ObjectManager Manager;
        private StringSprite username, shots, bombs, defp, defb, grep, colr, u_tw, u_fav, u_fr, u_fo;
        private Sprite uim;

        public UserInformationPanel()
        {
            Manager = new ObjectManager(2);
            username = new StringSprite(CommonObjects.FontSystemLarge, CommonObjects.Colors.Blue) { Value = "Loading...", X = 128, Y = 40 };
            shots = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { Value = "Loading...", X = 256, Y = 80 };
            bombs = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { Value = "Loading...", X = 256, Y = 104 };
            defp = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { Value = "Loading...", X = 256, Y = 128 };
            defb = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { Value = "Loading...", X = 256, Y = 152 };
            grep = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { Value = "Loading...", X = 256, Y = 176 };
            colr = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { Value = "Loading...", X = 256, Y = 200 };

            u_tw = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "Loading...", X = 512, Y = 80 };
            u_fav = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "Loading...", X = 512, Y = 104 };
            u_fr = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "Loading...", X = 512, Y = 128 };
            u_fo = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "Loading...", X = 512, Y = 152 };

            uim = new Sprite() { HomeX = 48, HomeY = 48, X = 64, Y = 128 };
        }

        public void RefreshUserInfo(UserInformation i)
        {
            info = i;
            username.Value = info.SourceUser.ScreenName;
            shots.Value = info.ShotStrength.ToString();
            bombs.Value = info.BombStrength.ToString();
            defp.Value = info.DefaultPlayers.ToString();
            defb.Value = info.DefaultBombs.ToString();
            grep.Value = info.GrazePoints.ToString();
            colr.Value = info.CollisionRadius.ToString();

            u_tw.Value = info.SourceUser.StatusesCount.ToString();
            u_fav.Value = info.SourceUser.FavouritesCount.ToString();
            u_fr.Value = info.SourceUser.FriendsCount.ToString();
            u_fo.Value = info.SourceUser.FollowersCount.ToString();

            var target = Path.Combine(
                            CommonObjects.DataDirectory,
                            "user",
                            string.Format("{0}.{1}", info.SourceUser.Id, "png"));
            if (!File.Exists(target))
            {
                using (var wc = new WebClient())
                using (var st = wc.OpenRead(info.SourceUser.ProfileImageUrlHttps.ToString().Replace("_normal.png", ".png")))
                {
                    var bm = new Bitmap(st);
                    var sav = new Bitmap(bm, 96, 96);
                    try
                    {
                        sav.Save(target);
                    }
                    catch
                    {

                    }
                }
            }
            uim.Image = DX.LoadGraph(target);

        }

        public override IEnumerator<bool> Tick()
        {

            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "ショット威力", X = 128, Y = 80 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "ボム威力", X = 128, Y = 104 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "初期残機数", X = 128, Y = 128 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "初期ボム数", X = 128, Y = 152 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "グレイズ得点", X = 128, Y = 176 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "自機判定半径", X = 128, Y = 200 }, 0);

            Manager.Add(uim, 0);
            Manager.Add(username, 0);
            Manager.Add(shots, 0);
            Manager.Add(bombs, 0);
            Manager.Add(defp, 0);
            Manager.Add(defb, 0);
            Manager.Add(grep, 0);
            Manager.Add(colr, 0);

            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "ツイート", X = 400, Y = 80 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "お気に入り", X = 400, Y = 104 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "フォロー", X = 400, Y = 128 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "フォロワー", X = 400, Y = 152 }, 0);

            Manager.Add(u_tw, 0);
            Manager.Add(u_fav, 0);
            Manager.Add(u_fr, 0);
            Manager.Add(u_fo, 0);

            while (true)
            {

                Manager.TickAll();
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                Manager.OffsetX = ActualX;
                Manager.OffsetY = ActualY;
                Manager.DrawAll();
                yield return true;
            }
        }
    }

    public class AccountInformation
    {
        public Kbtter4Account[] Accounts { get; private set; }
        public User[] Users { get; private set; }
        public bool[] HasGot { get; private set; }

        public AccountInformation(IEnumerable<Kbtter4Account> acs)
        {
            Accounts = acs.ToArray();
            Users = new User[Accounts.Length];
            HasGot = new bool[Accounts.Length];
        }
    }
}
