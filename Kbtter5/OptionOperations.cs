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
            UserValueCombination = OptionSelectionValue.Direction | OptionSelectionValue.Mode | OptionSelectionValue.StringValue,
            UserValueDescription = new Dictionary<OptionSelectionValue, string> 
            {
                { OptionSelectionValue.StringValue, "あ" }
            }
        };

        public static IEnumerator<bool> HomingShotOption(PlayerOption option, OptionInitializationInformation info)
        {
            while (true) yield return true;
        }
        #endregion

        #region StringAdvertisementOption
        public static OptionSelectionInformation StringAdvertisementOption = new OptionSelectionInformation()
        {
            Name = "文字列表示",
            Description = "好きな文字列を表示します。",
            Operation = NoneOption,
            UserValueCombination = OptionSelectionValue.StringValue | OptionSelectionValue.Int32Value1,
            UserValueDescription = new Dictionary<OptionSelectionValue, string>
            {
                { OptionSelectionValue.StringValue, "表示文字列" },
                { OptionSelectionValue.Int32Value1, "Int32テスト" }
            }
        };
        #endregion

        public static IReadOnlyList<OptionSelectionInformation> SelectionInformation = new List<OptionSelectionInformation>()
        {
            NoneOptionInformation,
            LinearLaserOptionInformation,
            HomingShotOptionInformation,
            StringAdvertisementOption,
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

    public class OptionInitializationInformation
    {
        public OptionDirection Direction { get; set; }
        public int Mode { get; set; }
        public int UserInt32Value1 { get; set; }
        public int UserInt32Value2 { get; set; }
        public double UserDoubleValue1 { get; set; }
        public double UserDoubleValue2 { get; set; }
        public string UserStringValue { get; set; }
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

        public OptionSelectionInformation()
        {
            Name = "";
            Description = "";
            ActualUserValues = new OptionSelectionValue[3];
            ModeStrings = new List<string>();
            UserValueDescription = new Dictionary<OptionSelectionValue, string>();
        }
    }

    [Flags]
    public enum OptionSelectionValue
    {
        Direction = 1,
        Mode = 2,
        Int32Value1 = 4,
        Int32Value2 = 8,
        DoubleValue1 = 16,
        DoubleValue2 = 32,
        StringValue = 64,
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

}
