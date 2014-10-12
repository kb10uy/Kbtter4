using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasingSharp;

namespace Kbtter5
{
    public delegate IEnumerator<bool> EnemyPattern(UserSprite sp);

    public static class EnemyPatterns
    {
        public static IEnumerator<bool> GoDownAndAway(UserSprite sp)
        {
            var sx = sp.X;
            var sy = sp.Y;
            for (int i = 0; i < 60; i++)
            {
                sp.Y = Easing.OutSine(i, 60, sy, 100);
                yield return true;
            }
        }
    }

    public delegate IEnumerator<bool> BulletPattern(UserSprite par,Bullet b);

    public static class BulletPatterns
    {
        public static BulletPattern CreateUniformLinearMotion(double angle, double speed, int time)
        {
            return (par, b) => UniformLinearMotion(par, b, angle, speed, time);
        }

        private static IEnumerator<bool> UniformLinearMotion(UserSprite par, Bullet b, double angle, double speed, int time)
        {
            for (int i = 0; i < time; i++)
            {
                b.X += Math.Cos(angle) * speed;
                b.Y += Math.Sin(angle) * speed;
                yield return true;
            }
            b.IsDead = true;
            yield break;
        }
    }
}
