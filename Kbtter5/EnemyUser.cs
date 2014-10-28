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
    public class EnemyUser : UserSprite
    {
        public Status SourceStatus { get; protected set; }
        public IEnumerator<bool> Operation { get; protected set; }
        protected SceneGame Game { get; set; }
        public EnemyUser ParentEnemy { get; protected set; }
        public bool DieWithParentDeath { get; set; }
        public int Health { get; protected set; }
        public int TotalHealth { get; protected set; }
        protected static Xorshift128Random rnd = new Xorshift128Random();

        public EnemyUser()
        {
            MyKind = ObjectKind.Enemy;
            TargetKind = ObjectKind.Player;
            DamageKind = ObjectKind.PlayerBullet;
            CollisonRadius = 10;
            GrazeRadius = 14;
            DieWithParentDeath = false;
        }

        public EnemyUser(SceneGame sc, EnemyPattern op, Status s)
            : this()
        {
            Game = sc;
            SourceStatus = s;
            SourceUser = s.User;
            Operation = op(this);
            TotalHealth = Health = 1000 + (SourceStatus.User.StatusesCount / 10) + (DateTime.Now - SourceStatus.User.CreatedAt.LocalDateTime).Days * 3;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceStatus.User);
                IsImageLoaded = true;
            });
        }

        public EnemyUser(EnemyUser sc, EnemyPattern op, Status s)
            : this()
        {
            ParentEnemy = sc;
            Game = sc.Game;
            SourceStatus = s;
            SourceUser = s.User;
            Operation = op(this);
            TotalHealth = Health = 1000 + (SourceStatus.User.StatusesCount / 10) + (DateTime.Now - SourceStatus.User.CreatedAt.LocalDateTime).Days * 3;
            Task.Run(() =>
            {
                Image = UserImageManager.GetUserImage(SourceStatus.User);
                IsImageLoaded = true;
            });
        }

        public PlayerUser Player { get { return Game.Player; } }

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

        public virtual void Damage(int point)
        {
            Health -= point;
            Game.Score(point / 100 * 10);
            ParentManager.Add(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, point / 100 * 10) { X = X, Y = Y }, EffectLayer);
            if (Health <= 0)
            {
                ParentManager.Add(new ScoreSprite(CommonObjects.ImageNumber12Red, 6, 12, TotalHealth / 10 * 10) { X = X, Y = Y }, EffectLayer);
                Game.Score(TotalHealth / 10 * 10);
                Game.DestroyEnemy();
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
}
