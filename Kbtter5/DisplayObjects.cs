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
                DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, (int)Alpha * 255);
                DX.DrawRotaGraph3((int)X, (int)Y, (int)HomeX, (int)HomeY, ScaleX, ScaleY, Angle, Image, DX.TRUE);
                yield return true;
            }
        }
    }

    public class EnemyUser : UserSprite
    {
        private User user;
        public IEnumerator<bool> Operation { get; protected set; }
        private SceneGame game;

        public EnemyUser(SceneGame sc, User u, EnemyPattern op)
        {
            game = sc;
            user = u;
            Operation = op(this);
            MyKind = ObjectKind.Enemy;
            DamageKind = ObjectKind.PlayerBullet;
        }

        public void AddBullet(Bullet b)
        {
            game.AddBullet(b);
        }

        public override IEnumerator<bool> Tick()
        {
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(user);
                ImageLoaded = true;
            });
            while (!(IsDead = !(Operation.MoveNext() && Operation.Current))) yield return true;
        }
    }

    public class PlayerUser : UserSprite
    {

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
            while (!(IsDead = !(Operation.MoveNext() && Operation.Current))) yield return true;
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