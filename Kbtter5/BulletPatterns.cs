using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{
    public delegate IEnumerator<bool> BulletPattern(UserSprite par, Bullet b);

    public static class BulletPatterns
    {
        public static BulletPattern Linear(double angle, double speed, int time)
        {
            return (par, b) => Linear(par, b, angle, speed, time);
        }

        private static IEnumerator<bool> Linear(UserSprite par, Bullet b, double angle, double speed, int time)
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

        public static BulletPattern LinearCurve(double angle, double curve, double speed, int time)
        {
            return (par, b) => LinearCurve(par, b, angle, curve, speed, time);
        }

        private static IEnumerator<bool> LinearCurve(UserSprite par, Bullet b, double angle, double curve, double speed, int time)
        {
            for (int i = 0; i < time; i++)
            {
                angle += curve;
                b.X += Math.Cos(angle) * speed;
                b.Y += Math.Sin(angle) * speed;
                yield return true;
            }
            b.IsDead = true;
            yield break;
        }

        public static BulletPattern LazyHoming(double startAngle, double startSpeed, int delay, UserSprite target, double homingSpeed)
        {
            return (par, b) => LazyHoming(par, b, startAngle, startSpeed, delay, target, homingSpeed);
        }

        private static IEnumerator<bool> LazyHoming(UserSprite par, Bullet b, double startAngle, double startSpeed, int delay, UserSprite target, double homingSpeed)
        {
            for (int i = 0; i < delay; i++)
            {
                b.X += Math.Cos(startAngle) * startSpeed;
                b.Y += Math.Sin(startAngle) * startSpeed;
                yield return true;
            }
            var ha = Math.Atan2(target.Y - b.Y, target.X - b.X);
            while (true)
            {
                b.X += Math.Cos(ha) * homingSpeed;
                b.Y += Math.Sin(ha) * homingSpeed;
                yield return true;
            }
        }
    }
}
