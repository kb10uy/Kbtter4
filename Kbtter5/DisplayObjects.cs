﻿using System;
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
    public class DisplayObject
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double ActualX { get { return X + ParentManager.OffsetX; } }
        public double ActualY { get { return Y + ParentManager.OffsetY; } }
        public double HomeX { get; set; }
        public double HomeY { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double Angle { get; set; }
        public double Alpha { get; set; }
        public ObjectKind MyKind { get; set; }
        public ObjectKind TargetKind { get; set; }
        public ObjectKind DamageKind { get; set; }
        public double CollisonRadius { get; set; }
        public double GrazeRadius { get; set; }
        public ObjectManager ParentManager { get; set; }
        public int Layer { get; set; }
        public virtual bool IsDead { get; set; }

        public IEnumerator<bool> TickCoroutine { get; protected set; }
        public virtual IEnumerator<bool> Tick()
        {
            while (true) yield return true;
        }

        public IEnumerator<bool> DrawCoroutine { get; protected set; }
        public virtual IEnumerator<bool> Draw()
        {
            while (true) yield return true;
        }

        public DisplayObject()
        {
            ScaleX = 1;
            ScaleY = 1;
            Alpha = 1;
            TickCoroutine = Tick();
            DrawCoroutine = Draw();
            MyKind = ObjectKind.None;
            TargetKind = ObjectKind.None;
            DamageKind = ObjectKind.None;
        }
    }

    public class Sprite : DisplayObject
    {
        public int Image { get; set; }
        protected bool IsImageLoaded { get; set; }

        public Sprite()
        {

        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (IsImageLoaded)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                    DX.DrawRotaGraph3F((float)ActualX, (float)ActualY, (float)HomeX, (float)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                }
                else
                {
                    IsImageLoaded = DX.CheckHandleASyncLoad(Image) == DX.FALSE;
                }
                yield return true;
            }
        }
    }


    public class Bullet : Sprite
    {
        public IEnumerator<bool> Operation { get; protected set; }
        public Bullet()
        {
        }
    }

    public class UserSprite : Sprite
    {
        protected static int EnemyBulletLayer = (int)GameLayer.EnemyBullet;
        protected static int EnemyLayer = (int)GameLayer.Enemy;
        protected static int PlayerLayer = (int)GameLayer.Player;
        protected static int PlayerBulletLayer = (int)GameLayer.PlayerBullet;
        protected static int EffectLayer = (int)GameLayer.Effect;

        public UserSprite()
        {
            Image = CommonObjects.ImageLoadingCircle32;
            HomeX = 16;
            HomeY = 16;
            CollisonRadius = 6;
            GrazeRadius = 8;
        }
    }

    public class EnemyUser : UserSprite
    {
        public Status SourceStatus { get; protected set; }
        public IEnumerator<bool> Operation { get; protected set; }
        private SceneGame game;
        public EnemyUser ParentEnemy { get; protected set; }
        public bool DieWithParentDeath { get; set; }
        public int Health { get; protected set; }
        public int TotalHealth { get; protected set; }
        private static Random rnd = new Random();

        public EnemyUser()
        {
            MyKind = ObjectKind.Enemy;
            TargetKind = ObjectKind.Player;
            DamageKind = ObjectKind.PlayerBullet;
            CollisonRadius = 10;
            GrazeRadius = 14;
            DieWithParentDeath = false;
        }

        public EnemyUser(SceneGame sc, Status s, EnemyPattern op)
            : this()
        {
            game = sc;
            SourceStatus = s;
            Operation = op(this);
            TotalHealth = Health = 1000 + (SourceStatus.User.StatusesCount / 10) + (DateTime.Now - SourceStatus.User.CreatedAt.LocalDateTime).Days * 3;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceStatus.User);
                IsImageLoaded = true;
            });
        }

        public EnemyUser(EnemyUser sc, Status s, EnemyPattern op)
            : this()
        {
            ParentEnemy = sc;
            game = sc.game;
            SourceStatus = s;
            Operation = op(this);
            TotalHealth = Health = 1000 + (SourceStatus.User.StatusesCount / 10) + (DateTime.Now - SourceStatus.User.CreatedAt.LocalDateTime).Days * 3;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceStatus.User);
                IsImageLoaded = true;
            });
        }

        public PlayerUser Player { get { return game.Player; } }

        public override IEnumerator<bool> Tick()
        {
            while (!(IsDead = !(!IsDead && Operation.MoveNext() && Operation.Current)))
            {
                if (DieWithParentDeath && ParentEnemy.IsDead) IsDead = true;
                if (Player.HasCollision)
                {
                    var xd = X - Player.X;
                    var yd = Y - Player.Y;
                    var zd = CollisonRadius + Player.CollisonRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        Player.Kill();
                    }
                    else
                    {
                        zd = GrazeRadius + Player.GrazeRadius;
                        if ((xd * xd + yd * yd) < zd * zd)
                        {
                            Player.Graze();
                        }
                    }
                }
                yield return true;
            }
        }

        public void Damage(int point)
        {
            Health -= point;
            game.Score(point / 100 * 10);
            ParentManager.Add(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, point / 100 * 10) { X = X, Y = Y }, EffectLayer);
            if (Health <= 0)
            {
                ParentManager.Add(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, TotalHealth / 10 * 10) { X = X, Y = Y }, EffectLayer);
                game.Score(TotalHealth / 10 * 10);
                game.DestroyEnemy();
                IsDead = true;
                var ofs = rnd.NextDouble() * Math.PI * 2;
                for (int i = 0; i < 5; i++)
                {
                    ParentManager.Add(new CoroutineSprite(SpritePatterns.MissStar(ofs + Math.PI * 2.0 / 5.0 * i, this))
                    {
                        Image = CommonObjects.ImageStar,
                        X = X,
                        Y = Y,
                        HomeX = 8,
                        HomeY = 8
                    }, EffectLayer);
                }
            }
        }
    }

    public class PlayerUser : UserSprite
    {
        private Random rnd = new Random();
        private SceneGame game;
        public User SourceUser { get; protected set; }
        public int ShotInterval { get; set; }
        public IEnumerator<bool> Operation { get; protected set; }
        public IEnumerator<bool> SpecialOperation { get; protected set; }
        public bool Operatable { get; protected set; }
        public int ShotStrength { get; protected set; }
        private int GrazePoint;
        public bool IsGameOver { get; protected set; }
        public bool HasCollision { get; protected set; }

        public PlayerUser(SceneGame sc, User u, PlayerOperation op)
        {
            game = sc;
            SourceUser = u;
            Operation = op(this);
            MyKind = ObjectKind.Player;
            DamageKind = ObjectKind.Enemy | ObjectKind.EnemyBullet;
            CollisonRadius = 4.0 * (SourceUser.FriendsCount / SourceUser.FollowersCount);
            GrazeRadius = CollisonRadius * 1.5;
            ShotStrength = (SourceUser.StatusesCount + (DateTime.Now - SourceUser.CreatedAt.LocalDateTime).Days * (int)Math.Log10(SourceUser.StatusesCount)) / 25;
            ShotInterval = 2;
            Operatable = true;
            HasCollision = true;
            GrazePoint = (SourceUser.StatusesCount / SourceUser.FollowersCount) / 20 + 10;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceUser);
                IsImageLoaded = true;
            });
        }

        public void Graze()
        {
            ParentManager.Add(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, GrazePoint) { X = X, Y = Y }, EffectLayer);
            game.Score(GrazePoint);
            game.Graze();
        }

        public void Kill()
        {
            var ofs = rnd.NextDouble() * Math.PI * 2;
            for (int i = 0; i < 5; i++)
            {
                ParentManager.Add(new CoroutineSprite(SpritePatterns.MissStar(ofs + Math.PI * 2.0 / 5.0 * i, this))
                {
                    Image = CommonObjects.ImageStar,
                    X = X,
                    Y = Y,
                    HomeX = 8,
                    HomeY = 8
                }, EffectLayer);
            }
            SpecialOperation = MissOut();
            IsGameOver = !game.Miss();
        }

        private IEnumerator<bool> MissOut()
        {
            Operatable = false;
            DisableCollision(1.0);
            for (int i = 0; i < 100; i++)
            {
                ScaleX += 5.0 / 100.0;
                ScaleY += 5.0 / 100.0;
                Alpha -= 0.01;
                yield return true;
            }

            while (IsGameOver) yield return true;

            ScaleX = 1;
            ScaleY = 1;
            //無敵
            Operatable = true;
            DisableCollision();
            for (int i = 0; i < 120; i++) yield return true;

            EnableCollision();
        }

        public void EnableCollision()
        {
            HasCollision = true;
            Alpha = 1;
            DamageKind = ObjectKind.Enemy | ObjectKind.EnemyBullet;
        }

        public void DisableCollision()
        {
            HasCollision = false;
            Alpha = 0.5;
            DamageKind = ObjectKind.None;
        }

        public void DisableCollision(double al)
        {
            HasCollision = false;
            Alpha = al;
            DamageKind = ObjectKind.None;
        }

        public void TryBomb()
        {
            if (SpecialOperation == null && game.UseBomb()) SpecialOperation = UseBomb();
        }

        private IEnumerator<bool> UseBomb()
        {
            DisableCollision();

            for (int i = 0; i < 16; i++)
            {
                var x = rnd.Next(20, 620);
                var y = rnd.Next(20, 460);
                var xd = x - X;
                var yd = y - Y;

                var at = Math.Atan2(yd, xd);
                var sp = Math.Sqrt(xd * xd + yd * yd) / 60.0;
                ParentManager.Add(new PlayerImageBullet(this, CommonObjects.ImageStar, BulletPatterns.LazyHomingToEnemy(this, at, sp, 60, 10), SourceUser.StatusesCount)
                {
                    ScaleX = 8.0,
                    ScaleY = 8.0,
                    CollisonRadius = 56,
                    HomeX = 8,
                    HomeY = 8,
                    X = X,
                    Y = Y
                }, PlayerBulletLayer);
            }
            for (int i = 0; i < 300; i++) yield return true;

            EnableCollision();
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                Operation.MoveNext();
                SpecialOperation = (SpecialOperation != null && SpecialOperation.MoveNext() && SpecialOperation.Current) ? SpecialOperation : null;
                yield return true;
            }
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

        public CharacterBullet(EnemyUser parent, char c, BulletPattern op)
        {
            Parent = parent;
            MyKind = ObjectKind.EnemyBullet;
            TargetKind = ObjectKind.Player;
            Operation = op(Parent, this);
            Character = c;
            Size = 32;
            HomeX = Size / 2;
            HomeY = Size / 2;
            CollisonRadius = 4;
            GrazeRadius = 6;
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);
                if (X <= -Size || X >= Size + 640 || Y <= -Size || Y >= Size + 480) IsDead = true;
                if (Parent.Player.HasCollision)
                {
                    var xd = X - Parent.Player.X;
                    var yd = Y - Parent.Player.Y;
                    var zd = CollisonRadius + Parent.Player.CollisonRadius;
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
                //DX.DrawStringFToHandle((float)(ActualX - HomeX), (float)(ActualY - HomeY), buffered, CommonObjects.Colors.White, CommonObjects.FontBullet);
                DX.DrawGraphF((float)(ActualX - HomeX + buffered.OffsetX), (float)(ActualY - HomeY + buffered.OffsetY), buffered.Handle, DX.TRUE);
                yield return true;
            }
        }
    }

    public class LinearLaser : Bullet
    {
        public EnemyUser Parent { get; protected set; }
        public double Length { get; set; }
        public double Thickness { get; set; }

        public LinearLaser(EnemyUser par, int img, double thickness, LinearLaserPattern op)
        {
            Parent = par;
            Image = img;
            CollisonRadius = thickness;
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
                if ((X <= 0 || X >= 640 || Y <= 0 || Y >= 480) && (ex <= 0 || ex >= 640 || ey <= 0 || ey >= 480)) IsDead = true;
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

                    var zd = CollisonRadius + Parent.Player.CollisonRadius;
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
                DX.DrawModiGraphF(
                    (float)su.X, (float)su.Y,
                    (float)eu.X, (float)eu.Y,
                    (float)ed.X, (float)ed.Y,
                    (float)sd.X, (float)sd.Y,
                    Image, DX.TRUE);
                yield return true;
            }
        }
    }

    public class PlayerImageBullet : Bullet
    {
        public PlayerUser Parent { get; protected set; }
        public int Strength { get; protected set; }

        public PlayerImageBullet(PlayerUser pa, int i, BulletPattern op, int s)
        {
            Parent = pa;
            Operation = op(Parent, this);
            Image = i;
            IsImageLoaded = true;
            Strength = s;
            CollisonRadius = 8;
            MyKind = ObjectKind.PlayerBullet;
            TargetKind = ObjectKind.Enemy;
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);
                if (X <= -HomeX || X >= HomeX + 640 || Y <= -HomeY || Y >= HomeY + 480) IsDead = true;

                foreach (var i in ParentManager.OfType<EnemyUser>().Where(p =>
                {
                    var tg = p.MyKind != MyKind && ((p.DamageKind & MyKind) != 0);
                    var xd = X - p.X;
                    var yd = Y - p.Y;
                    var zd = CollisonRadius + p.CollisonRadius;
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

    public class NumberSprite : DisplayObject
    {
        public int Digits { get; set; }
        public double DigitX { get; protected set; }
        public double DigitY { get; protected set; }
        public int Value { get; set; }
        public bool FillWithZero { get; set; }
        public IReadOnlyList<int> NumberImage { get; protected set; }

        public NumberSprite(IReadOnlyList<int> img, int x, int y, int digits)
        {
            Digits = digits;
            FillWithZero = false;
            NumberImage = img;
            Value = 0;
            DigitX = x;
            DigitY = y;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                var v = Value;
                var reald = (int)(Math.Log10(Value) + 1);
                int[] ls = new int[Digits];
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                for (int i = Digits - 1; i >= 0; i--)
                {
                    DX.DrawGraphF((float)(ActualX + DigitX * i - HomeX), (float)(ActualY - HomeY), NumberImage[(Digits - 1 - i < reald) ? v % 10 : FillWithZero ? 0 : 10], DX.TRUE);
                    v /= 10;
                }
                yield return true;
            }
        }
    }

    public class ScoreSprite : NumberSprite
    {
        public ScoreSprite(IReadOnlyList<int> img, int x, int y, int pt)
            : base(img, x, y, (int)(Math.Log10(pt) + 1))
        {
            HomeX = x * Digits / 2;
            HomeY = y / 2;
            Value = pt;
        }

        public override IEnumerator<bool> Tick()
        {
            for (int i = 0; i < 20; i++)
            {
                Y--;
                Alpha -= 1.0 / 20.0;
                yield return true;
            }
        }
    }

    public class StringSprite : DisplayObject
    {
        public string Value { get; set; }
        public int FontHandle { get; set; }
        public int Color { get; set; }

        public StringSprite(int font, int col)
        {
            FontHandle = font;
            Color = col;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                DX.DrawStringFToHandle((float)(ActualX - HomeX), (float)(ActualY - HomeY), Value, Color, FontHandle);
                yield return true;
            }
        }
    }

    public class CoroutineSprite : Sprite
    {
        public IEnumerator<bool> Operation { get; protected set; }

        public CoroutineSprite(SpritePattern op)
        {
            Operation = op(this);
        }

        public override IEnumerator<bool> Tick()
        {
            while (!(IsDead = !(Operation.MoveNext() && Operation.Current))) yield return true;
        }
    }

    public class CircleObject : DisplayObject
    {
        public double Radius { get; set; }
        public int Color { get; set; }
        public bool AllowFill { get; set; }

        public CircleObject()
        {
            Radius = 0;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                DX.DrawOval((int)(ActualX - HomeX), (int)(ActualY - HomeY), (int)Radius, (int)Radius, Color, AllowFill ? DX.TRUE : DX.FALSE);
                yield return true;
            }
        }
    }

    public class LineObject : DisplayObject
    {
        public double Length { get; set; }
        public int Color { get; set; }

        public LineObject()
        {
            Length = 0;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                var dx = Math.Cos(Angle) * Length;
                var dy = Math.Sin(Angle) * Length;
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                DX.DrawLine((int)(ActualX - HomeX), (int)(ActualY - HomeY), (int)(ActualX - HomeX + dx), (int)(ActualY - HomeY + dy), Color);
                yield return true;
            }
        }
    }

    public struct Point
    {
        public double X;
        public double Y;
    }

    [Flags]
    public enum ObjectKind
    {
        None = 1,
        Player = 2,
        Enemy = 4,
        PlayerBullet = 8,
        EnemyBullet = 16,
    }
}