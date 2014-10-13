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
    public class DisplayObject
    {
        public double X { get; set; }
        public double Y { get; set; }
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

        protected bool _dead;
        public virtual bool IsDead
        {
            get { return _dead; }
            set
            {
                _dead = value;
            }
        }

        public int Layer { get; set; }

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

        public Sprite()
        {

        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (DX.CheckHandleASyncLoad(Image) == DX.FALSE)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                    DX.DrawRotaGraph3((int)X, (int)Y, (int)HomeX, (int)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                }
                yield return true;
            }
        }
    }


    public class Bullet : DisplayObject
    {
        public Bullet()
        {
            Layer = 1;
        }
    }

    public class UserSprite : DisplayObject
    {
        public int Image { get; set; }
        public bool ImageLoaded { get; set; }

        public UserSprite()
        {
            Image = CommonObjects.ImageLoadingCircle32;
            HomeX = 16;
            HomeY = 16;
            CollisonRadius = 6;
            GrazeRadius = 8;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (DX.CheckHandleASyncLoad(Image) == DX.FALSE)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                    DX.DrawRotaGraph3((int)X, (int)Y, (int)HomeX, (int)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                }
                yield return true;
            }
        }
    }

    public class EnemyUser : UserSprite
    {
        public Status SourceStatus { get; protected set; }
        public IEnumerator<bool> Operation { get; protected set; }
        private SceneGame game;
        public int Health { get; protected set; }
        public int TotalHealth { get; protected set; }
        private static Random rnd = new Random();

        public EnemyUser(SceneGame sc, Status s, EnemyPattern op)
        {
            game = sc;
            SourceStatus = s;
            Operation = op(this);
            MyKind = ObjectKind.Enemy;
            TargetKind = ObjectKind.Player;
            DamageKind = ObjectKind.PlayerBullet;
            CollisonRadius = 10;
            GrazeRadius = 14;
            TotalHealth = Health = 1000 + (SourceStatus.User.StatusesCount / 10) + (DateTime.Now - SourceStatus.User.CreatedAt.LocalDateTime).Days * 3;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceStatus.User);
                ImageLoaded = true;
            });
        }

        public void AddBullet(Bullet b)
        {
            game.AddBullet(b);
        }

        public void AddObject(DisplayObject obj)
        {
            game.AddObject(obj);
        }

        public PlayerUser Player { get { return game.Player; } }

        public IReadOnlyList<DisplayObject> GameObjects { get { return game.Objects; } }

        public override IEnumerator<bool> Tick()
        {
            while (!(IsDead = !(!IsDead && Operation.MoveNext() && Operation.Current)))
            {
                foreach (var i in GameObjects.Where(p => p.MyKind != MyKind && p.DamageKind.HasFlag(MyKind)))
                {
                    var xd = X - i.X;
                    var yd = Y - i.Y;
                    var zd = CollisonRadius + i.CollisonRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        i.IsDead = true;
                        break;
                    }
                    zd = GrazeRadius + i.GrazeRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        Player.Graze();
                    }
                }
                yield return true;
            }
        }
        /*
        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (DX.CheckHandleASyncLoad(Image) == DX.FALSE)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                    DX.DrawRotaGraph3((int)X, (int)Y, (int)HomeX, (int)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                }
                DX.DrawStringToHandle((int)X, (int)Y, Health.ToString(), DX.GetColor(255, 0, 0), CommonObjects.FontSystem);
                yield return true;
            }
        }
        */
        public void Damage(int point)
        {
            Health -= point;
            game.Score(point / 100 * 10);
            game.AddObject(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, point / 100 * 10) { X = X, Y = Y });
            if (Health <= 0)
            {
                game.AddObject(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, TotalHealth / 10 * 10) { X = X, Y = Y });
                game.Score(TotalHealth / 10 * 10);
                IsDead = true;
                var ofs = rnd.NextDouble() * Math.PI * 2;
                for (int i = 0; i < 5; i++)
                {
                    game.AddObject(new CoroutineSprite(SpritePatterns.MissStar(ofs + Math.PI * 2.0 / 5.0 * i, this)) { Image = CommonObjects.ImageStar, X = X, Y = Y, HomeX = 8, HomeY = 8 });
                }
            }
        }
    }

    public class PlayerUser : UserSprite
    {
        private SceneGame game;
        public User SourceUser { get; protected set; }
        private Random rnd = new Random();
        public int chain = 2;
        private IEnumerator<bool> SpecialOperation;
        private bool visible = true;
        private int ShotStrength { get; set; }
        private int GrazePoint;

        public override bool IsDead
        {
            get { return false; }
            set { if (value) Kill(); }
        }

        public IReadOnlyList<DisplayObject> GameObjects { get { return game.Objects; } }

        public PlayerUser(SceneGame sc, User u)
        {
            game = sc;
            SourceUser = u;
            MyKind = ObjectKind.Player;
            DamageKind = ObjectKind.Enemy | ObjectKind.EnemyBullet;
            CollisonRadius = 4.0 * (SourceUser.FriendsCount / SourceUser.FollowersCount);
            GrazeRadius = CollisonRadius * 1.5;
            ShotStrength = (SourceUser.StatusesCount + (DateTime.Now - SourceUser.CreatedAt.LocalDateTime).Days * (int)Math.Log10(SourceUser.StatusesCount)) / 25;

            GrazePoint = (SourceUser.StatusesCount / SourceUser.FollowersCount) / 20 + 10;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceUser);
                ImageLoaded = true;
            });
        }

        public void Graze()
        {

            game.AddObject(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, GrazePoint) { X = X, Y = Y });
            game.Score(GrazePoint);
        }

        public void Kill()
        {
            var ofs = rnd.NextDouble() * Math.PI * 2;
            for (int i = 0; i < 5; i++)
            {
                game.AddObject(new CoroutineSprite(SpritePatterns.MissStar(ofs + Math.PI * 2.0 / 5.0 * i, this)) { Image = CommonObjects.ImageStar, X = X, Y = Y, HomeX = 8, HomeY = 8 });
            }
            SpecialOperation = MissOut();
        }

        private IEnumerator<bool> MissOut()
        {
            visible = false;
            DamageKind = ObjectKind.None;
            for (int i = 0; i < 100; i++)
            {
                ScaleX += 5.0 / 100.0;
                ScaleY += 5.0 / 100.0;
                Alpha -= 0.01;
                yield return true;
            }
            ScaleX = 1;
            ScaleY = 1;

            //無敵
            visible = true;
            Alpha = 0.5;
            for (int i = 0; i < 120; i++) yield return true;

            Alpha = 1;
            DamageKind = ObjectKind.Enemy | ObjectKind.EnemyBullet;
        }

        public override IEnumerator<bool> Tick()
        {
            int count = 0;
            while (true)
            {
                int x, y;
                DX.GetMousePoint(out x, out y);
                if (visible)
                {
                    X = x;
                    Y = y;
                }

                if ((DX.GetMouseInput() & DX.MOUSE_INPUT_LEFT) != 0 && count % chain == 0)
                {
                    var dr = (count / chain) % 8;
                    if (DX.CheckHitKey(DX.KEY_INPUT_D) == 1) dr = 0;
                    if (DX.CheckHitKey(DX.KEY_INPUT_C) == 1) dr = 1;
                    if (DX.CheckHitKey(DX.KEY_INPUT_X) == 1) dr = 2;
                    if (DX.CheckHitKey(DX.KEY_INPUT_Z) == 1) dr = 3;
                    if (DX.CheckHitKey(DX.KEY_INPUT_A) == 1) dr = 4;
                    if (DX.CheckHitKey(DX.KEY_INPUT_Q) == 1) dr = 5;
                    if (DX.CheckHitKey(DX.KEY_INPUT_W) == 1) dr = 6;
                    if (DX.CheckHitKey(DX.KEY_INPUT_E) == 1) dr = 7;

                    game.AddBullet(new PlayerImageBullet(
                        this,
                        CommonObjects.ImageShot,
                        BulletPatterns.Linear(Math.PI / 4 * dr, 8, 90),
                        ShotStrength)
                        {
                            X = X,
                            Y = Y,
                            HomeX = 8,
                            HomeY = 8,
                            MyKind = ObjectKind.PlayerBullet,
                            TargetKind = ObjectKind.Enemy
                        });
                }
                if (SpecialOperation != null)
                {
                    SpecialOperation = (SpecialOperation.MoveNext() && SpecialOperation.Current) ? SpecialOperation : null;
                }
                count++;
                yield return true;
            }
        }
        /*
        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (DX.CheckHandleASyncLoad(Image) == DX.FALSE)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                    DX.DrawRotaGraph3((int)X, (int)Y, (int)HomeX, (int)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                }
                DX.DrawStringToHandle((int)X, (int)Y, ShotStrength.ToString(), DX.GetColor(255, 0, 0), CommonObjects.FontSystem);
                yield return true;
            }
        }
        */
    }

    public class CharacterBullet : Bullet
    {
        public char Character { get; protected set; }
        public EnemyUser Parent { get; protected set; }
        public double Size { get; protected set; }
        public IEnumerator<bool> Operation { get; protected set; }

        public CharacterBullet(EnemyUser parent, char c, BulletPattern op)
        {
            Parent = parent;
            MyKind = ObjectKind.EnemyBullet;
            TargetKind = ObjectKind.Player;
            Operation = op(Parent, this);
            Character = c;
            Size = 20;
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
                foreach (var i in Parent.GameObjects.Where(p => p.MyKind != MyKind && p.DamageKind.HasFlag(MyKind)))
                {
                    var xd = X - i.X;
                    var yd = Y - i.Y;
                    var zd = CollisonRadius + i.CollisonRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        i.IsDead = true;
                        IsDead = true;
                        break;
                    }
                    zd = GrazeRadius + i.GrazeRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        Parent.Player.Graze();
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
                DX.DrawStringToHandle((int)(X - HomeX), (int)(Y - HomeY), Character.ToString(), DX.GetColor(255, 255, 255), CommonObjects.FontBullet);
                yield return true;
            }
        }
    }

    public class PlayerImageBullet : Bullet
    {
        public int Image { get; protected set; }
        public PlayerUser Parent { get; protected set; }
        public IEnumerator<bool> Operation { get; protected set; }
        public int Strength { get; protected set; }

        public PlayerImageBullet(PlayerUser pa, int i, BulletPattern op, int s)
        {
            Parent = pa;
            Operation = op(Parent, this);
            Image = i;
            Strength = s;
            CollisonRadius = 8;
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);
                if (X <= -HomeX || X >= HomeX + 640 || Y <= -HomeY || Y >= HomeY + 480) IsDead = true;
                foreach (var i in Parent.GameObjects.Where(p => p.MyKind != MyKind && p.DamageKind.HasFlag(MyKind)).OfType<EnemyUser>())
                {
                    var xd = X - i.X;
                    var yd = Y - i.Y;
                    var zd = CollisonRadius + i.CollisonRadius;
                    if ((xd * xd + yd * yd) < zd * zd)
                    {
                        i.Damage(Strength);
                        IsDead = true;
                        break;
                    }
                }
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (DX.CheckHandleASyncLoad(Image) == DX.FALSE)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)(Alpha * 255));
                    DX.DrawRotaGraph3((int)X, (int)Y, (int)HomeX, (int)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
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
                    DX.DrawGraph((int)(X + DigitX * i - HomeX), (int)(Y - HomeY), NumberImage[(Digits - 1 - i < reald) ? v % 10 : FillWithZero ? 0 : 10], DX.TRUE);
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
                DX.DrawStringToHandle((int)(X - HomeX), (int)(Y - HomeY), Value, DX.GetColor(255, 255, 255), FontHandle);
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