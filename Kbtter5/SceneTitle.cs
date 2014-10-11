using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;
using EasingSharp;
using Kbtter4.Models;

namespace Kbtter5
{
    class SceneTitle : Scene
    {
        private Kbtter Kbtter = Kbtter.Instance;
        private int backblend = 0;
        private int logoblend = 0;
        private int state = 0;
        private bool? hasacc;
        private Point logopos = new Point { X = 80, Y = 160 };
        private Kbtter4Account[] accounts;
        private int selac = 0;

        public override IEnumerator<bool> Tick()
        {
            for (int i = 0; i < 60; i++)
            {
                backblend = (int)((255.0 / 60.0) * i);
                logoblend = (int)((255.0 / 60.0) * i);
                yield return true;
            }
            backblend = 255;
            logoblend = 255;
            while (!Gamepad.GetState().Buttons[0]) yield return true;
            for (int i = 0; i < 60; i++)
            {
                logopos.X = Easing.InOutSine(i, 60, 80, -60);
                logopos.Y = Easing.InOutSine(i, 60, 160, -140);
                yield return true;
            }

            //読み込む
            if (Kbtter.Accounts.Count == 0)
            {
                state = 1;
                while (true) yield return true;
            }

            state = 2;
            accounts = Kbtter.Accounts.ToArray();

            var ps = Gamepad.GetState();
            var ks = Gamepad.GetState();
            while (true)
            {
                ks = Gamepad.GetState().GetTriggerStateWith(ps);

                if (ks.Direction.HasFlag(GamepadDirection.Up))
                {
                    selac = (selac + accounts.Length - 1) % accounts.Length;
                }
                if (ks.Direction.HasFlag(GamepadDirection.Down))
                {
                    selac = (selac + 1) % accounts.Length;
                }
                if (ks.Buttons[0])
                {
                    state = 3;
                    break;
                }
                ps = Gamepad.GetState();
                yield return true;
            }

            for (int i = 0; i < 180; i++)
            {
                backblend = (int)(255.0 - (255.0 / 180.0) * i);
                logoblend = (int)(255.0 - (255.0 / 180.0) * i);
                yield return true;
            }
            backblend = 0;
            logoblend = 0;

            state = 4;

            while (true) yield return true;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 255);
                DX.DrawBox(0, 0, 640, 480, DX.GetColor(backblend, backblend, backblend), DX.TRUE);

                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, logoblend);
                DX.DrawGraph((int)logopos.X, (int)logopos.Y, CommonObjects.ImageLogo, DX.TRUE);

                DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 255);
                switch (state)
                {
                    case 0:
                    case 3:
                        break;
                    case 1:
                        DX.DrawStringToHandle(
                            20, 240,
                            "アカウントデータがありません！Kbtter4で一回アカウントを登録してください。",
                            DX.GetColor(255, 0, 0),
                            CommonObjects.FontSystem);
                        break;
                    case 2:
                        DX.DrawStringToHandle(40, 200, "アカウント選択", DX.GetColor(0, 0, 0), CommonObjects.FontSystem);
                        for (int i = 0; i < accounts.Length; i++)
                        {
                            DX.DrawStringToHandle(60, 240 + i * 20, accounts[i].ScreenName, DX.GetColor(0, 0, 255), CommonObjects.FontSystem);
                        }
                        DX.DrawStringToHandle(40, 240 + selac * 20, "→", DX.GetColor(0, 0, 0), CommonObjects.FontSystem);
                        break;
                    case 4:
                        break;

                }

                yield return true;
            }

        }
    }
}
