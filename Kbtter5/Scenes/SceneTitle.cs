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
        private Kbtter Kbtter = Kbtter.Instance;
        private Kbtter4Account[] accounts;
        private Kbtter5 Kbtter5 = Kbtter5.Instance;
        private IEnumerator<bool> Operation;


        public SceneTitle()
        {
            Operation = Execute();
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                Operation.MoveNext();
                Manager.TickAll();
                yield return true;
            }
        }

        public IEnumerator<bool> Execute()
        {
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

            var selm = 0;
            var menu = new[] 
            {
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=128,Image=CommonObjects.ImageTitleMenuStart},
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=128,Image=CommonObjects.ImageTitleMenuQuick},
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=128,Image=CommonObjects.ImageTitleMenuOption},
                new AdditionalCoroutineSprite(){HomeX=160,HomeY=128,Image=CommonObjects.ImageTitleMenuRanking},
            };

            while (true) yield return true;
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
}
