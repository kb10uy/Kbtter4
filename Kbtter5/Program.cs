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
using Kbtter5.Scenes;

namespace Kbtter5
{
    class Program
    {
        static void Main(string[] args)
        {
            DX.ChangeWindowMode(DX.TRUE);
            if (DX.DxLib_Init() == -1) return;
            DX.SetAlwaysRunFlag(DX.TRUE);
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);
            DX.SetWindowText("Kbtter5 Polyvinyl Chloride");
            DX.SetUseASyncLoadFlag(DX.TRUE);
            Kbtter5.Instance.Run();

            DX.DxLib_End();
        }
    }

    public sealed class Kbtter5
    {
        private Kbtter Kbtter = Kbtter.Instance;

        private static Kbtter5 ins = new Kbtter5();
        public static Kbtter5 Instance
        {
            get { return ins; }
        }

        private Kbtter5()
        {

        }

        private Scene curscene;
        public Scene CurrentScene
        {
            get { return curscene; }
            set
            {
                if (value != null)
                {
                    curscene = value;
                }
                else
                {
                    curscene = new Scene();
                }
            }
        }

        public void Run()
        {
            CurrentScene = new SceneTitle();
            while (DX.ProcessMessage() != -1)
            {
                CurrentScene.TickCoroutine.MoveNext();
                DX.ClearDrawScreen();
                CurrentScene.DrawCoroutine.MoveNext();
                DX.ScreenFlip();
            }
        }
    }
}
