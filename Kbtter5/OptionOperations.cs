using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet;
using DxLibDLL;

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
            UserValueCombination = OptionSelectionValue.Direction | OptionSelectionValue.Mode
        };

        public static IEnumerator<bool> LinearLaserOption(PlayerOption option, OptionInitializationInformation info)
        {
            while (true) yield return true;
        }
        #endregion

        #region HomingShotOption
        public static OptionSelectionInformation HomingShotOptionInformation = new OptionSelectionInformation()
        {
            Name = "ホーミング",
            Description = "ホーミング弾です",
            Operation = HomingShotOption,
            ModeStrings = new[] { "ショット性能重視", "カーブ性能重視" },
            UserValueCombination = OptionSelectionValue.Direction | OptionSelectionValue.Mode,
        };

        public static IEnumerator<bool> HomingShotOption(PlayerOption option, OptionInitializationInformation info)
        {
            while (true) yield return true;
        }
        #endregion

        #region StringAdvertisementOption
        public static OptionSelectionInformation StringAdvertisementOptionInformation = new OptionSelectionInformation()
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

        public static IEnumerator<bool> StringAdvertisementOption(PlayerOption option, OptionInitializationInformation info)
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

        public static IReadOnlyList<OptionSelectionInformation> SelectionInformation = new List<OptionSelectionInformation>()
        {
            NoneOptionInformation,
            LinearLaserOptionInformation,
            HomingShotOptionInformation,
            StringAdvertisementOptionInformation,
        };

        #region ユーティリティ
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
