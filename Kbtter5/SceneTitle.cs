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
        private bool? hasacc;
        private Point logopos = new Point { X = 80, Y = 160 };

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
                hasacc = false;
            }


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
                if (hasacc == false)
                {
                    DX.DrawStringToHandle(20, 240, "アカウントデータがありません", DX.GetColor(255, 0, 0), CommonObjects.FontSystem);
                }
                yield return true;
            }
        }
    }
}
