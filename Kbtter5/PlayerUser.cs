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
        public int Frames { get; protected set; }
        public PlayerInput CurrentInput { get; protected set; }
        public bool Operatable { get; protected set; }
        public IEnumerator<bool> MovingOperation { get; protected set; }
        public IEnumerator<bool> ShotOperation { get; protected set; }
        public IEnumerator<bool> SpecialOperation { get; protected set; }
        public bool HasCollision { get; protected set; }
        public int ShotStrength { get; protected set; }
        public int ShotInterval { get; protected set; }
        public bool IsShottableTiming { get; protected set; }
        public bool IsTriggerShottableTiming { get; protected set; }
        public bool IsGameOver { get; protected set; }
        private Xorshift128Random rnd = new Xorshift128Random();
        private SceneGame game;
        private int GrazePoint;
        private UserInformation info;
        private PlayerInputMethod ipmet;

        public PlayerUser(SceneGame sc, UserInformation u, CoroutineFunction<PlayerUser> mop, CoroutineFunction<PlayerUser> sop, PlayerInputMethod im)
        {
            info = u;
            game = sc;
            ipmet = im;
            SourceUser = u.SourceUser;
            MovingOperation = mop(this);
            ShotOperation = sop(this);
            MyKind = ObjectKind.Player;
            DamageKind = ObjectKind.Enemy | ObjectKind.EnemyBullet;
            CollisionRadius = u.CollisionRadius;
            GrazeRadius = CollisionRadius * 1.5;
            ShotStrength = u.ShotStrength;
            ShotInterval = 2;
            Operatable = true;
            HasCollision = true;
            GrazePoint = u.GrazePoints;
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
                ParentManager.Add(new PlayerImageBullet(this, BulletPatterns.LazyHomingToEnemy(this, at, sp, 60, 10), CommonObjects.ImageStar, info.BombStrength)
                {
                    ScaleX = 8.0,
                    ScaleY = 8.0,
                    CollisionRadius = 56,
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

        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                CurrentInput = ipmet();
                IsShottableTiming = (CurrentInput.Button & PlayerInputButton.Shot) != 0;
                IsTriggerShottableTiming = Frames % ShotInterval == 0 && IsShottableTiming;

                MovingOperation.MoveNext();
                X = Math.Min(Math.Max(X, 0), CommonObjects.StageWidth);
                Y = Math.Min(Math.Max(Y, 0), CommonObjects.StageHeight);
                ShotOperation.MoveNext();
                if ((CurrentInput.Button & PlayerInputButton.Bomb) != 0) TryBomb();
                SpecialOperation = (SpecialOperation != null && SpecialOperation.MoveNext() && SpecialOperation.Current) ? SpecialOperation : null;

                Frames++;
                yield return true;
            }
        }
    }

    

    public class UserInformation
    {
        public User SourceUser { get; private set; }
        public int ShotStrength { get; private set; }
        public int BombStrength { get; private set; }
        public int DefaultPlayers { get; private set; }
        public int DefaultBombs { get; private set; }
        public int GrazePoints { get; private set; }
        public double CollisionRadius { get; private set; }

        public UserInformation(User user)
        {
            SourceUser = user;
            ShotStrength = (SourceUser.StatusesCount + (DateTime.Now - SourceUser.CreatedAt.LocalDateTime).Days * (int)Math.Log10(SourceUser.StatusesCount)) / 25;
            GrazePoints = (SourceUser.StatusesCount / SourceUser.FollowersCount) / 20 + 10;
            CollisionRadius = 4.0 * SourceUser.FriendsCount / SourceUser.FollowersCount;
            DefaultPlayers = (int)(Math.Log10(SourceUser.FollowersCount) * Math.Log10(SourceUser.FriendsCount)) * 4;
            DefaultBombs = (int)(Math.Log10(SourceUser.FavouritesCount) + Math.Log10(SourceUser.StatusesCount)) * 2;
            BombStrength = SourceUser.StatusesCount;
        }
    }

    public delegate PlayerInput PlayerInputMethod();

    public struct PlayerInput
    {
        public PlayerInputDirection Direction;
        public PlayerInputDirection ExtraDirection;
        public PlayerInputButton Button;
        public short MouseX;
        public short MouseY;
    }

    [Flags]
    public enum PlayerInputButton
    {
        Shot = 1,
        Bomb = 2,
        Pause = 4,
    }

    [Flags]
    public enum PlayerInputDirection
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
    }
}
