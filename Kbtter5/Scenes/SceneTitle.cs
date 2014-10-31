using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;
using EasingSharp;
using Kbtter4.Models;

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
        private MultiAdditionalCoroutineSprite[] lrc, udc;

        public SceneTitle()
        {
            Operation = Execute();
        }

        public IEnumerator<bool> Execute()
        {
            DX.SetDrawMode(DX.DX_DRAWMODE_BILINEAR);
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
                menu[i].ApplyOperation(MenuIntro(i * 15, 60, 400));
            }

            lrc = new[] 
            {
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.5,ScaleY=0.8,Image=CommonObjects.ImageCursor128[0],X=120,Y=400},
                new MultiAdditionalCoroutineSprite(){HomeX=64,HomeY=64,ScaleX=0.5,ScaleY=0.8,Image=CommonObjects.ImageCursor128[1],X=520,Y=400},
            };
            Manager.AddRangeTo(lrc, 2);
            foreach (var i in lrc) i.AddOperation(Blink(30, 0.8, Easing.Linear));

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
                                        menu[i].ApplyOperation(MenuIntro(0, 60, 600));
                                    }
                                    break;
                                }
                            }
                            yield return true;
                        }
                        switch (selmenu)
                        {
                            case 0:
                                //Start
                                Children.AddChildScene(new TitleChildSceneAccountSelect());
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
                        menu[i].ApplyOperation(MenuIntro(i * 15, 60, 400));
                    }
                    break;
            }
        }

        #region メニュー用AdditionalCoroutineSpritePatternとか
        public static CoroutineFunction<MultiAdditionalCoroutineSprite> Blink(int time, double duraiton, EasingFunction easing)
        {
            return sp => BlinkFunction(sp, time, duraiton, easing);
        }

        public static IEnumerator<bool> BlinkFunction(MultiAdditionalCoroutineSprite sp, int time, double duraiton, EasingFunction easing)
        {
            while (true)
            {
                for (int i = 0; i < time; i++)
                {
                    sp.Alpha = easing(i, time, 1, -duraiton);
                    yield return true;
                }
            }
        }

        private static CoroutineFunction<AdditionalCoroutineSprite> MenuIntro(int delay, int time, double ty)
        {
            return sp => MenuIntroFunction(sp, delay, time, ty);
        }

        private static IEnumerator<bool> MenuIntroFunction(AdditionalCoroutineSprite sp, int delay, int time, double ty)
        {
            for (int i = 0; i < delay; i++) yield return true;
            var sy = sp.Y;
            for (int i = 0; i < time; i++)
            {
                sp.Y = Easing.OutBack(i, time, sy, ty - sy);
                yield return true;
            }
            sp.Y = ty;
        }

        private static CoroutineFunction<AdditionalCoroutineSprite> MenuOutro(int delay, int time, double ty)
        {
            return sp => MenuOutroFunction(sp, delay, time, ty);
        }

        private static IEnumerator<bool> MenuOutroFunction(AdditionalCoroutineSprite sp, int delay, int time, double ty)
        {
            for (int i = 0; i < delay; i++) yield return true;
            var sy = sp.Y;
            for (int i = 0; i < time; i++)
            {
                sp.Y = Easing.OutSine(i, time, sy, ty - sy);
                yield return true;
            }
            sp.Y = ty;
        }

        private static CoroutineFunction<AdditionalCoroutineSprite> MenuEnable()
        {
            return sp => MenuEnableFunction(sp);
        }

        private static IEnumerator<bool> MenuEnableFunction(AdditionalCoroutineSprite sp)
        {
            var sx = sp.X;
            for (int i = 0; i < 30; i++)
            {
                sp.X = Easing.OutQuad(i, 30, sx, 320 - sx);
                sp.Alpha = Easing.OutQuad(i, 30, 0.5, 0.5);
                sp.ScaleX = sp.ScaleY = Easing.OutQuad(i, 30, 0.8, 0.2);
                yield return true;
            }
            sp.X = 320;
            sp.Alpha = 1;
            sp.ScaleX = sp.ScaleY = 1;
            yield return true;
        }

        private static CoroutineFunction<AdditionalCoroutineSprite> MenuDisable(double tx)
        {
            return sp => MenuDisableFunction(sp, tx);
        }

        private static IEnumerator<bool> MenuDisableFunction(AdditionalCoroutineSprite sp, double tx)
        {
            var sx = sp.X;
            var sa = sp.Alpha;
            var ss = sp.ScaleX;
            for (int i = 0; i < 30; i++)
            {
                sp.X = Easing.OutQuad(i, 30, sx, tx - sx);
                sp.Alpha = Easing.OutQuad(i, 30, sa, 0.5 - sa);
                sp.ScaleX = sp.ScaleY = Easing.OutQuad(i, 30, ss, 0.8 - ss);
                yield return true;
            }
            sp.X = tx;
            sp.Alpha = 0.5;
            sp.ScaleX = sp.ScaleY = 0.8;
            yield return true;
        }

        public void RefreshMenuPosition()
        {
            for (int i = 0; i < menu.Length; i++)
            {
                if (i == selmenu)
                {
                    menu[i].ApplyOperation(MenuEnable());
                }
                else
                {
                    menu[i].ApplyOperation(MenuDisable(320 + 256 * (i - selmenu)));
                }
            }
        }
        #endregion

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
        private Kbtter4Account[] accounts;
        private GamepadState state, tstate, prevstate;

        public override IEnumerator<bool> Execute()
        {
            Manager.OffsetX = 640;
            Manager.OffsetY = 240;

            Manager.Add(new StringSprite(CommonObjects.FontSystem, CommonObjects.Colors.Black) { Value = "アカウント選択", X = 8, Y = 8 }, 0);

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
    }
}
