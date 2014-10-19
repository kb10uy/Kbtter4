using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasingSharp;

namespace Kbtter5
{
    public delegate IEnumerator<bool> EnemyPattern(EnemyUser sp);

    public static class EnemyPatterns
    {
        static int EnemyLayer = (int)GameLayer.Enemy;
        static int EnemyBulletLayer = (int)GameLayer.EnemyBullet;
        static Xorshift128Random rnd = new Xorshift128Random();

        public static Dictionary<EnemyPattern, int> Patterns = new Dictionary<EnemyPattern, int>
        {
            { GoDownAndAway, 120 },
            { ToorimasuyoUpper, 2 },
            { ToorimasuyoUpperReverse, 2 },
            { ToorimasuyoLower, 2 },
            { ToorimasuyoLowerReverse, 2 },
            { ToorimasuyoLefter, 2 },
            { ToorimasuyoLefterReverse, 2 },
            { ToorimasuyoRighter, 2},
            { ToorimasuyoRighterReverse, 2 },
        };

        public static EnemyPattern GetRandomPattern()
        {
            var all = Patterns.Values.Sum();
            var sel = rnd.Next(all);
            foreach (var i in Patterns)
            {
                if (sel < i.Value)
                {
                    return i.Key;
                }
                sel -= i.Value;
            }
            return Patterns.First().Key;
        }

        public static IEnumerator<bool> GoDownAndAway(EnemyUser sp)
        {
            var sx = rnd.Next(600) + 20;
            var sy = -32;
            sp.X = sx;
            sp.Y = sy;

            var val = 100 + (sp.SourceStatus.Text.Length + sp.SourceStatus.User.FavouritesCount) % 100;
            for (int i = 0; i < 60; i++)
            {
                sp.Y = Easing.OutSine(i, 60, sy, val);
                yield return true;
            }

            string str = sp.SourceStatus.Text;
            var us = sp.SourceStatus.User;
            var ta = Math.Atan2(sp.Player.Y - sp.Y, sp.Player.X - sp.X);

            switch ((us.Id - sp.SourceStatus.Text.Length * us.StatusesCount) % 11)
            {
                case 0:
                    for (int i = 0; i < str.Length; i++)
                    {
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI * 2 / str.Length * i, 5, 240), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < str.Length; i++)
                    {
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LinearCurve(Math.PI * 2 / str.Length * i, -0.02 + (us.FollowersCount % 100) / 2500.0, 5, 240), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                    }
                    break;
                case 2:
                    for (int i = 0; i < str.Length; i++)
                    {
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI * 2 / str.Length * i, 5, 240), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        for (int j = 0; j < 420 / str.Length; j++) yield return true;
                    }
                    break;
                case 3:
                    for (int i = 0; i < str.Length; i++)
                    {
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LinearCurve(Math.PI * 2 / str.Length * i, -0.02 + (us.FollowersCount % 100) / 2500.0, 5, 240), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        for (int j = 0; j < 420 / str.Length; j++) yield return true;
                    }
                    break;
                case 4:
                    sp.ParentManager.Add(new LinearLaser(sp, LinearLaserPatterns.Normal(200, 5), 16, CommonObjects.ImageLaser16)
                    {
                        X = sp.X,
                        Y = sp.Y,
                        Angle = ta
                    }, EnemyBulletLayer);
                    break;
                case 5:
                    sp.ParentManager.Add(new LinearLaser(sp, LinearLaserPatterns.Worm(200, 5), 16, CommonObjects.ImageLaser16)
                    {
                        X = sp.X,
                        Y = sp.Y,
                        Angle = Math.Atan2(sp.Player.Y - sp.Y, sp.Player.X - sp.X)
                    }, EnemyBulletLayer);
                    break;
                case 6:
                    var cd = rnd.Next(100);
                    sp.ParentManager.Add(
                        new CurveLaser(sp,
                            new BezierCurve(
                                64,
                                new Point { X = sp.X, Y = sp.Y },
                                new Point
                                {
                                    X = (sp.Player.X - sp.X) + Math.Cos(ta + Math.PI / 2.0) * cd,
                                    Y = (sp.Player.Y - sp.Y) + Math.Sin(ta + Math.PI / 2.0) * cd
                                },
                                new Point { X = sp.Player.X, Y = sp.Player.Y }),
                            CurveLaserPatterns.Normal(1),
                            16,
                            CommonObjects.ImageBezierLaser),
                        EnemyBulletLayer);
                    break;
                case 7:
                    sp.ParentManager.Add(
                        new CurveLaser(
                            sp,
                            new TargettingDummyCurve(
                                new Point { X = sp.X, Y = sp.Y },
                                new Point { X = sp.Player.X, Y = sp.Player.Y },
                                6,
                                32),
                            CurveLaserPatterns.Homing(sp.Player, 240, 6, 0.08),
                            16,
                            CommonObjects.ImageBezierLaser),
                        EnemyBulletLayer);
                    break;
                case 8:
                    for (int i = 0; i < str.Length / 2; i++)
                    {
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(ta - 0.15, 5, 240), str[i * 2]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(ta + 0.15, 5, 240), str[i * 2 + 1]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        for (int j = 0; j < 6; j++) yield return true;
                    }
                    break;
                case 9:
                    for (int i = 0; i < str.Length / 3; i++)
                    {
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(ta - 0.15, 5, 240), str[i * 3 + 1]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(ta, 5, 240), str[i * 3]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(ta + 0.15, 5, 240), str[i * 3 + 2]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        for (int j = 0; j < 6; j++) yield return true;
                    }
                    break;
                case 10:

                    for (int i = 0; i < str.Length; i++)
                    {
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(Math.PI * 2 / str.Length * i, 3, 60, sp.Player, 6), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                    }
                    break;

            }


            for (int i = 0; i < 60; i++) yield return true;

            for (int i = 0; i < 120; i++)
            {
                sp.Y = Easing.OutSine(i, 120, sy + val, -val);
                yield return true;
            }
        }

        public static IEnumerator<bool> GoDownThrough(EnemyUser sp)
        {
            sp.X = rnd.Next(600) + 20;
            sp.Y = -32;

            var speed = unchecked((sp.SourceStatus.User.FriendsCount * sp.SourceStatus.User.StatusesCount % 100) / 11.0);
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            for (int i = 0; i < 600 / speed; i++)
            {
                sp.Y += speed;
                switch ((us.Id - sp.SourceStatus.Text.Length * us.StatusesCount) % 4)
                {
                    case 0:
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI * 2 / str.Length * i, 5, 240), str[i % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        break;
                    case 1:
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LinearCurve(Math.PI * 2 / str.Length * i, -0.02 + (us.FollowersCount % 100) / 2500.0, 5, 240), str[i % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        break;
                    case 2:
                    case 3:
                        sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(Math.PI * 2 / str.Length * i, 3, 60, sp.Player, 6), str[i % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoUpper(EnemyUser sp)
        {
            sp.X = -32;
            sp.Y = 80 + rnd.Next(40);
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(2);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.X <= 672)
            {
                sp.X += speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI / 2, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoLower(EnemyUser sp)
        {
            sp.X = -32;
            sp.Y = 360 + rnd.Next(40);
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(2);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.X <= 672)
            {
                sp.X += speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI * 1.5, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoUpperReverse(EnemyUser sp)
        {
            sp.X = 672;
            sp.Y = 80 + rnd.Next(40);
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(2);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.X >= -32)
            {
                sp.X -= speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI / 2, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoLowerReverse(EnemyUser sp)
        {
            sp.X = 672;
            sp.Y = 360 + rnd.Next(40);
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(2);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.X >= -32)
            {
                sp.X -= speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI * 1.5, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoLefter(EnemyUser sp)
        {
            sp.X = 80 + rnd.Next(40);
            sp.Y = -32;
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(4);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.Y <= 512)
            {
                sp.Y += speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(0, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                    case 2:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(Math.PI * 2 / str.Length * i, 3, 60, sp.Player, 6), str[i % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 3:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(rnd.NextDouble() * Math.PI * 2, 3, 60, sp.Player, 6), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoLefterReverse(EnemyUser sp)
        {
            sp.X = 80 + rnd.Next(40);
            sp.Y = 512;
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(4);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.Y >= -32)
            {
                sp.Y -= speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(0, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                    case 2:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(Math.PI * 2 / str.Length * i, 3, 60, sp.Player, 6), str[i % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 3:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(rnd.NextDouble() * Math.PI * 2, 3, 60, sp.Player, 6), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoRighter(EnemyUser sp)
        {
            sp.X = 520 + rnd.Next(40);
            sp.Y = -32;
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(4);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.Y <= 512)
            {
                sp.Y += speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                    case 2:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(Math.PI * 2 / str.Length * i, 3, 60, sp.Player, 6), str[i % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 3:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(rnd.NextDouble() * Math.PI * 2, 3, 60, sp.Player, 6), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> ToorimasuyoRighterReverse(EnemyUser sp)
        {
            sp.X = 520 + rnd.Next(40);
            sp.Y = 512;
            var speed = rnd.NextDouble() * 5.0 + 3.0;
            var type = rnd.Next(4);
            var iv = rnd.Next(10) + 10;
            var iv2 = rnd.Next(8) + 6;
            var cnt = 0;
            var us = sp.SourceStatus.User;
            string str = sp.SourceStatus.Text;
            while (sp.Y >= -32)
            {
                sp.Y -= speed;
                switch (type)
                {
                    case 0:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(Math.PI, (i / 3.0) + 2, 600), str[i]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 1:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 4, 600), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                    case 2:
                        if ((++cnt % iv) == 0)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(Math.PI * 2 / str.Length * i, 3, 60, sp.Player, 6), str[i % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                            }
                        }
                        break;
                    case 3:
                        if ((cnt++ % iv2) == 0)
                        {
                            sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.LazyHoming(rnd.NextDouble() * Math.PI * 2, 3, 60, sp.Player, 6), str[cnt / iv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                        }
                        break;
                }
                yield return true;
            }
        }

        public static IEnumerator<bool> RetweeterMultiCannon(EnemyUser sp)
        {
            sp.X = rnd.Next(600) + 20;
            sp.Y = -40;
            var dw = rnd.Next(100) + 100;

            for (int i = 0; i < 120; i++)
            {
                sp.Y = Easing.OutElastic(i, 120, -40, dw);
                yield return true;
            }

            var rt = sp.SourceStatus.RetweetedStatus;
            var cc = (int)(Math.Log10(rt.RetweetCount ?? 0) + 1);
            var ofs = rnd.NextDouble() * Math.PI * 2;
            for (int i = 0; i < cc; i++)
            {
                sp.ParentManager.Add(new EnemyUser(sp, RetweeterCannonCircle(ofs + Math.PI * 2 / cc * i, 5, 0.03, 64), rt) { X = sp.X, Y = sp.Y, DieWithParentDeath = true }, EnemyLayer);
            }
            while (true) yield return true;
        }

        private static EnemyPattern RetweeterCannonCircle(double startAngle, double startSpeed, double curve, double dist)
        {
            return (sp) => RetweeterCannonCircle(sp, startAngle, startSpeed, curve, dist);
        }

        private static IEnumerator<bool> RetweeterCannonCircle(EnemyUser sp, double startAngle, double startSpeed, double curve, double dist)
        {

            var ang = startAngle;
            var brake = -(startSpeed * startSpeed) / (dist * 2);
            while (startSpeed > 0)
            {
                sp.X += Math.Cos(startAngle) * startSpeed;
                sp.Y += Math.Sin(startAngle) * startSpeed;
                startSpeed += brake;
                yield return true;
            }
            var cnt = 0;
            var intv = 6;
            var str = sp.SourceStatus.Text;
            while (true)
            {
                sp.X = sp.ParentEnemy.X + Math.Cos(ang += curve) * dist;
                sp.Y = sp.ParentEnemy.Y + Math.Sin(ang) * dist;
                if ((cnt++ % intv) == 0)
                {
                    sp.ParentManager.Add(new CharacterBullet(sp, BulletPatterns.Linear(rnd.NextDouble() * Math.PI * 2, 7, 240), str[cnt / intv % str.Length]) { X = sp.X, Y = sp.Y }, EnemyBulletLayer);
                }
                yield return true;
            }
        }


    }


}
