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
    public class EnemyBoss : EnemyUser
    {
        public int PhaseIndex { get; private set; }
        public IReadOnlyList<BossPhasePattern> Phases { get; private set; }
        public IEnumerator<bool> CurrentPhaseOperation { get; private set; }
        public int CurrentPhaseHealth { get; private set; }


        public EnemyBoss()
        {
            CollisionRadius = 40;
            GrazeRadius = 64;
            ScaleX = 4;
            ScaleY = 4;
        }

        public EnemyBoss(SceneGame sc, BossPattern op, Status s)
        {
            SourceStatus = s;
            SourceUser = s.User;
            Phases = op(this);
            PhaseIndex = -1;
            MoveNextPhase();
        }

        public override IEnumerator<bool> Tick()
        {
            while (!(IsDead = !(!IsDead && CurrentPhaseOperation.MoveNext() && CurrentPhaseOperation.Current)))
            {
                if (Player.HasCollision)
                {
                    var xd = X - Player.X;
                    var yd = Y - Player.Y;
                    var zd = CollisionRadius + Player.CollisionRadius;
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

        public override void Damage(int point)
        {
            Health -= point;
            Game.Score(point / 100 * 10);
            if (!MoveNextPhase())
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

        public bool MoveNextPhase()
        {
            if (++PhaseIndex < Phases.Count)
            {
                var ph = Phases[PhaseIndex](this);
                Health = CurrentPhaseHealth = ph.MaxHealth;
                CurrentPhaseOperation = ph.Operation;
                return true;
            }
            return false;
        }
    }
}
