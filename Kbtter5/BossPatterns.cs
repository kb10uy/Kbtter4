using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{
    public delegate IReadOnlyList<BossPhasePattern> BossPattern(EnemyBoss boss);

    public delegate BossPhase BossPhasePattern(EnemyBoss boss);

    public class BossPhase
    {
        public IEnumerator<bool> Operation { get; private set; }
        public int MaxHealth { get; private set; }

        private BossPhase()
        {

        }

        public static BossPhase Create(EnemyBoss boss, IEnumerator<bool> op, int health)
        {
            return new BossPhase { Operation = op, MaxHealth = health };
        }
    }

    public static class BossPatterns
    {
        static Xorshift128Random rnd = new Xorshift128Random();
    }

    public static class BossPhases
    {

        public static BossPhasePattern SineWave(int hp, double speed)
        {
            return (boss) => BossPhase.Create(boss, SineWave(boss, speed), hp);
        }

        private static IEnumerator<bool> SineWave(EnemyBoss boss, double speed)
        {
            while (boss.Health >= 0)
            {
                yield return true;
            }
        }
    }

}
