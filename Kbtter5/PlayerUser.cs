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
    public class PlayerUser : UserSprite
    {
        private Xorshift128Random rnd = new Xorshift128Random();
        private SceneGame game;
        public int ShotInterval { get; set; }
        public IEnumerator<bool> Operation { get; protected set; }
        public IEnumerator<bool> SpecialOperation { get; protected set; }
        public bool Operatable { get; protected set; }
        public int ShotStrength { get; protected set; }
        private int GrazePoint;
        public bool IsGameOver { get; protected set; }
        public bool HasCollision { get; protected set; }
        public int Frames { get; protected set; }

        public PlayerUser(SceneGame sc, PlayerOperation op, User u)
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
                ParentManager.Add(new PlayerImageBullet(this, BulletPatterns.LazyHomingToEnemy(this, at, sp, 60, 10), CommonObjects.ImageStar, SourceUser.StatusesCount)
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

        public void TryShot(double angle, double speed)
        {
            if (Frames % ShotInterval != 0) return;
            ParentManager.Add(new PlayerImageBullet(this, BulletPatterns.Linear(angle, speed, 90), CommonObjects.ImageShot, ShotStrength)
            {
                X = X,
                Y = Y,
                HomeX = 8,
                HomeY = 8,
            }, PlayerBulletLayer);
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                Operation.MoveNext();
                SpecialOperation = (SpecialOperation != null && SpecialOperation.MoveNext() && SpecialOperation.Current) ? SpecialOperation : null;
                Frames++;
                X = Math.Min(Math.Max(X, 0), CommonObjects.StageWidth);
                Y = Math.Min(Math.Max(Y, 0), CommonObjects.StageHeight);
                yield return true;
            }
        }
    }

    public class PlayerImageBullet : Bullet
    {
        public PlayerUser Parent { get; protected set; }
        public int Strength { get; protected set; }

        public PlayerImageBullet(PlayerUser pa, BulletPattern op, int i, int s)
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
                if (X <= -HomeX || X >= HomeX + CommonObjects.StageWidth || Y <= -HomeY || Y >= HomeY + CommonObjects.StageHeight) IsDead = true;

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

}
