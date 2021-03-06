﻿using System;
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
    #region SceneTitle
    public class SceneTitle : Scene
    {
        private Kbtter5 Kbtter5 = Kbtter5.Instance;
        private IEnumerator<bool> Operation;
        private GamepadState state, tstate, prevstate;
        private AdditionalCoroutineSprite[] menu;
        private int selmenu = 0;
        private int modestate = 0;
        private int selac = 0;
        private MultiAdditionalCoroutineSprite[] lrc;
        private User[] optusers;
        private OptionInformation[] optinfo;

        private AccountInformation info;

        public SceneTitle()
        {
            Operation = Execute();
            var path = Path.Combine(CommonObjects.DataDirectory, "user");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            optusers = new User[5];
            optinfo = new OptionInformation[5];
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
            CommonObjects.SoundMenuOK.Play();
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
                menu[i].ApplySpecialOperation(SpritePatterns.MenuIntro(i * 15, 60, 400));
            }

            lrc = new[] 
            {
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.5,ScaleY=0.8,Image=CommonObjects.ImageCursor128[0],X=120,Y=400},
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.5,ScaleY=0.8,Image=CommonObjects.ImageCursor128[1],X=520,Y=400},
            };
            Manager.AddRangeTo(lrc, 2);
            foreach (var i in lrc) i.AddSubOperation(SpritePatterns.Blink(30, 0.8, Easing.Linear));

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
                                CommonObjects.SoundMenuSelect.Play();
                            }
                            if ((tstate.Direction & GamepadDirection.Right) != 0)
                            {
                                selmenu = (selmenu + 1) % menu.Length;
                                RefreshMenuPosition();
                                CommonObjects.SoundMenuSelect.Play();
                            }
                            if (tstate.Buttons[0])
                            {
                                //救済措置
                                if (selmenu == 0)
                                {
                                    CommonObjects.SoundMenuOK.Play();
                                    for (int i = 0; i < menu.Length; i++)
                                    {
                                        menu[i].ApplySpecialOperation(SpritePatterns.MenuIntro(0, 60, 600));
                                    }
                                    foreach (var i in lrc) i.AddSubOperation(SpritePatterns.MenuOutro(0, 60, 600));
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
                    case 2:
                        for (int i = 0; i < 60; i++)
                        {
                            Manager.Alpha -= 1.0 / 60.0;
                            yield return true;
                        }
                        Manager.Alpha = 0;
                        goto EXIT;
                }
            }
        EXIT:
            Kbtter5.CurrentScene = new SceneGame(info.Accounts[selac], new UserInformation(info.Users[selac]), optinfo);
        }

        public override void SendChildMessage(string mes)
        {
            switch (mes)
            {
                case "ReturnToMenuSelect":
                    modestate = 0;
                    for (int i = 0; i < menu.Length; i++)
                    {
                        menu[i].ApplySpecialOperation(SpritePatterns.MenuIntro(i * 15, 60, 400));
                    }
                    foreach (var i in lrc) i.AddSubOperation(SpritePatterns.MenuOutro(0, 60, 400));
                    return;
                case "ReturnToAccountSelect":
                    modestate = 1;
                    Children.AddChildScene(new TitleChildSceneAccountSelect(info, true));
                    return;
                case "GoToOptionSetting":
                    modestate = 1;
                    Children.AddChildScene(new TitleChildSceneOptionEdit(optusers));
                    return;
                case "ReturnToOptionEdit":
                    Children.AddChildScene(new TitleChildSceneOptionUserSelect(info, selac, true, optusers));
                    return;
                case "StartGame":
                    modestate = 2;
                    return;
            }

            var l = mes.Split(':');
            var args = l[1].Split(',');
            switch (l[0])
            {
                case "GoToOptionEdit":
                    modestate = 1;
                    selac = Convert.ToInt32(args[0]);
                    Children.AddChildScene(new TitleChildSceneOptionUserSelect(info, selac, false, optusers));
                    return;
            }
        }

        public override void SendChildMessage(string mes, object obj)
        {
            var tg = mes.Split(':');
            switch (tg[0])
            {
                case "EntryOption":
                    var ui = obj as User;
                    optusers[Convert.ToInt32(tg[1])] = ui;
                    optinfo[Convert.ToInt32(tg[1])] = new OptionInformation(ui);
                    break;
                case "ApplyOptionInformation":
                    var ol = obj as OptionInformation[];
                    optinfo = ol;
                    break;
            }
        }

        public void RefreshMenuPosition()
        {
            for (int i = 0; i < menu.Length; i++)
            {
                if (i == selmenu)
                {
                    menu[i].ApplySpecialOperation(SpritePatterns.MenuEnable());
                }
                else
                {
                    menu[i].ApplySpecialOperation(SpritePatterns.MenuDisable(320 + 256 * (i - selmenu)));
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
                prevstate = state;
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
    #endregion

    #region TitleChildSceneAccountSelect
    public class TitleChildSceneAccountSelect : ChildScene
    {
        private Kbtter Kbtter = Kbtter.Instance;
        private GamepadState state, tstate, prevstate;
        private MultiAdditionalCoroutineSprite[] udc;
        private AccountInformation ainfo;
        private Tokens[] tokens;
        private UserInformation[] uinfo;
        private UserInformationPanel[] uips;
        private bool backing;

        public TitleChildSceneAccountSelect(AccountInformation ai)
        {
            ainfo = ai;
        }

        public TitleChildSceneAccountSelect(AccountInformation ai, bool back)
        {
            ainfo = ai;
            backing = back;
        }

        public override IEnumerator<bool> Execute()
        {
            Manager.OffsetX = backing ? -640 : 640;
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
                uips[i].Y = 240 * i;
                if (i != sel) uips[i].Alpha = 0;
            }

            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "アカウント選択", X = 250, Y = 8 }, 0);
            udc = new[] 
            {
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.8,ScaleY=0.4,Image=CommonObjects.ImageCursor128[2],X=64,Y=48},
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.8,ScaleY=0.4,Image=CommonObjects.ImageCursor128[3],X=64,Y=208},
            };
            Manager.AddRangeTo(udc, 2);
            foreach (var i in udc) i.AddSubOperation(SpritePatterns.Blink(30, 0.8, Easing.Linear));

            for (int i = 0; i < 40; i++)
            {
                Manager.OffsetX = Easing.OutQuad(i, 40, backing ? -640 : 640, backing ? 640 : -640);
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
                    CommonObjects.SoundMenuCancel.Play();
                    for (int i = 0; i < 40; i++)
                    {
                        Manager.OffsetX = Easing.OutQuad(i, 40, 0, 640);
                        yield return true;
                    }
                    break;
                }
                if (tstate.Buttons[0] && uinfo[sel] != null)
                {
                    Parent.SendChildMessage("GoToOptionEdit:" + sel.ToString());
                    CommonObjects.SoundMenuOK.Play();
                    for (int i = 0; i < 40; i++)
                    {
                        Manager.OffsetX = Easing.OutQuad(i, 40, 0, -640);
                        yield return true;
                    }
                    break;
                }
                if ((tstate.Direction & GamepadDirection.Up) != 0)
                {
                    uips[sel].AddSubOperation(SpritePatterns.Move(15, 0, -120, Easing.OutQuad));
                    uips[sel].AddSubOperation(SpritePatterns.Alpha(15, 0, Easing.OutQuad));
                    sel = (sel + uips.Length - 1) % uips.Length;
                    uips[sel].Y = 120;
                    uips[sel].AddSubOperation(SpritePatterns.Move(15, 0, 0, Easing.OutQuad));
                    uips[sel].AddSubOperation(SpritePatterns.Alpha(15, 1, Easing.OutQuad));
                    CommonObjects.SoundMenuSelect.Play();

                }
                if ((tstate.Direction & GamepadDirection.Down) != 0)
                {
                    uips[sel].AddSubOperation(SpritePatterns.Move(15, 0, 120, Easing.OutQuad));
                    uips[sel].AddSubOperation(SpritePatterns.Alpha(15, 0, Easing.OutQuad));
                    sel = (sel + 1) % uips.Length;
                    uips[sel].Y = -120;
                    uips[sel].AddSubOperation(SpritePatterns.Move(15, 0, 0, Easing.OutQuad));
                    uips[sel].AddSubOperation(SpritePatterns.Alpha(15, 1, Easing.OutQuad));
                    CommonObjects.SoundMenuSelect.Play();
                }

                prevstate = state;
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
                    ainfo.Tokens[i] = t;
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

            var target = CommonObjects.GetUserFilePath(string.Format("{0}.{1}", info.SourceUser.Id, "png"));
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
                ProcessOperations();
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                Manager.OffsetX = ActualX;
                Manager.OffsetY = ActualY;
                Manager.Alpha = ActualAlpha;
                Manager.DrawAll();
                yield return true;
            }
        }
    }
    #endregion

    #region TitleChildSceneOptionSelect
    public class TitleChildSceneOptionUserSelect : ChildScene
    {
        AccountInformation ai;
        UserInformation uinfo;
        private GamepadState state, tstate, prevstate;
        int index, msel, ocmsel, cs, opopmsel;
        List<MenuAllocationInformation> mal, ocmal, opopmal;
        MultiAdditionalCoroutineSprite mc;
        OptionUserInformationPanel ouip;
        KeyInputObject ki;
        List<StringSprite> okcancel = new List<StringSprite>(), opod, opsn;
        Func<IReadOnlyList<StringSprite>, int, int, Action<MenuAllocationInformation, bool>> mvf;
        StringSprite valid, next, opop;
        User[] users;
        double nextpos = 272;
        double opsnpos = 224 - 8;
        bool b;

        public TitleChildSceneOptionUserSelect(AccountInformation ainfo, int idx, bool back, User[] us)
        {
            b = back;
            ai = ainfo;
            uinfo = new UserInformation(ai.Users[idx]);
            index = idx;
            mvf = (ta, aid, max) => (mai, val) =>
            {
                if (max > aid) ta[aid].Alpha = val ? 1.0 : 0.5;
            };
            users = us;
            opod = new List<StringSprite>
            {
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 64, Y = 64 - 8, Value = "1st" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 64, Y = 96 - 8, Value = "2nd" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 64, Y = 128 - 8, Value = "3rd" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 64, Y = 160 - 8, Value = "4th" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 64, Y = 192 - 8, Value = "5th" }
            };
            next = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { X = 64, Y = nextpos - 8, Value = "決定" };
            opsn = new List<StringSprite>
            {
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 128, Y = 64 - 8, Value = "" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 128, Y = 96 - 8, Value = "" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 128, Y = 128 - 8, Value = "" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 128, Y = 160 - 8, Value = "" },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 128, Y = 192 - 8, Value = "" }
            };
            mal = new List<MenuAllocationInformation> 
            {
                new MenuAllocationInformation(){ X = 32, Y = 64 },
                new MenuAllocationInformation(){ X = 32, Y = 96 },
                new MenuAllocationInformation(){ X = 32, Y = 128 },
                new MenuAllocationInformation(){ X = 32, Y = 160 },
                new MenuAllocationInformation(){ X = 32, Y = 192 },
                new MenuAllocationInformation(){ X = 32, Y = 224 },
            };
            opop = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 64, Y = 224 - 8, Value = "編集        削除" };
            opopmal = new List<MenuAllocationInformation>
            {
                new MenuAllocationInformation(){X=40,Y=224},
                new MenuAllocationInformation(){X=136,Y=224},
            };

            okcancel = new List<StringSprite>
            {
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "OK", X = 528, Y = 180 , Alpha = 0},
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "キャンセル", X = 528, Y = 204, Alpha = 0 }
            };
            ocmal = new List<MenuAllocationInformation> 
            {
                new MenuAllocationInformation(){X=512,Y=180+8,IsAvailable=false},
                new MenuAllocationInformation(){X=512,Y=204+8},
            };
            ouip = new OptionUserInformationPanel(ai.Tokens[index], ai.Users[index])
            {
                ChangingAction = (p) =>
                {
                    //タイムラグ対策
                    if (cs != 2) return;
                    if (p && users.Where(q => q != null).All(q => q.Id != ouip.SourceUser.Id))
                    {
                        valid.Value = "オプションOK";
                        valid.Color = CommonObjects.Colors.Blue;
                        ocmal[0].IsAvailable = true;
                    }
                    else
                    {
                        valid.Value = "オプションNG";
                        valid.Color = CommonObjects.Colors.Red;
                        ocmsel = 1;
                        ocmal[0].IsAvailable = false;
                        mc.AddSubOperation(SpritePatterns.VerticalMove(10, ocmal[ocmsel].Y, Easing.OutQuad));
                    }
                }
            };
            for (int i = 0; i < opopmal.Count; i++)
            {
                opopmal[i].Lefter = opopmal[(i + opopmal.Count - 1) % opopmal.Count];
                opopmal[i].Righter = opopmal[(i + 1) % opopmal.Count];
            }
            for (int i = 0; i < ocmal.Count; i++)
            {
                ocmal[i].Upper = ocmal[(i + ocmal.Count - 1) % ocmal.Count];
                ocmal[i].Lower = ocmal[(i + 1) % ocmal.Count];
                ocmal[i].AvailableChangingAction = mvf(okcancel, i, okcancel.Count);
            }
            for (int i = 0; i < mal.Count; i++)
            {
                mal[i].Upper = mal[(i + mal.Count - 1) % mal.Count];
                mal[i].Lower = mal[(i + 1) % mal.Count];
                mal[i].AvailableChangingAction = mvf(opod, i, opod.Count);
                mal[i].IsAvailable = (i < users.Length && users[i] != null) || i == mal.Count - 1 || (users.Count(p => p != null) == i);
            }
            for (int i = 0; i < opsn.Count; i++)
            {
                if (users[i] != null) opsn[i].Value = users[i].ScreenName;
            }

            valid = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 320, Y = 216, Value = "オプションOK/NG" };
            mc = new MultiAdditionalCoroutineSprite() { Image = CommonObjects.ImageCursor128[1], HomeX = 64, HomeY = 64, ScaleX = 0.25, ScaleY = 0.25 };
            cs = 0;
            ki = null;
            msel = 0;
            ocmsel = 0;
        }

        public override IEnumerator<bool> Execute()
        {
            Manager.OffsetX = 640;
            Manager.OffsetY = 240;

            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "オプションユーザー選択", X = 210, Y = 8 }, 0);
            Manager.AddRangeTo(opod, 0);
            Manager.AddRangeTo(opsn, 0);
            Manager.Add(opop, 0);
            Manager.Add(next, 0);

            Manager.Add(new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { X = 320, Y = 64 - 8, Value = "検索:" }, 1);
            Manager.Add(valid, 0);

            mc.AddSubOperation(SpritePatterns.Blink(30, 0.5, Easing.Linear));
            Manager.Add(mc, 1);
            mc.X = mal[msel].X;
            mc.Y = mal[msel].Y;

            Manager.AddRangeTo(okcancel, 1);
            Manager.Add(ouip, 1);

            //突入
            for (int i = 0; i < 40; i++)
            {
                Manager.OffsetX = Easing.OutQuad(i, 40, b ? -640 : 640, b ? 640 : -640);
                yield return true;
            }
            Manager.OffsetX = 0;


            prevstate = Gamepad.GetState();
            while (true)
            {
                state = Gamepad.GetState();
                tstate = state.GetTriggerStateWith(prevstate);

                switch (cs)
                {
                    case 0:
                        if (tstate.Buttons[1])
                        {
                            Parent.SendChildMessage("ReturnToAccountSelect");
                            CommonObjects.SoundMenuCancel.Play();
                            for (int i = 0; i < 40; i++)
                            {
                                Manager.OffsetX = Easing.OutQuad(i, 40, 0, 640);
                                yield return true;
                            }
                            goto EXIT;
                        }
                        if (tstate.Buttons[0])
                        {
                            CommonObjects.SoundMenuOK.Play();
                            if (msel < 5)
                            {
                                GoToOptionOperationSelect();
                            }
                            else
                            {
                                Parent.SendChildMessage("GoToOptionSetting");
                                for (int i = 0; i < 40; i++)
                                {
                                    Manager.OffsetX = Easing.OutQuad(i, 40, 0, -640);
                                    yield return true;
                                }
                                goto EXIT;
                            }
                        }
                        OptionUserOrderSelectionOperation();
                        break;
                    case 1:
                        OptionScreenNameInputOperation();
                        break;
                    case 2:
                        OptionDecisionOperation();
                        break;
                    case 3:
                        //本当はScreenNameInputの前だけどゆるして
                        OptionOperationSelectionOperation();
                        break;
                }
                prevstate = state;
                yield return true;
            }
        EXIT: ;
        }

        public void OptionUserOrderSelectionOperation()
        {
            if ((tstate.Direction & GamepadDirection.Up) != 0)
            {
                int ps = msel;
                do
                {
                    var tm = mal.IndexOf(mal[msel].Upper);
                    if (tm != -1) msel = tm;
                } while (!mal[msel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.VerticalMove(10, mal[msel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
                CheckNextTextPosition(ps);
                return;
            }
            if ((tstate.Direction & GamepadDirection.Down) != 0)
            {
                int ps = msel;
                do
                {
                    var tm = mal.IndexOf(mal[msel].Lower);
                    if (tm != -1) msel = tm;
                } while (!mal[msel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.VerticalMove(10, mal[msel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
                CheckNextTextPosition(ps);
                return;
            }
        }

        private void CheckNextTextPosition(int ps)
        {
            if ((ps != mal.Count - 1 && msel == mal.Count - 1)/*||(msel != mal.Count - 1 && ps == mal.Count - 1)*/)
            {
                opop.AddSubOperation(SpritePatterns.VerticalMove(15, nextpos, Easing.OutQuad));
                next.AddSubOperation(SpritePatterns.VerticalMove(15, opsnpos, Easing.OutQuad));
            }
            else
            {
                opop.AddSubOperation(SpritePatterns.VerticalMove(15, opsnpos, Easing.OutQuad));
                next.AddSubOperation(SpritePatterns.VerticalMove(15, nextpos, Easing.OutQuad));
            }
        }

        public void OptionOperationSelectionOperation()
        {
            if (tstate.Buttons[0])
            {
                if (opopmsel == 0)
                {
                    GoToScreenNameInput();
                }
                else
                {

                }
                return;
            }
            if (tstate.Buttons[1])
            {
                GoToNumberSelect();
                return;
            }
            if ((tstate.Direction & GamepadDirection.Left) != 0)
            {
                do
                {
                    var tm = opopmal.IndexOf(opopmal[opopmsel].Lefter);
                    if (tm != -1) opopmsel = tm;
                } while (!opopmal[opopmsel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.Move(10, opopmal[opopmsel].X, opopmal[opopmsel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
                return;
            }
            if ((tstate.Direction & GamepadDirection.Right) != 0)
            {
                do
                {
                    var tm = opopmal.IndexOf(opopmal[opopmsel].Righter);
                    if (tm != -1) opopmsel = tm;
                } while (!opopmal[opopmsel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.Move(10, opopmal[opopmsel].X, opopmal[opopmsel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
                return;
            }
        }

        public void OptionScreenNameInputOperation()
        {
            if (ki.IsCanceled)
            {
                ki.Dispose();
                GoToOptionOperationSelect();
            }
            else if (ki.HasCompleted)
            {
                GoToOptionCheck();
            }
        }

        public void OptionDecisionOperation()
        {
            if ((tstate.Direction & GamepadDirection.Up) != 0)
            {
                do
                {
                    var tm = ocmal.IndexOf(ocmal[ocmsel].Upper);
                    if (tm != -1) ocmsel = tm;
                } while (!ocmal[ocmsel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.VerticalMove(10, ocmal[ocmsel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
                return;
            }
            if ((tstate.Direction & GamepadDirection.Down) != 0)
            {
                do
                {
                    var tm = ocmal.IndexOf(ocmal[ocmsel].Lower);
                    if (tm != -1) ocmsel = tm;
                } while (!ocmal[ocmsel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.VerticalMove(10, ocmal[ocmsel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
                return;
            }
            if (tstate.Buttons[1])
            {
                GoToScreenNameInput();
                CommonObjects.SoundMenuCancel.Play();
            }
            if (tstate.Buttons[0])
            {
                CommonObjects.SoundMenuOK.Play();
                switch (ocmsel)
                {
                    case 0:
                        Parent.SendChildMessage("EntryOption:" + msel.ToString(), ouip.SourceUser);
                        opsn[msel].Value = ouip.SourceUser.ScreenName;
                        users[msel] = ouip.SourceUser;
                        if (msel + 1 < 5) mal[msel + 1].IsAvailable = true;
                        GoToNumberSelect();
                        break;
                    case 1:
                        GoToScreenNameInput();
                        break;
                }
                return;
            }
        }

        public void GoToOptionOperationSelect()
        {
            cs = 3;
            mc.AddSubOperation(SpritePatterns.Move(10, opopmal[opopmsel].X, opopmal[opopmsel].Y, Easing.OutQuad));
        }

        private void GoToNumberSelect()
        {
            cs = 0;
            okcancel.ForEach(p => p.Alpha = 0);
            mc.AddSubOperation(SpritePatterns.Move(10, mal[msel].X, mal[msel].Y, Easing.OutQuad));
        }

        private void GoToOptionCheck()
        {
            cs = 2;
            ki.Dispose();
            okcancel.ForEach(p => p.Alpha = 1);
            ouip.SearchUser(ki.InputString);
            ocmsel = 1;
            mc.AddSubOperation(SpritePatterns.Move(10, ocmal[ocmsel].X, ocmal[ocmsel].Y, Easing.OutQuad));
        }

        private void GoToScreenNameInput()
        {
            cs = 1;
            mc.AddSubOperation(SpritePatterns.Move(10, 300, 64, Easing.OutQuad));
            ki = new KeyInputObject(CommonObjects.FontSystemMedium, 20, true, true, false)
            {
                X = 376,
                Y = 64 - 8,
            };
            Manager.Add(ki, 1);
            okcancel.ForEach(p => p.Alpha = 0);
        }
    }

    public class OptionUserInformationPanel : ExpandableDisplayObject
    {
        StringSprite sname, tweets, fav, friend, follower;
        Sprite img;
        public User SourceUser { get; protected set; }
        public bool IsValidUser { get; protected set; }
        private Tokens t;
        private User m;
        public Action<bool> ChangingAction { get; set; }

        public OptionUserInformationPanel(Tokens ut, User me)
        {
            t = ut;
            m = me;
        }

        public void SearchUser(string sn)
        {
            sname.Value = sn;
            IsValidUser = true;
            ChangingAction(false);
            t.Users.ShowAsync(screen_name => sn)
                .ContinueWith(p =>
                {
                    if (p.IsFaulted)
                    {
                        sname.Value = "Not Found";
                        IsValidUser = false;
                        return;
                    }
                    var u = p.Result;
                    SourceUser = u;
                    tweets.Value = SourceUser.StatusesCount.ToString();
                    fav.Value = SourceUser.FavouritesCount.ToString();
                    friend.Value = SourceUser.FriendsCount.ToString();
                    follower.Value = SourceUser.FollowersCount.ToString();
                    img.Image = UserImageManager.GetUserImage(SourceUser);
                    try
                    {
                        var fs = t.Friendships.Show(source_id => m.Id, target_id => SourceUser.Id);
                        if ((fs.Source.IsFollowing ?? false) && (fs.Target.IsFollowing ?? false) && m.Id != SourceUser.Id)
                        {
                            IsValidUser = true;
                            ChangingAction(true);
                        }
                    }
                    catch
                    {
                    }
                });
        }

        public override IEnumerator<bool> Execute()
        {
            sname = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { X = 48, Y = 14, Value = "ここに表示されます" };
            tweets = new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Red) { Value = "Loading...", X = 96, Y = 48 };
            fav = new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Red) { Value = "Loading...", X = 96, Y = 68 };
            friend = new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Red) { Value = "Loading...", X = 96, Y = 88 };
            follower = new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Red) { Value = "Loading...", X = 96, Y = 108 };
            img = new Sprite() { X = 0, Y = 4 };

            X = 320;
            Y = 80;
            Manager.Add(new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Black) { Value = "ツイート数", X = 0, Y = 48 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Black) { Value = "お気に入り", X = 0, Y = 68 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Black) { Value = "フォロー", X = 0, Y = 88 }, 0);
            Manager.Add(new StringSprite(CommonObjects.FontSystemSmall, CommonObjects.Colors.Black) { Value = "フォロワー", X = 0, Y = 108 }, 0);

            Manager.Add(sname, 0);
            Manager.Add(img, 0);
            Manager.Add(tweets, 0);
            Manager.Add(fav, 0);
            Manager.Add(friend, 0);
            Manager.Add(follower, 0);

            while (true) yield return true;
        }
    }

    #endregion

    #region TitleChildSceneOptionEdit

    public class TitleChildSceneOptionEdit : ChildScene
    {
        private GamepadState state, tstate, prevstate;
        StringSprite sum, type, dirc, mode, seltype, seldirc, selmode;
        List<StringSprite> uvdesc, ipuvdesc, uvs, seluvs;
        MultiAdditionalCoroutineSprite[] udc;
        MultiAdditionalCoroutineSprite mc;
        KeyInputObject ki;

        List<MenuAllocationInformation> uvsmal;
        int cstate = 0, usel = 0, smsel = 0, avu;
        User[] opts;
        int[] osi;
        OptionInitializationInformation[] oii;
        List<MultiAdditionalCoroutineSprite> selopts;

        public TitleChildSceneOptionEdit(User[] op)
        {
            opts = op;
            osi = new int[opts.Length];
            oii = new OptionInitializationInformation[opts.Length];
            sum = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "オプション装備編集", X = 230, Y = 8 };
            type = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Blue) { Value = "装備タイプ", X = 160, Y = 32 + 8 };
            dirc = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Green) { Value = "装備方向", X = 160, Y = 64 + 8 };
            mode = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Green) { Value = "モード", X = 160, Y = 96 + 8 };
            uvdesc = new List<StringSprite>()
            {
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "装備固有オプション1", X = 160, Y = 128 + 8 },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "装備固有オプション2", X = 160, Y = 160 + 8 },       
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "装備固有オプション3", X = 160, Y = 192 + 8 },
            };

            seltype = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "", X = 360, Y = 32 + 8 };
            seldirc = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "", X = 360, Y = 64 + 8 };
            selmode = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "", X = 360, Y = 96 + 8 };
            ipuvdesc = new List<StringSprite>()
            {
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "", X = 360, Y = 128 + 8 },
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "", X = 360, Y = 160 + 8 },       
                new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Black) { Value = "", X = 360, Y = 192 + 8 },
            };

            uvs = new List<StringSprite>();
            uvs.Add(type);
            uvs.Add(dirc);
            uvs.Add(mode);
            uvs.AddRange(uvdesc);

            seluvs = new List<StringSprite>();
            seluvs.Add(seltype);
            seluvs.Add(seldirc);
            seluvs.Add(selmode);
            seluvs.AddRange(ipuvdesc);

            uvsmal = new List<MenuAllocationInformation>
            {
                new MenuAllocationInformation{ X = 144, Y = 48 },
                new MenuAllocationInformation{ X = 144, Y = 80 },
                new MenuAllocationInformation{ X = 144, Y = 112 },
                new MenuAllocationInformation{ X = 144, Y = 144 },
                new MenuAllocationInformation{ X = 144, Y = 176 },
                new MenuAllocationInformation{ X = 144, Y = 208 },
            };

            for (int i = 0; i < uvsmal.Count; i++)
            {
                int ci = i;
                uvsmal[i].Upper = uvsmal[(i + uvsmal.Count - 1) % uvsmal.Count];
                uvsmal[i].Lower = uvsmal[(i + 1) % uvsmal.Count];
                uvsmal[i].AvailableChangingAction =
                    (p, v) =>
                    {
                        uvs[ci].Alpha = seluvs[ci].Alpha = v ? 1.0 : 0.5;
                    };
                if (i >= 1) uvsmal[i].IsAvailable = false;
            }

            udc = new[] 
            {
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.8,ScaleY=0.4,Image=CommonObjects.ImageCursor128[2],X=64,Y=48},
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.8,ScaleY=0.4,Image=CommonObjects.ImageCursor128[3],X=64,Y=208},
            };
            selopts = new List<MultiAdditionalCoroutineSprite>();
            foreach (var i in opts)
            {
                if (i == null) continue;
                selopts.Add(new MultiAdditionalCoroutineSprite() { HomeX = 48, HomeY = 48, X = 64, Y = 128, Image = BigUserImageManager.GetUserImage(i), Alpha = 0 });
            }
            avu = selopts.Count;
            for (int i = 0; i < avu; i++)
            {
                osi[i] = 0;
                oii[i] = new OptionInitializationInformation();
            }
            selopts.Add(new MultiAdditionalCoroutineSprite() { HomeX = 48, HomeY = 48, X = 64, Y = 128, Image = CommonObjects.ImageOptionEditEnd, Alpha = 0 });
            selopts[usel].Alpha = 1;

            mc = new MultiAdditionalCoroutineSprite() { Image = CommonObjects.ImageCursor128[1], HomeX = 64, HomeY = 64, ScaleX = 0.25, ScaleY = 0.25 };
            mc.X = uvsmal[smsel].X;
            mc.Y = uvsmal[smsel].Y;
        }

        public override IEnumerator<bool> Execute()
        {
            Manager.Add(sum, 1);
            Manager.AddRangeTo(uvs, 1);
            Manager.AddRangeTo(seluvs, 1);
            Manager.AddRangeTo(udc, 2);
            foreach (var i in udc) i.AddSubOperation(SpritePatterns.Blink(30, 0.8, Easing.Linear));
            Manager.Add(mc, 2);
            Manager.AddRangeTo(selopts, 1);
            Manager.OffsetX = 640;
            Manager.OffsetY = 240;
            //突入
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
                switch (cstate)
                {
                    case 0:
                        if (tstate.Buttons[1])
                        {
                            Parent.SendChildMessage("ReturnToOptionEdit");
                            CommonObjects.SoundMenuCancel.Play();
                            for (int i = 0; i < 40; i++)
                            {
                                Manager.OffsetX = Easing.OutQuad(i, 40, 0, 640);
                                yield return true;
                            }
                            goto EXIT;
                        }
                        if (tstate.Buttons[0])
                        {
                            CommonObjects.SoundMenuOK.Play();
                            if (usel == selopts.Count - 1)
                            {
                                var ol = new OptionInformation[5];
                                for (int i = 0; i < avu; i++)
                                {
                                    ol[i] = new OptionInformation(opts[i]);
                                    ol[i].TargetOperation = OptionOperations.SelectionInformation[osi[i]].Operation;
                                    ol[i].InitializationInformation = oii[i];
                                }
                                Parent.SendChildMessage("ApplyOptionInformation", ol);
                                Parent.SendChildMessage("StartGame");
                                for (int i = 0; i < 40; i++)
                                {
                                    Manager.OffsetX = Easing.OutQuad(i, 40, 0, -640);
                                    yield return true;
                                }
                                goto EXIT;
                            }
                            else
                            {
                                GoToEditingMode();
                            }
                        }
                        if ((tstate.Direction & GamepadDirection.Up) != 0)
                        {
                            selopts[usel].AddSubOperation(SpritePatterns.Alpha(15, 0, Easing.Linear));
                            selopts[usel].AddSubOperation(SpritePatterns.Move(15, selopts[usel].X, selopts[usel].Y - 96, Easing.OutQuad));
                            usel = (usel + (selopts.Count - 1)) % selopts.Count;
                            selopts[usel].Y = 128 + 96;
                            selopts[usel].AddSubOperation(SpritePatterns.Alpha(15, 1, Easing.Linear));
                            selopts[usel].AddSubOperation(SpritePatterns.Move(15, selopts[usel].X, selopts[usel].Y - 96, Easing.OutQuad));
                            if (usel < avu) RefreshOptionInformation(false);
                            smsel = 0;
                            mc.AddSubOperation(SpritePatterns.VerticalMove(10, uvsmal[smsel].Y, Easing.OutQuad));
                            CommonObjects.SoundMenuSelect.Play();
                        }
                        if ((tstate.Direction & GamepadDirection.Down) != 0)
                        {
                            selopts[usel].AddSubOperation(SpritePatterns.Alpha(15, 0, Easing.Linear));
                            selopts[usel].AddSubOperation(SpritePatterns.Move(15, selopts[usel].X, selopts[usel].Y + 96, Easing.OutQuad));
                            usel = (usel + 1) % selopts.Count;
                            selopts[usel].Y = 128 - 96;
                            selopts[usel].AddSubOperation(SpritePatterns.Alpha(15, 1, Easing.Linear));
                            selopts[usel].AddSubOperation(SpritePatterns.Move(15, selopts[usel].X, selopts[usel].Y + 96, Easing.OutQuad));
                            if (usel < avu) RefreshOptionInformation(false);
                            smsel = 0;
                            mc.AddSubOperation(SpritePatterns.VerticalMove(10, uvsmal[smsel].Y, Easing.OutQuad));
                            CommonObjects.SoundMenuSelect.Play();
                        }
                        break;
                    case 1:
                        SubMenuOperation();
                        break;
                    case 2:
                        if (!UserValueInputOperation())
                        {
                            var ivs = new StringSprite(CommonObjects.FontSystemMedium, CommonObjects.Colors.Red) { Value = "入力値が不正です!", X = seluvs[smsel].X, Y = seluvs[smsel].Y };
                            ivs.AddSubOperation(SpritePatterns.VerticalFadeOut(60, -64, Easing.OutQuad, Easing.Linear));
                            Manager.Add(ivs, 3);
                        }
                        break;
                }
                prevstate = state;
                yield return true;
            }
        EXIT: ;
        }

        public void SubMenuOperation()
        {
            if ((tstate.Direction & GamepadDirection.Up) != 0)
            {
                do
                {
                    var tm = uvsmal.IndexOf(uvsmal[smsel].Upper);
                    if (tm != -1) smsel = tm;
                } while (!uvsmal[smsel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.VerticalMove(10, uvsmal[smsel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
            }
            if ((tstate.Direction & GamepadDirection.Down) != 0)
            {
                do
                {
                    var tm = uvsmal.IndexOf(uvsmal[smsel].Lower);
                    if (tm != -1) smsel = tm;
                } while (!uvsmal[smsel].IsAvailable);
                mc.AddSubOperation(SpritePatterns.VerticalMove(10, uvsmal[smsel].Y, Easing.OutQuad));
                CommonObjects.SoundMenuSelect.Play();
            }
            switch (smsel)
            {
                case 0:
                    if ((tstate.Direction & GamepadDirection.Left) != 0)
                    {
                        osi[usel] = (osi[usel] + (OptionOperations.SelectionInformation.Count - 1)) % OptionOperations.SelectionInformation.Count;
                        RefreshOptionInformation(true);
                        CommonObjects.SoundMenuSelect.Play();
                    }
                    if ((tstate.Direction & GamepadDirection.Right) != 0)
                    {
                        osi[usel] = (osi[usel] + 1) % OptionOperations.SelectionInformation.Count;
                        RefreshOptionInformation(true);
                        CommonObjects.SoundMenuSelect.Play();
                    }
                    break;
                case 1:
                    if ((tstate.Direction & GamepadDirection.Left) != 0)
                    {
                        oii[usel].Direction = (OptionDirection)((int)(oii[usel].Direction + (OptionOperations.OptionDirectionDescriptions.Count - 1)) % OptionOperations.OptionDirectionDescriptions.Count);
                        RefreshOptionInformation(false);
                        CommonObjects.SoundMenuSelect.Play();
                    }
                    if ((tstate.Direction & GamepadDirection.Right) != 0)
                    {
                        oii[usel].Direction = (OptionDirection)((int)(oii[usel].Direction + 1) % OptionOperations.OptionDirectionDescriptions.Count);
                        RefreshOptionInformation(false);
                        CommonObjects.SoundMenuSelect.Play();
                    }
                    break;
                case 2:
                    var tosi = OptionOperations.SelectionInformation[osi[usel]];
                    if ((tstate.Direction & GamepadDirection.Left) != 0)
                    {
                        oii[usel].Mode = (oii[usel].Mode + (tosi.ModeStrings.Count - 1)) % tosi.ModeStrings.Count;
                        RefreshOptionInformation(false);
                        CommonObjects.SoundMenuSelect.Play();
                    }
                    if ((tstate.Direction & GamepadDirection.Right) != 0)
                    {
                        oii[usel].Mode = (oii[usel].Mode + 1) % tosi.ModeStrings.Count;
                        RefreshOptionInformation(false);
                        CommonObjects.SoundMenuSelect.Play();
                    }
                    break;
                case 3:
                case 4:
                case 5:
                    if (tstate.Buttons[0])
                    {
                        ki = new KeyInputObject(CommonObjects.FontSystemMedium, 256, true, false, false) { X = seluvs[smsel].X, Y = seluvs[smsel].Y };
                        seluvs[smsel].Alpha = 0;
                        Manager.Add(ki, 2);
                        cstate = 2;
                        CommonObjects.SoundMenuOK.Play();
                    }
                    break;
            }
            if (tstate.Buttons[1])
            {
                cstate = 0;
                mc.AvailableSubOperations.Clear();
                CommonObjects.SoundMenuCancel.Play();
            }
        }

        public bool UserValueInputOperation()
        {
            if (ki.HasCompleted)
            {
                cstate = 1;
                seluvs[smsel].Alpha = 1.0;
                return TrySetUserValue();
            }
            else if (ki.IsCanceled)
            {
                cstate = 1;
                seluvs[smsel].Alpha = 1.0;
                return true;
            }
            else
            {
                return true;
            }
        }

        public bool TrySetUserValue()
        {
            var toii = oii[usel];
            var tosi = OptionOperations.SelectionInformation[osi[usel]];
            var uvi = smsel - 3;
            try
            {
                switch (tosi.ActualUserValues[uvi])
                {
                    case OptionSelectionValue.Int32Value1:
                        var vali1 = Convert.ToInt32(ki.InputString);
                        if (tosi.DefaultValue.UserInt32Value1Validation == null ||
                            (tosi.DefaultValue.UserInt32Value1Validation != null && tosi.DefaultValue.UserInt32Value1Validation(vali1)))
                        {
                            toii.UserInt32Value1 = vali1;
                            ipuvdesc[uvi].Value = toii.UserInt32Value1.ToString();
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.Int32Value2:
                        var vali2 = Convert.ToInt32(ki.InputString);
                        if (tosi.DefaultValue.UserInt32Value2Validation == null ||
                            (tosi.DefaultValue.UserInt32Value2Validation != null && tosi.DefaultValue.UserInt32Value2Validation(vali2)))
                        {
                            toii.UserInt32Value2 = vali2;
                            ipuvdesc[uvi].Value = toii.UserInt32Value2.ToString();
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.Int32Value3:
                        var vali3 = Convert.ToInt32(ki.InputString);
                        if (tosi.DefaultValue.UserInt32Value3Validation == null ||
                            (tosi.DefaultValue.UserInt32Value3Validation != null && tosi.DefaultValue.UserInt32Value3Validation(vali3)))
                        {
                            toii.UserInt32Value3 = vali3;
                            ipuvdesc[uvi].Value = toii.UserInt32Value3.ToString();
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.DoubleValue1:
                        var vald1 = Convert.ToDouble(ki.InputString);
                        if (tosi.DefaultValue.UserDoubleValue1Validation == null ||
                            (tosi.DefaultValue.UserDoubleValue1Validation != null && tosi.DefaultValue.UserDoubleValue1Validation(vald1)))
                        {
                            toii.UserDoubleValue1 = vald1;
                            ipuvdesc[uvi].Value = toii.UserDoubleValue1.ToString();
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.DoubleValue2:
                        var vald2 = Convert.ToDouble(ki.InputString);
                        if (tosi.DefaultValue.UserDoubleValue2Validation == null ||
                            (tosi.DefaultValue.UserDoubleValue2Validation != null && tosi.DefaultValue.UserDoubleValue2Validation(vald2)))
                        {
                            toii.UserDoubleValue2 = vald2;
                            ipuvdesc[uvi].Value = toii.UserDoubleValue2.ToString();
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.DoubleValue3:
                        var vald3 = Convert.ToDouble(ki.InputString);
                        if (tosi.DefaultValue.UserDoubleValue3Validation == null ||
                            (tosi.DefaultValue.UserDoubleValue3Validation != null && tosi.DefaultValue.UserDoubleValue3Validation(vald3)))
                        {
                            toii.UserDoubleValue1 = vald3;
                            ipuvdesc[uvi].Value = toii.UserDoubleValue3.ToString();
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.StringValue1:
                        if (tosi.DefaultValue.UserStringValue1Validation == null ||
                            (tosi.DefaultValue.UserStringValue1Validation != null && tosi.DefaultValue.UserStringValue1Validation(ki.InputString)))
                        {
                            toii.UserStringValue1 = ki.InputString;
                            ipuvdesc[uvi].Value = toii.UserStringValue1;
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.StringValue2:
                        if (tosi.DefaultValue.UserStringValue2Validation == null ||
                            (tosi.DefaultValue.UserStringValue2Validation != null && tosi.DefaultValue.UserStringValue2Validation(ki.InputString)))
                        {
                            toii.UserStringValue2 = ki.InputString;
                            ipuvdesc[uvi].Value = toii.UserStringValue2;
                        }
                        else { throw new InvalidDataException(); }
                        break;
                    case OptionSelectionValue.StringValue3:
                        if (tosi.DefaultValue.UserStringValue3Validation == null ||
                            (tosi.DefaultValue.UserStringValue3Validation != null && tosi.DefaultValue.UserStringValue3Validation(ki.InputString)))
                        {
                            toii.UserStringValue3 = ki.InputString;
                            ipuvdesc[uvi].Value = toii.UserStringValue3;
                        }
                        else { throw new InvalidDataException(); }
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void GoToEditingMode()
        {
            cstate = 1;
            mc.AddSubOperation(SpritePatterns.Blink(30, 0.5, Easing.Linear));
        }

        public void RefreshOptionInformation(bool changing)
        {
            var tosi = OptionOperations.SelectionInformation[osi[usel]];
            var al = tosi.UserValueCombination.GetDecomposedValues();
            for (int i = 1; i < uvsmal.Count; i++)
            {
                seluvs[i].Value = "";
                uvsmal[i].IsAvailable = false;
            }
            for (int i = 0; i < uvdesc.Count; i++)
            {
                uvdesc[i].Value = String.Format("装備固有オプション{0}", i + 1);
            }
            if (changing)
            {
                oii[usel].Direction = tosi.DefaultValue.Direction;
                oii[usel].Mode = tosi.DefaultValue.Mode;
                oii[usel].UserInt32Value1 = tosi.DefaultValue.UserInt32Value1;
                oii[usel].UserInt32Value2 = tosi.DefaultValue.UserInt32Value2;
                oii[usel].UserInt32Value3 = tosi.DefaultValue.UserInt32Value3;
                oii[usel].UserDoubleValue1 = tosi.DefaultValue.UserDoubleValue1;
                oii[usel].UserDoubleValue2 = tosi.DefaultValue.UserDoubleValue2;
                oii[usel].UserDoubleValue3 = tosi.DefaultValue.UserDoubleValue3;
                oii[usel].UserStringValue1 = tosi.DefaultValue.UserStringValue1;
                oii[usel].UserStringValue2 = tosi.DefaultValue.UserStringValue2;
                oii[usel].UserStringValue3 = tosi.DefaultValue.UserStringValue3;
            }
            seltype.Value = tosi.Name;
            var uvu = 0;
            foreach (var i in al)
            {
                switch (i)
                {
                    case OptionSelectionValue.Direction:
                        uvsmal[1].IsAvailable = true;
                        seldirc.Value = OptionOperations.OptionDirectionDescriptions[changing ? (int)tosi.DefaultValue.Direction : (int)oii[usel].Direction];
                        break;
                    case OptionSelectionValue.Mode:
                        uvsmal[2].IsAvailable = true;
                        selmode.Value = tosi.ModeStrings[(changing ? tosi.DefaultValue : oii[usel]).Mode];
                        break;

                    case OptionSelectionValue.Int32Value1:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserInt32Value1.ToString();
                        uvu++;
                        break;
                    case OptionSelectionValue.Int32Value2:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserInt32Value2.ToString();
                        uvu++;
                        break;
                    case OptionSelectionValue.Int32Value3:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserInt32Value3.ToString();
                        uvu++;
                        break;
                    case OptionSelectionValue.DoubleValue1:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserDoubleValue1.ToString();
                        uvu++;
                        break;
                    case OptionSelectionValue.DoubleValue2:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserDoubleValue2.ToString();
                        uvu++;
                        break;
                    case OptionSelectionValue.DoubleValue3:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserDoubleValue3.ToString();
                        uvu++;
                        break;
                    case OptionSelectionValue.StringValue1:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserStringValue1;
                        uvu++;
                        break;
                    case OptionSelectionValue.StringValue2:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserStringValue2;
                        uvu++;
                        break;
                    case OptionSelectionValue.StringValue3:
                        if (uvu >= 3) break;
                        ApplyUserValue(tosi, uvu, i);
                        ipuvdesc[uvu].Value = (changing ? tosi.DefaultValue : oii[usel]).UserStringValue3;
                        uvu++;
                        break;
                }
            }
        }

        private void ApplyUserValue(OptionSelectionInformation tosi, int uvu, OptionSelectionValue i)
        {
            uvsmal[uvu + 3].IsAvailable = true;
            uvdesc[uvu].Value = tosi.UserValueDescription[i];
            tosi.ActualUserValues[uvu] = i;
        }
    }

    #endregion

    public class AccountInformation
    {
        public Kbtter4Account[] Accounts { get; private set; }
        public User[] Users { get; private set; }
        public bool[] HasGot { get; private set; }
        public Tokens[] Tokens { get; private set; }

        public AccountInformation(IEnumerable<Kbtter4Account> acs)
        {
            Accounts = acs.ToArray();
            Users = new User[Accounts.Length];
            HasGot = new bool[Accounts.Length];
            Tokens = new Tokens[Accounts.Length];
        }
    }

    public class MenuAllocationInformation
    {
        public double X { get; set; }
        public double Y { get; set; }
        public MenuAllocationInformation Upper { get; set; }
        public MenuAllocationInformation Lower { get; set; }
        public MenuAllocationInformation Lefter { get; set; }
        public MenuAllocationInformation Righter { get; set; }
        bool _av;
        public bool IsAvailable
        {
            get { return _av; }
            set
            {
                _av = value;
                if (AvailableChangingAction != null) AvailableChangingAction(this, value);
            }
        }
        public Action<MenuAllocationInformation, bool> AvailableChangingAction { get; set; }

        public MenuAllocationInformation()
        {
            Upper = this;
            Lower = this;
            Lefter = this;
            Righter = this;
            IsAvailable = true;
        }
    }
}
