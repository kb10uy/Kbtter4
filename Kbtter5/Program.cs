using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Kbtter4.Models;
using CoreTweet;
using CoreTweet.Streaming;
using Newtonsoft.Json;
using DxLibDLL;

namespace Kbtter5
{
    class Program
    {
        static void Main(string[] args)
        {
            DX.ChangeWindowMode(DX.TRUE);
            if (DX.DxLib_Init() == -1) return;
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);
            DX.SetWindowText("Kbtter5 Polyvinyl Chroride");

            new Kbtter5().Run();

            DX.DxLib_End();
        }
    }

    public sealed class Kbtter5
    {
        private Kbtter Kbtter = Kbtter.Instance;
        private Scene curscene;
        public Scene CurrentScene
        {
            get { return curscene; }
            set
            {
                if (value != null)
                {
                    curscene = value;
                    DrawCoroutine = curscene.Draw();
                    TickCoroutine = curscene.Tick();
                }
                else
                {
                    curscene = null;
                    DrawCoroutine = new Scene().Draw();
                    TickCoroutine = new Scene().Tick();
                }
            }
        }

        public IEnumerator<bool> DrawCoroutine { get; private set; }
        public IEnumerator<bool> TickCoroutine { get; private set; }

        public void Run()
        {
            CurrentScene = new SceneTitle();
            while (DX.ProcessMessage() != -1)
            {
                TickCoroutine.MoveNext();
                DX.ClearDrawScreen();
                DrawCoroutine.MoveNext();
                DX.ScreenFlip();
            }
        }
    }

    public class Scene
    {
        public virtual IEnumerator<bool> Draw()
        {
            while (true) yield return true;
        }

        public virtual IEnumerator<bool> Tick()
        {
            while (true) yield return true;
        }
    }
}
