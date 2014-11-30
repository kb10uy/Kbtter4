using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet;
using DxLibDLL;
using EasingSharp;

namespace Kbtter5
{
    public delegate IEnumerator<bool> OptionOperation(PlayerOption option, OptionInitializationInformation info);

    public static class OptionOperations
    {
        #region None
        public static OptionSelectionInformation NoneOptionInformation = new OptionSelectionInformation()
        {
            Name = "なし",
            Description = "いるだけで何もしません",
            Operation = NoneOption,
        };

        public static IEnumerator<bool> NoneOption(PlayerOption option, OptionInitializationInformation info)
        {
            while (true) yield return true;
        }
        #endregion

        #region LinearLaserOption
        public static OptionSelectionInformation LinearLaserOptionInformation = new OptionSelectionInformation()
        {
            Name = "直線レーザー",
            Description = "普通のレーザーです",
            Operation = LinearLaserOption,
            ModeStrings = new[] { "Kbtter5", "魔理沙" },
            UserValueCombination = OptionSelectionValue.Direction | OptionSelectionValue.Mode,
            DefaultValue = new OptionInitializationInformation()
            {
                Direction = OptionDirection.Up,
                Mode = 1,
            }
        };

        private static IEnumerator<bool> LinearLaserOption(PlayerOption option, OptionInitializationInformation info)
        {
            var s = false;
            var ps = false;
            var la = false;
            PlayerLinearLaser ls = null;

            while (true)
            {
                s = option.Parent.IsShottableTiming;
                if (!ps && s)
                {
                    la = true;
                    ls = new PlayerLinearLaser(option, LinearLaserMarisaStyle, 8, CommonObjects.ImageLaser8)
                    {
                        Angle = GetAngle(info.Direction) ?? 0,
                        X = option.X,
                        Y = option.Y
                    };
                    option.ParentManager.Add(ls, (int)GameLayer.PlayerBullet);
                }
                if (ps && !s)
                {
                    la = false;
                    ls.AddSubOperation(LaserThrowAway(ls.Angle, 20));
                }
                if (la)
                {
                    ls.X = option.X;
                    ls.Y = option.Y;
                }
                ps = s;
                yield return true;
            }
        }

        private static IEnumerator<bool> LinearLaserMarisaStyle(UserSprite par, PlayerLinearLaser laser)
        {
            laser.BrightR = (byte)((long)par.SourceUser.Id & 0xFF);
            laser.BrightG = (byte)((long)(par.SourceUser.Id >> 8) & 0xFF);
            laser.BrightB = (byte)((long)(par.SourceUser.Id >> 16) & 0xFF);
            while (true)
            {
                if (laser.Length < 800) laser.Length += 20;
                yield return true;
            }
        }

        private static CoroutineFunction<MultiAdditionalCoroutineSprite> LaserThrowAway(double angle, double speed)
        {
            return sp => LaserThrowAwayFunction(sp, angle, speed);
        }

        private static IEnumerator<bool> LaserThrowAwayFunction(MultiAdditionalCoroutineSprite sp, double angle, double speed)
        {
            while (true)
            {
                sp.X += Math.Cos(angle) * speed;
                sp.Y += Math.Sin(angle) * speed;
                yield return true;
            }
        }

        #endregion

        #region FollwingAttackOption
        private static OptionSelectionInformation FollwingAttackOptionInformation = new OptionSelectionInformation()
        {
            Name = "サーチアンドデストロイ",
            Description = "近くの敵に特攻を仕掛けまくります",
            ModeStrings = new[] { "広く弱く", "狭く強く", "なみなみ" },
            Operation = FollwingAttackOption,
            UserValueCombination = OptionSelectionValue.Mode,
            DefaultValue = new OptionInitializationInformation()
            {
                Mode = 2
            }
        };

        private static IEnumerator<bool> FollwingAttackOption(PlayerOption option, OptionInitializationInformation info)
        {
            bool tg = false;
            EnemyUser target = null;
            var tl = option.ParentManager.OfType<EnemyUser>()
                .Where(p =>
                {
                    var x = p.X - option.X;
                    var y = p.Y - option.Y;
                    return Math.Sqrt(x * x + y * y) <= 200;
                });
            while (true)
            {
                target = tl.FirstOrDefault();
                if (target == null && tg)
                {
                    tg = false;
                    var sx = option.X;
                    var sy = option.Y;
                    for (int i = 0; i < 30; i++)
                    {
                        option.PreventParentOperation();
                        option.X = Easing.OutQuad(i, 30, sx, option.Parent.X - sx);
                        option.Y = Easing.OutQuad(i, 30, sy, option.Parent.Y - sy);
                        yield return true;
                    }
                }
                else if (!tg && target != null)
                {
                    tg = true;
                    while (target != null)
                    {
                        var sx = option.X;
                        var sy = option.Y;
                        //同じ敵から同時に帰ってくると見栄えしないのでランダム
                        var rw = rnd.Next(10);
                        for (int i = 0; i < rw; i++)
                        {
                            option.PreventParentOperation();
                            yield return true;
                        }
                        for (int i = 0; i < 30; i++)
                        {
                            option.PreventParentOperation();
                            option.X = Easing.OutQuad(i, 30, sx, target.X - sx);
                            option.Y = Easing.OutQuad(i, 30, sy, target.Y - sy);
                            yield return true;
                        }
                        while (!target.IsDead)
                        {
                            option.PreventParentOperation();
                            option.X = target.X;
                            option.Y = target.Y;
                            target.Damage(100);
                            yield return true;
                        }
                        tl = option.ParentManager.OfType<EnemyUser>()
                        .Where(p =>
                        {
                            var x = p.X - option.X;
                            var y = p.Y - option.Y;
                            return Math.Sqrt(x * x + y * y) <= 200;
                        });
                        target = tl.FirstOrDefault();
                    }
                }
                else
                {
                    yield return true;
                }
            }
        }

        #endregion

        #region HomingShotOption
        private static OptionSelectionInformation HomingShotOptionInformation = new OptionSelectionInformation()
        {
            Name = "ホーミング",
            Description = "ホーミング弾です",
            Operation = HomingShotOption,
            ModeStrings = new[] { "ショット性能重視", "カーブ性能重視", "バランス重視" },
            UserValueCombination = OptionSelectionValue.Direction | OptionSelectionValue.Mode,
            DefaultValue = new OptionInitializationInformation()
            {
                Mode = 2,
                Direction = OptionDirection.Up
            }
        };

        private static IEnumerator<bool> HomingShotOption(PlayerOption option, OptionInitializationInformation info)
        {
            while (true)
            {
                if (option.Parent.IsTriggerShottableTiming)
                {
                    option.ParentManager.Add(
                        new PlayerImageBullet(option, Homing(), CommonObjects.ImageShotArrow, option.Information.UserInformation.ShotStrength / 2)
                        {
                            X = option.X,
                            Y = option.Y,
                            Angle = GetAngle(info.Direction) ?? -Math.PI / 2.0,
                            HomeX = 6,
                            HomeY = 12,
                            CollisionRadius = 6
                        },
                        (int)GameLayer.PlayerBullet);
                }
                yield return true;
            }
        }

        private static CoroutineFunction<UserSprite, PlayerBullet> Homing()
        {
            return (sp, b) => HomingFunction(sp, b);
        }

        private static IEnumerator<bool> HomingFunction(UserSprite us, PlayerBullet b)
        {
            var speed = 12.0;
            var curve = 0.03;
            var tl = us.ParentManager.OfType<EnemyUser>()
                .OrderBy(p =>
                {
                    var x = p.X - b.X;
                    var y = p.Y - b.Y;
                    return Math.Sqrt(x * x + y * y);
                }).FirstOrDefault();
            var hm = tl != null;
            while (true)
            {
                if (hm && !tl.IsDead)
                {
                    var ta = Math.Atan2(tl.Y - b.Y, tl.X - b.X) - b.Angle;
                    if (ta <= 0)
                    {
                        b.Angle -= curve;
                    }
                    else
                    {
                        b.Angle += curve;
                    }
                }
                b.X += Math.Cos(b.Angle) * speed;
                b.Y += Math.Sin(b.Angle) * speed;
                yield return true;
            }
        }
        #endregion

        #region StringAdvertisementOption
        private static OptionSelectionInformation StringAdvertisementOptionInformation = new OptionSelectionInformation()
        {
            Name = "文字列表示",
            Description = "好きな文字列を表示します。",
            Operation = StringAdvertisementOption,
            UserValueCombination = OptionSelectionValue.StringValue1 | OptionSelectionValue.StringValue2,
            UserValueDescription = new Dictionary<OptionSelectionValue, string>
            {
                { OptionSelectionValue.StringValue1, "表示文字列" },
                { OptionSelectionValue.StringValue2, "色(R,G,Bの形で10進)" },
            },
            DefaultValue = new OptionInitializationInformation()
            {
                UserStringValue2 = "255,255,255",
                UserStringValue2Validation = (p) =>
                {
                    try
                    {
                        var t = p.Split(',').Select(q => Convert.ToInt32(q)).ToArray();
                        return t.Length == 3;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        };

        private static IEnumerator<bool> StringAdvertisementOption(PlayerOption option, OptionInitializationInformation info)
        {
            var cv = info.UserStringValue2.Split(',').Select(p => Convert.ToInt32(p)).ToArray();

            var ss = new StringSprite(CommonObjects.FontSystemMedium, DX.GetColor(cv[0], cv[1], cv[2])) { Value = info.UserStringValue1 };
            option.ParentManager.Add(ss, (int)GameLayer.Player);

            while (true)
            {
                ss.X = option.X;
                ss.Y = option.Y;
                yield return true;
            }
        }
        #endregion

        #region GradiusLaserOption
        public static OptionSelectionInformation GradiusLaserOptionInformation = new OptionSelectionInformation()
        {
            Name = "グラディウス風レーザー",
            Description = "グラディウスっぽいやつです",
            Operation = GradiusLaserOption,
            UserValueCombination = OptionSelectionValue.Direction | OptionSelectionValue.DoubleValue1,
            UserValueDescription = new Dictionary<OptionSelectionValue, string>
            {
                { OptionSelectionValue.DoubleValue1, "移動感知最小値" }
            },
            DefaultValue = new OptionInitializationInformation()
            {
                Direction = OptionDirection.Up,
                UserDoubleValue1 = 1.0,
                UserDoubleValue1Validation = (p) => p >= 0.1
            }
        };

        private static IEnumerator<bool> GradiusLaserOption(PlayerOption option, OptionInitializationInformation info)
        {
            var sbc = 0;
            var sb = new Point[30];

            var sp = Point.FromDisplayObject(option.Parent);
            var threshold = info.UserDoubleValue1;
            var pp = sp;
            for (int i = 0; i < sb.Length; i++) sb[i] = sp;

            while (true)
            {
                option.PreventParentOperation();
                var np = Point.FromDisplayObject(option.Parent);
                if (pp.DistanceTo(np) >= threshold)
                {
                    sb[(sbc++) % sb.Length] = np;
                    option.ApplyPosition(sb[(sbc + sb.Length + 1) % sb.Length]);
                }
                pp = np;
                yield return true;
            }
        }
        #endregion

        public static IReadOnlyList<OptionSelectionInformation> SelectionInformation = new List<OptionSelectionInformation>()
        {
            NoneOptionInformation,
            HomingShotOptionInformation,
            LinearLaserOptionInformation,
            GradiusLaserOptionInformation,
            FollwingAttackOptionInformation,
            StringAdvertisementOptionInformation,
        };

        #region ユーティリティ
        public static void ApplyPosition(this PlayerOption opt, Point p)
        {
            opt.X = p.X;
            opt.Y = p.Y;
        }

        public static double DistanceTo(this Point bp, Point tp)
        {
            return Math.Sqrt((tp.X - bp.X) * (tp.X - bp.X) + (tp.Y - bp.Y) * (tp.Y - bp.Y));
        }

        public static double DistanceToWithOutSqrt(this Point bp, Point tp)
        {
            return (tp.X - bp.X) * (tp.X - bp.X) + (tp.Y - bp.Y) * (tp.Y - bp.Y);
        }

        private static Xorshift128Random rnd = new Xorshift128Random();

        public static IReadOnlyList<OptionSelectionValue> GetDecomposedValues(this OptionSelectionValue v)
        {
            var ret = new List<OptionSelectionValue>();
            var em = Enum.GetValues(typeof(OptionSelectionValue)).Cast<OptionSelectionValue>();
            foreach (var i in em) if ((v & i) != 0) ret.Add(i);
            return ret;
        }

        public static IReadOnlyList<string> OptionDirectionDescriptions = new List<string>
        {
            "未定義",
            "右",
            "右下",
            "下",
            "左下",
            "左",
            "左上",
            "上",
            "右上",
            "プレイヤーに従う",
            "最も近い敵",
            "ランダム"
        };

        public static double? GetAngle(OptionDirection d)
        {
            switch ((int)d)
            {
                case 0:
                case 9:
                case 10:
                    return null;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                    return Math.PI * 2.0 / 8.0 * ((int)d - 1);
                case 11:
                    return rnd.NextDouble() * Math.PI * 2;
                default:
                    return null;
            }
        }

        #endregion
    }

    #region 関係各クラス
    public class OptionInitializationInformation
    {
        public OptionDirection Direction { get; set; }
        public int Mode { get; set; }
        public int UserInt32Value1 { get; set; }
        public int UserInt32Value2 { get; set; }
        public int UserInt32Value3 { get; set; }
        public double UserDoubleValue1 { get; set; }
        public double UserDoubleValue2 { get; set; }
        public double UserDoubleValue3 { get; set; }
        public string UserStringValue1 { get; set; }
        public string UserStringValue2 { get; set; }
        public string UserStringValue3 { get; set; }
        public Predicate<int> UserInt32Value1Validation { get; set; }
        public Predicate<int> UserInt32Value2Validation { get; set; }
        public Predicate<int> UserInt32Value3Validation { get; set; }
        public Predicate<double> UserDoubleValue1Validation { get; set; }
        public Predicate<double> UserDoubleValue2Validation { get; set; }
        public Predicate<double> UserDoubleValue3Validation { get; set; }
        public Predicate<string> UserStringValue1Validation { get; set; }
        public Predicate<string> UserStringValue2Validation { get; set; }
        public Predicate<string> UserStringValue3Validation { get; set; }

        public OptionInitializationInformation()
        {
            UserStringValue1 = "";
            UserStringValue2 = "";
            UserStringValue3 = "";
        }
    }


    public class OptionInformation
    {
        public User SourceUser { get; set; }
        public UserInformation UserInformation { get; set; }
        public OptionOperation TargetOperation { get; set; }
        public OptionInitializationInformation InitializationInformation { get; set; }

        public OptionInformation(User u)
        {
            SourceUser = u;
            UserInformation = new UserInformation(u);
        }
    }

    public class OptionSelectionInformation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<OptionSelectionValue> ActualUserValues { get; private set; }
        public IReadOnlyList<string> ModeStrings { get; set; }
        public OptionOperation Operation { get; set; }
        public OptionSelectionValue UserValueCombination { get; set; }
        public IReadOnlyDictionary<OptionSelectionValue, string> UserValueDescription { get; set; }
        public OptionInitializationInformation DefaultValue { get; set; }

        public OptionSelectionInformation()
        {
            Name = "";
            Description = "";
            ActualUserValues = new OptionSelectionValue[3];
            ModeStrings = new List<string>();
            UserValueDescription = new Dictionary<OptionSelectionValue, string>();
            DefaultValue = new OptionInitializationInformation();
        }
    }

    [Flags]
    public enum OptionSelectionValue
    {
        Direction = 1,
        Mode = 2,
        Int32Value1 = 4,
        Int32Value2 = 8,
        Int32Value3 = 16,
        DoubleValue1 = 32,
        DoubleValue2 = 64,
        DoubleValue3 = 128,
        StringValue1 = 256,
        StringValue2 = 512,
        StringValue3 = 1024,
    }

    public enum OptionDirection
    {
        Undefined = 0,
        Right,
        DownerRight,
        Down,
        DownerLeft,
        Left,
        UpperLeft,
        Up,
        UpperRight,
        PlayerFollowing,
        NearestEnemy,
        Random,
    }
    #endregion
}
