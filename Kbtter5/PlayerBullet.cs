using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using DxLibDLL;
using CoreTweet;
using Kbtter5.Scenes;

namespace Kbtter5
{
    public class PlayerBullet : MultiAdditionalCoroutineSprite
    {
        public PlayerBullet()
        {
            MyKind = ObjectKind.PlayerBullet;
            TargetKind = ObjectKind.Enemy;
        }
    }

    public class PlayerImageBullet : PlayerBullet
    {
        public UserSprite Parent { get; protected set; }
        public int Strength { get; protected set; }

        public PlayerImageBullet(UserSprite pa, CoroutineFunction<UserSprite, PlayerBullet> op, int i, int s)
        {
            Parent = pa;
            SpecialOperation = op(Parent, this);
            Image = i;
            IsImageLoaded = true;
            Strength = s;
            CollisionRadius = 8;
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                ProcessOperations();
                if (X <= -HomeX || X >= HomeX + CommonObjects.StageWidth || Y <= -HomeY || Y >= HomeY + CommonObjects.StageHeight) IsDead = true;

                foreach (var i in ParentManager.OfType<EnemyUser>().Where(p =>
                {
                    var tg = p.MyKind != MyKind && ((p.DamageKind & MyKind) != 0);
                    var xd = X - p.X;
                    var yd = Y - p.Y;
                    var zd = CollisionRadius + p.CollisionRadius;
                    var cl = (xd * xd + yd * yd) < zd * zd;
                    return tg && cl;
                }))
                {
                    i.Damage(Strength);
                    IsDead = true;
                    break;
                }
                yield return true;
            }
        }
    }

    public class PlayerLinearLaser : PlayerBullet
    {
        public UserSprite Parent { get; protected set; }
        public double Length { get; set; }
        public double Thickness { get; set; }
        public byte BrightR { get; set; }
        public byte BrightG { get; set; }
        public byte BrightB { get; set; }

        public PlayerLinearLaser(UserSprite par, CoroutineFunction<UserSprite, PlayerLinearLaser> op, double thickness, int img)
        {
            Parent = par;
            Image = img;
            CollisionRadius = thickness;
            Thickness = thickness;
            SpecialOperation = op(par, this);
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                var ex = X + Math.Cos(Angle) * Length;
                var ey = Y + Math.Sin(Angle) * Length;
                ProcessOperations();
                if ((X <= 0 || X >= CommonObjects.StageWidth || Y <= 0 || Y >= CommonObjects.StageHeight) &&
                    (ex <= 0 || ex >= CommonObjects.StageWidth || ey <= 0 || ey >= CommonObjects.StageHeight))
                    IsDead = true;
                foreach (var i in ParentManager.OfType<EnemyUser>().Where(p =>
                {
                    var vax = ex - X;
                    var vay = ey - Y;
                    var vbx = p.X - X;
                    var vby = p.Y - Y;
                    var r = (vax * vbx + vay * vby) / (vax * vax + vay * vay);
                    double xd = 0, yd = 0;
                    if (r <= 0)
                    {
                        xd = X - p.X;
                        yd = Y - p.Y;
                    }
                    else if (r >= 1)
                    {
                        xd = ex - p.X;
                        yd = ey - p.Y;
                    }
                    else
                    {
                        xd = (X + vax * r) - p.X;
                        yd = (Y + vay * r) - p.Y;
                    }

                    var zd = CollisionRadius + p.CollisionRadius;
                    return ((xd * xd + yd * yd) < zd * zd);
                }))
                {
                    i.Damage(100);
                }
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                var su = new Point { X = ActualX + Math.Cos(Angle - Math.PI / 2) * Thickness / 2, Y = ActualY + Math.Sin(Angle - Math.PI / 2) * Thickness / 2 };
                var sd = new Point { X = ActualX + Math.Cos(Angle + Math.PI / 2) * Thickness / 2, Y = ActualY + Math.Sin(Angle + Math.PI / 2) * Thickness / 2 };
                var eu = new Point
                {
                    X = ActualX + Math.Cos(Angle) * Length + Math.Cos(Angle - Math.PI / 2) * Thickness / 2,
                    Y = ActualY + Math.Sin(Angle) * Length + Math.Sin(Angle - Math.PI / 2) * Thickness / 2
                };
                var ed = new Point
                {
                    X = ActualX + Math.Cos(Angle) * Length + Math.Cos(Angle + Math.PI / 2) * Thickness / 2,
                    Y = ActualY + Math.Sin(Angle) * Length + Math.Sin(Angle + Math.PI / 2) * Thickness / 2
                };
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(ActualAlpha * 255));
                DX.SetDrawBright(BrightR, BrightG, BrightB);
                DX.DrawModiGraphF(
                    (float)su.X, (float)su.Y,
                    (float)eu.X, (float)eu.Y,
                    (float)ed.X, (float)ed.Y,
                    (float)sd.X, (float)sd.Y,
                    Image, DX.TRUE);
                DX.SetDrawBright(255, 255, 255);
                yield return true;
            }
        }
    }
}
