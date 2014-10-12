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

        private bool dead;
        public bool IsDead
        {
            get { return dead; }
            set
            {
                dead = value;
            }
        }

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
        }
    }

    public class Bullet : DisplayObject
    {
        public ObjectKind MyKind { get; set; }
        public ObjectKind TargetKind { get; set; }
    }

    public class UserSprite : DisplayObject
    {
        public int Image { get; set; }
        public bool ImageLoaded { get; set; }
        public ObjectKind MyKind { get; set; }
        public ObjectKind DamageKind { get; set; }

        public UserSprite()
        {
            Image = CommonObjects.ImageLoadingCircle32;
            HomeX = 16;
            HomeY = 16;
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (DX.CheckHandleASyncLoad(Image) == DX.FALSE)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)Alpha * 255);
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

        public EnemyUser(SceneGame sc, Status s, EnemyPattern op)
        {
            game = sc;
            SourceStatus = s;
            Operation = op(this);
            MyKind = ObjectKind.Enemy;
            DamageKind = ObjectKind.PlayerBullet;
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

        public PlayerUser Player { get { return game.Player; } }

        public override IEnumerator<bool> Tick()
        {
            while (!(IsDead = !(Operation.MoveNext() && Operation.Current))) yield return true;
        }
    }

    public class PlayerUser : UserSprite
    {
        private SceneGame game;
        public User SourceUser { get; protected set; }
        public int chain = 2;

        public PlayerUser(SceneGame sc, User u)
        {
            game = sc;
            SourceUser = u;
            MyKind = ObjectKind.Player;
            DamageKind = ObjectKind.Enemy | ObjectKind.EnemyBullet;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceUser);
                ImageLoaded = true;
            });
        }

        public override IEnumerator<bool> Tick()
        {
            int count = 0;
            while (true)
            {
                int x, y;
                DX.GetMousePoint(out x, out y);
                X = x;
                Y = y;
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

                    game.AddBullet(new ImageBullet(this, CommonObjects.ImageShot, BulletPatterns.Linear(Math.PI / 4 * dr, 8, 90)) { X = X, Y = Y, HomeX = 8, HomeY = 8 });
                }
                count++;
                yield return true;
            }
        }
    }

    public class CharacterBullet : Bullet
    {
        public char Character { get; protected set; }
        public UserSprite Parent { get; protected set; }
        public double Size { get; protected set; }
        public IEnumerator<bool> Operation { get; protected set; }

        public CharacterBullet(UserSprite parent, char c, BulletPattern op)
        {
            Parent = parent;
            Operation = op(Parent, this);
            Character = c;
            Size = 20;
            HomeX = Size / 2;
            HomeY = Size / 2;
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);
                if (X <= -Size || X >= Size + 640 || Y <= -Size || Y >= Size + 480) IsDead = true;
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                DX.DrawString((int)(X - HomeX), (int)(Y - HomeY), Character.ToString(), DX.GetColor(255, 255, 255));
                yield return true;
            }
        }
    }

    public class ImageBullet : Bullet
    {
        public int Image { get; protected set; }
        public UserSprite Parent { get; protected set; }
        public IEnumerator<bool> Operation { get; protected set; }

        public ImageBullet(UserSprite pa, int i, BulletPattern op)
        {
            Parent = pa;
            Operation = op(Parent, this);
            Image = i;
        }

        public override IEnumerator<bool> Tick()
        {
            while (!IsDead)
            {
                IsDead = !(Operation.MoveNext() && Operation.Current);
                if (X <= -HomeX || X >= HomeX + 640 || Y <= -HomeY || Y >= HomeY + 480) IsDead = true;
                yield return true;
            }
        }

        public override IEnumerator<bool> Draw()
        {
            while (true)
            {
                if (DX.CheckHandleASyncLoad(Image) == DX.FALSE)
                {
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)Alpha * 255);
                    DX.DrawRotaGraph3((int)X, (int)Y, (int)HomeX, (int)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                }
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
        None = 0,
        Player = 1,
        Enemy = 2,
        PlayerBullet = 4,
        EnemyBullet = 8,
    }
}