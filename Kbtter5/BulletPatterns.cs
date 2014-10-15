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
        static Random rnd = new Random();

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

        public static BulletPattern LazyHomingToEnemy(PlayerUser u, double startAngle, double startSpeed, int delay, double homingSpeed)
        {
            return (par, b) => LazyHomingToEnemy(u, b, startAngle, startSpeed, delay, homingSpeed);
        }

        private static IEnumerator<bool> LazyHomingToEnemy(PlayerUser par, Bullet b, double startAngle, double startSpeed, int delay, double homingSpeed)
        {
            for (int i = 0; i < delay; i++)
            {
                b.X += Math.Cos(startAngle) * startSpeed;
                b.Y += Math.Sin(startAngle) * startSpeed;
                yield return true;
            }

            var t = par.ParentManager
                .Where(p => p.MyKind != b.MyKind && p.DamageKind.HasFlag(b.MyKind))
                .OrderBy(p => (p.X - b.X) * (p.X - b.X) + (p.Y - b.Y) * (p.Y - b.Y))
                .FirstOrDefault();
            var ang = 0.0;
            if (t == null)
            {
                ang = rnd.NextDouble() * Math.PI * 2;
            }
            else
            {
                ang = Math.Atan2(t.Y - b.Y, t.X - b.X);
            }

            while (true)
            {
                b.X += Math.Cos(ang) * homingSpeed;
                b.Y += Math.Sin(ang) * homingSpeed;
                yield return true;
            }
        }
    }
}
