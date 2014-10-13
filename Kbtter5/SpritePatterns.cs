using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{
    public delegate IEnumerator<bool> SpritePattern(CoroutineSprite sp);

    public static class SpritePatterns
    {
        public static SpritePattern MissStar(double angle, UserSprite p)
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
    }
}
