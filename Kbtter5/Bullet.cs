using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using DxLibDLL;
using CoreTweet;


namespace Kbtter5
{
    public class Bullet : Sprite
    {
        public IEnumerator<bool> Operation { get; protected set; }
        public Bullet()
        {
        }
    }

    public class CharacterBullet : Bullet
    {
        private LetterInformation buffered;
        public char Character
        {
            set { CommonObjects.TextureFontBullet.Letters.TryGetValue(value, out buffered); }
        }
        public EnemyUser Parent { get; protected set; }
        public double Size { get; protected set; }

        public CharacterBullet(EnemyUser parent, CoroutineFunction<UserSprite, Bullet> op, char c)
        {
            Parent = parent;
            MyKind = ObjectKind.EnemyBullet;
            TargetKind = ObjectKind.Player;
            Operation = op(Parent, this);
            Character = c;
            Size = 20;
            HomeX = buffered.Width / 2.0;
            HomeY = buffered.Height / 2.0;
            CollisionRadius = 4;
            GrazeRadius = 8;
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);
                if (X <= -Size || X >= Size + CommonObjects.StageWidth || Y <= -Size || Y >= Size + CommonObjects.StageHeight) IsDead = true;
                if (Parent.Player.HasCollision)
                {
                    var xd = X - Parent.Player.X;
                    var yd = Y - Parent.Player.Y;
                    var zd = CollisionRadius + Parent.Player.CollisionRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        Parent.Player.Kill();
                    }
                    else
                    {
                        zd = GrazeRadius + Parent.Player.GrazeRadius;
                        if ((xd * xd + yd * yd) < zd * zd)
                        {
                            Parent.Player.Graze();
                        }
                    }
                }
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                DX.DrawGraphF((float)(ActualX - HomeX), (float)(ActualY - HomeY), buffered.Handle, DX.TRUE);
                yield return true;
            }
        }
    }

    public class LinearLaser : Bullet
    {
        public EnemyUser Parent { get; protected set; }
        public double Length { get; set; }
        public double Thickness { get; set; }
        public byte BrightR { get; set; }
        public byte BrightG { get; set; }
        public byte BrightB { get; set; }

        public LinearLaser(EnemyUser par, CoroutineFunction<UserSprite, LinearLaser> op, double thickness, int img)
        {
            Parent = par;
            Image = img;
            CollisionRadius = thickness;
            Thickness = thickness;
            Operation = op(par, this);
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);

                var ex = X + Math.Cos(Angle) * Length;
                var ey = Y + Math.Sin(Angle) * Length;
                if ((X <= 0 || X >= CommonObjects.StageWidth || Y <= 0 || Y >= CommonObjects.StageHeight) &&
                    (ex <= 0 || ex >= CommonObjects.StageWidth || ey <= 0 || ey >= CommonObjects.StageHeight))
                    IsDead = true;
                if (Parent.Player.HasCollision)
                {
                    var vax = ex - X;
                    var vay = ey - Y;
                    var vbx = Parent.Player.X - X;
                    var vby = Parent.Player.Y - Y;
                    var r = (vax * vbx + vay * vby) / (vax * vax + vay * vay);
                    double xd = 0, yd = 0;
                    if (r <= 0)
                    {
                        xd = X - Parent.Player.X;
                        yd = Y - Parent.Player.Y;
                    }
                    else if (r >= 1)
                    {
                        xd = ex - Parent.Player.X;
                        yd = ey - Parent.Player.Y;
                    }
                    else
                    {
                        xd = (X + vax * r) - Parent.Player.X;
                        yd = (Y + vay * r) - Parent.Player.Y;
                    }

                    var zd = CollisionRadius + Parent.Player.CollisionRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        Parent.Player.Kill();
                    }
                    else
                    {
                        zd = GrazeRadius + Parent.Player.GrazeRadius;
                        if ((xd * xd + yd * yd) < zd * zd)
                        {
                            Parent.Player.Graze();
                        }
                    }
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
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
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

    public class CurveLaser : Bullet
    {
        public EnemyUser Parent { get; protected set; }
        public double Thickness { get; set; }
        public byte BrightR { get; set; }
        public byte BrightG { get; set; }
        public byte BrightB { get; set; }
        public CurveLaserImage LaserImage { get; set; }
        public IList<Point> Curve { get; set; }
        public int Index { get; set; }
        public int DrawLength { get; set; }

        public CurveLaser(EnemyUser par, ICurve curve, CoroutineFunction<UserSprite, CurveLaser> op, double col, CurveLaserImage img)
        {
            Parent = par;
            LaserImage = img;
            CollisionRadius = col;
            Thickness = col;
            Operation = op(par, this);
            Curve = curve.Points.ToList();
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);
                if (!IsDead && Parent.Player.HasCollision)
                {
                    IsDead = true;
                    for (int i = 0; i < DrawLength && i + Index + 1 < Curve.Count - 1; i++)
                    {
                        var ex = Curve[i + Index + 1].X;
                        var ey = Curve[i + Index + 1].Y;
                        if ((X > 0 && X < CommonObjects.StageWidth && Y > 0 && Y < CommonObjects.StageHeight) || (ex > 0 && ex < CommonObjects.StageWidth && ey > 0 && ey < CommonObjects.StageHeight)) IsDead = false;

                        var vax = ex - Curve[i + Index].X;
                        var vay = ey - Curve[i + Index].Y;
                        var vbx = Parent.Player.X - Curve[i + Index].X;
                        var vby = Parent.Player.Y - Curve[i + Index].Y;
                        var r = (vax * vbx + vay * vby) / (vax * vax + vay * vay);
                        double xd = 0, yd = 0;
                        if (r <= 0)
                        {
                            xd = X - Parent.Player.X;
                            yd = Y - Parent.Player.Y;
                        }
                        else if (r >= 1)
                        {
                            xd = ex - Parent.Player.X;
                            yd = ey - Parent.Player.Y;
                        }
                        else
                        {
                            xd = (Curve[i].X + vax * r) - Parent.Player.X;
                            yd = (Curve[i].Y + vay * r) - Parent.Player.Y;
                        }

                        var zd = CollisionRadius + Parent.Player.CollisionRadius;
                        if ((xd * xd + yd * yd) < zd * zd)
                        {
                            Parent.Player.Kill();
                        }
                        else
                        {
                            zd = GrazeRadius + Parent.Player.GrazeRadius;
                            if ((xd * xd + yd * yd) < zd * zd)
                            {
                                Parent.Player.Graze();
                            }
                        }
                    }
                }
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                DX.SetDrawBright(BrightR, BrightG, BrightB);
                Point? pu = null, pd = null;
                for (int i = 0; i < DrawLength && i + Index + 1 < Curve.Count; i++)
                {
                    var ax = Curve[i + Index].X + ParentManager.OffsetX;
                    var ay = Curve[i + Index].Y + ParentManager.OffsetY;
                    var eax = Curve[i + Index + 1].X + ParentManager.OffsetX;
                    var eay = Curve[i + Index + 1].Y + ParentManager.OffsetY;
                    var ang = Math.Atan2(eay - ay, eax - ax);

                    var su = pu ?? new Point { X = ax + Math.Cos(ang - Math.PI / 2) * Thickness / 2, Y = ay + Math.Sin(ang - Math.PI / 2) * Thickness / 2 };
                    var sd = pd ?? new Point { X = ax + Math.Cos(ang + Math.PI / 2) * Thickness / 2, Y = ay + Math.Sin(ang + Math.PI / 2) * Thickness / 2 };
                    var eu = new Point
                    {
                        X = eax + Math.Cos(ang - Math.PI / 2) * Thickness / 2,
                        Y = eay + Math.Sin(ang - Math.PI / 2) * Thickness / 2
                    };
                    var ed = new Point
                    {
                        X = eax + Math.Cos(ang + Math.PI / 2) * Thickness / 2,
                        Y = eay + Math.Sin(ang + Math.PI / 2) * Thickness / 2
                    };

                    pu = eu;
                    pd = ed;

                    DX.DrawModiGraphF(
                        (float)su.X, (float)su.Y,
                        (float)eu.X, (float)eu.Y,
                        (float)ed.X, (float)ed.Y,
                        (float)sd.X, (float)sd.Y,
                        LaserImage.Images[i], DX.TRUE);
                }
                DX.SetDrawBright(255, 255, 255);
                yield return true;
            }
        }
    }

    public class CurveCharacterLaser : CurveLaser
    {
        public Status SourceStatus { get; private set; }
        private LetterInformation[] Letters;

        public CurveCharacterLaser(EnemyUser par, ICurve curve, CoroutineFunction<UserSprite, CurveLaser> op, Status st)
            : base(par, curve, op, 0, null)
        {
            CollisionRadius = 4;
            GrazeRadius = 8;
            SourceStatus = st;
            Letters = st.Text.Select(p =>
            {
                LetterInformation info;
                CommonObjects.TextureFontBullet.Letters.TryGetValue(p, out info);
                return info;
            }).ToArray();
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                DX.SetDrawBright(BrightR, BrightG, BrightB);
                for (int i = 0; i < DrawLength && i + Index < Curve.Count; i++)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                    DX.DrawGraphF((float)(ActualX - HomeX), (float)(ActualY - HomeY), Letters[i % Letters.Length].Handle, DX.TRUE);
                }
                DX.SetDrawBright(255, 255, 255);
                yield return true;
            }
        }
    }
}
