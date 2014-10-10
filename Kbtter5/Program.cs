using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Scene curscene;
        public Scene CurrentScene
        {
            get { return curscene; }
            set
            {
                if (curscene != null)
                {
                    curscene = value;
                    DrawCoroutine = curscene.Draw().GetEnumerator();
                    TickCoroutine = curscene.Tick().GetEnumerator();
                }
                else
                {
                    curscene = null;
                    DrawCoroutine = new Scene().Draw().GetEnumerator();
                    TickCoroutine = new Scene().Tick().GetEnumerator();
                }
            }
        }

        public IEnumerator<bool> DrawCoroutine { get; private set; }
        public IEnumerator<bool> TickCoroutine { get; private set; }

        public void Run()
        {
            while (DX.ProcessMessage() != -1)
            {
                TickCoroutine.MoveNext();
                DrawCoroutine.MoveNext();
                DX.ScreenFlip();
            }
        }
    }

    public class Scene
    {
        public virtual IEnumerable<bool> Draw()
        {
            while (true) yield return true;
        }

        public virtual IEnumerable<bool> Tick()
        {
            while (true) yield return true;
        }
    }
}
