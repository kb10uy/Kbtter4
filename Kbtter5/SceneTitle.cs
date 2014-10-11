using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace Kbtter5
{
    class SceneTitle : Scene
    {
        private int backblend = 0;
        private int logoblend = 0;

        public override IEnumerator<bool> Tick()
        {
            for (int i = 0; i < 60; i++)
            {
                backblend = (int)((255.0 / 60.0) * i);
                yield return true;
            }
            backblend = 255;
            for (int i = 0; i < 60; i++)
            {
                logoblend = (int)((255.0 / 60.0) * i);
                yield return true;
            }
            logoblend = 255;
            while (true) yield return true;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_NOBLEND, 255);
                DX.DrawBox(0, 0, 640, 480, DX.GetColor(backblend, backblend, backblend), DX.TRUE);

                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, logoblend);
                DX.DrawGraph(80, 120, CommonObjects.Logo, DX.TRUE);
                yield return true;
            }
        }
    }
}
