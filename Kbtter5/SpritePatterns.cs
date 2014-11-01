using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasingSharp;

namespace Kbtter5
{

    public static class SpritePatterns
    {
        public static CoroutineFunction<CoroutineSprite> MissStar(double angle, UserSprite p)
        {
            return (sp) => MissStarOperation(sp, angle, p);
        }

        private static IEnumerator<bool> MissStarOperation(CoroutineSprite sp, double angle, UserSprite p)
        {
            for (int i = 0; i < 60; i++)
            {
                sp.X += Math.Cos(angle) * 4;
                sp.Y += Math.Sin(angle) * 4;
                sp.ScaleX -= 0.01;
                sp.ScaleY -= 0.01;
                sp.Alpha -= 0.01;
                yield return true;
            }
        }

        #region メニュー用AdditionalCoroutineSpritePatternとか
        public static CoroutineFunction<MultiAdditionalCoroutineSprite> Blink(int time, double duraiton, EasingFunction easing)
        {
            return sp => BlinkFunction(sp, time, duraiton, easing);
        }

        private static IEnumerator<bool> BlinkFunction(MultiAdditionalCoroutineSprite sp, int time, double duraiton, EasingFunction easing)
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

        public static CoroutineFunction<AdditionalCoroutineSprite> MenuIntro(int delay, int time, double ty)
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

        public static CoroutineFunction<MultiAdditionalCoroutineSprite> MenuOutro(int delay, int time, double ty)
        {
            return sp => MenuOutroFunction(sp, delay, time, ty);
        }

        private static IEnumerator<bool> MenuOutroFunction(MultiAdditionalCoroutineSprite sp, int delay, int time, double ty)
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

        public static CoroutineFunction<AdditionalCoroutineSprite> MenuEnable()
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

        public static CoroutineFunction<AdditionalCoroutineSprite> MenuDisable(double tx)
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

        #endregion
    }
}
