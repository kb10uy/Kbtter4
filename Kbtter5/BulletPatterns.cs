using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{

    #region 通常弾

    public static class BulletPatterns
    {
        static Xorshift128Random rnd = new Xorshift128Random();

        public static CoroutineFunction<UserSprite, Bullet> Linear(double angle, double speed, int time)
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

        public static CoroutineFunction<UserSprite, Bullet> LinearCurve(double angle, double curve, double speed, int time)
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

        public static CoroutineFunction<UserSprite, Bullet> LazyHoming(double startAngle, double startSpeed, int delay, UserSprite target, double homingSpeed)
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

        public static CoroutineFunction<UserSprite, PlayerBullet> LazyHomingToEnemy(PlayerUser u, double startAngle, double startSpeed, int delay, double homingSpeed)
        {
            return (par, b) => LazyHomingToEnemy(u, b, startAngle, startSpeed, delay, homingSpeed);
        }

        private static IEnumerator<bool> LazyHomingToEnemy(PlayerUser par, PlayerBullet b, double startAngle, double startSpeed, int delay, double homingSpeed)
        {
            for (int i = 0; i < delay; i++)
            {
                b.X += Math.Cos(startAngle) * startSpeed;
                b.Y += Math.Sin(startAngle) * startSpeed;
                yield return true;
            }

            var t = par.ParentManager
                .Where(p => p.MyKind != b.MyKind && ((p.DamageKind & b.MyKind) != 0))
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
    #endregion

    #region レーザー用

    public static class LinearLaserPatterns
    {
        static Xorshift128Random rnd = new Xorshift128Random();

        public static CoroutineFunction<UserSprite, LinearLaser> Normal(double length, double speed)
        {
            return (par, l) => Normal(par, l, length, speed);
        }

        private static IEnumerator<bool> Normal(UserSprite parent, LinearLaser laser, double length, double speed)
        {
            laser.BrightR = (byte)((long)parent.SourceUser.Id & 0xFF);
            laser.BrightG = (byte)((long)(parent.SourceUser.Id >> 8) & 0xFF);
            laser.BrightB = (byte)((long)(parent.SourceUser.Id >> 16) & 0xFF);

            while (laser.Length < length)
            {
                laser.Length += speed;
                yield return true;
            }
            while (true)
            {
                laser.X += Math.Cos(laser.Angle) * speed;
                laser.Y += Math.Sin(laser.Angle) * speed;
                yield return true;
            }
        }

        public static CoroutineFunction<UserSprite, LinearLaser> Worm(double length, double speed)
        {
            return (par, l) => Worm(par, l, length, speed);
        }

        private static IEnumerator<bool> Worm(UserSprite parent, LinearLaser laser, double length, double speed)
        {
            laser.BrightR = (byte)((long)parent.SourceUser.Id & 0xFF);
            laser.BrightG = (byte)((long)(parent.SourceUser.Id >> 8) & 0xFF);
            laser.BrightB = (byte)((long)(parent.SourceUser.Id >> 16) & 0xFF);
            while (true)
            {
                while (laser.Length < length)
                {
                    laser.Length += speed;
                    yield return true;
                }
                while (laser.Length > 0.1)
                {
                    laser.X += Math.Cos(laser.Angle) * speed;
                    laser.Y += Math.Sin(laser.Angle) * speed;
                    laser.Length -= speed;
                    yield return true;
                }
            }
        }
    }

    public static class CurveLaserPatterns
    {
        public static CoroutineFunction<UserSprite, CurveLaser> Normal(int speed)
        {
            return (par, l) => Normal(par, l);
        }

        public static IEnumerator<bool> Normal(UserSprite parent, CurveLaser laser)
        {
            laser.BrightR = (byte)((long)parent.SourceUser.Id & 0xFF);
            laser.BrightG = (byte)((long)(parent.SourceUser.Id >> 8) & 0xFF);
            laser.BrightB = (byte)((long)(parent.SourceUser.Id >> 16) & 0xFF);
            var curve = laser.Curve;
            while (laser.DrawLength < laser.LaserImage.Length)
            {
                laser.DrawLength++;
                yield return true;
            }
            while (true)
            {
                laser.Index++;
                if (laser.Curve.Count - laser.Index <= laser.LaserImage.Length)
                {
                    curve.Add(new Point
                        {
                            X = curve[curve.Count - 1].X * 2 - curve[curve.Count - 2].X,
                            Y = curve[curve.Count - 1].Y * 2 - curve[curve.Count - 2].Y
                        });
                }
                yield return true;
            }
        }

        public static CoroutineFunction<UserSprite, CurveLaser> Homing(UserSprite target, int homingFrame, double homingSpeed, double homingCurveMax)
        {
            return (sp, laser) => Homing(sp, laser, target, homingFrame, homingSpeed, homingCurveMax);
        }

        public static IEnumerator<bool> Homing(UserSprite parent, CurveLaser laser, UserSprite target, int homingFrame, double homingSpeed, double homingCurveMax)
        {
            laser.BrightR = (byte)((long)parent.SourceUser.Id & 0xFF);
            laser.BrightG = (byte)((long)(parent.SourceUser.Id >> 8) & 0xFF);
            laser.BrightB = (byte)((long)(parent.SourceUser.Id >> 16) & 0xFF);
            var curve = laser.Curve;
            while (laser.DrawLength < laser.LaserImage.Length)
            {
                laser.DrawLength++;
                yield return true;
            }
            while (true)
            {
                laser.Index++;
                if (laser.Curve.Count - laser.Index <= laser.LaserImage.Length)
                {
                    var px = curve[curve.Count - 2].X;
                    var py = curve[curve.Count - 2].Y;
                    var lx = curve[curve.Count - 1].X;
                    var ly = curve[curve.Count - 1].Y;
                    var ma = Math.Atan2(ly - py, lx - px);
                    var ta = Math.Atan2(ly - target.Y, lx - target.X);

                    if (homingFrame > 0)
                    {
                        var su = ((ta - ma) + Math.PI * 2) % (Math.PI * 2);
                        var ca = Math.Min(Math.Abs(ta - ma), homingCurveMax);
                        if (su <= Math.PI)
                        {
                            //右サイド
                            curve.Add(new Point
                            {
                                X = lx + Math.Cos(ma - ca) * homingSpeed,
                                Y = ly + Math.Sin(ma - ca) * homingSpeed
                            });
                        }
                        else
                        {
                            //左サイド
                            curve.Add(new Point
                            {
                                X = lx + Math.Cos(ma + ca) * homingSpeed,
                                Y = ly + Math.Sin(ma + ca) * homingSpeed
                            });
                        }
                        homingFrame--;
                    }
                    else
                    {
                        curve.Add(new Point
                        {
                            X = lx + Math.Cos(ma) * homingSpeed,
                            Y = ly + Math.Sin(ma) * homingSpeed
                        });
                    }
                }
                yield return true;
            }
        }
    }
    #endregion
}
