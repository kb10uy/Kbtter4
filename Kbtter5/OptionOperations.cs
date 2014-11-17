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
        public static IReadOnlyList<OptionSelectionInformation> SelectionInformation = new List<OptionSelectionInformation>()
        {
            LinearLaserOptionInformation
        };

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
        public IReadOnlyList<string> ModeStrings { get; set; }
        public OptionOperation Operation { get; set; }
        public OptionSelectionValue UserValueCombination { get; set; }

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
        Right,
        DownerRight,
        Down,
        DownerLeft,
        Left,
        UpperLeft,
        Up,
        UpperRight,
        Undefined,
        PlayerFollowing,
        NearestEnemy,
        Random,
    }

}
