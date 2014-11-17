using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{
    public delegate IEnumerator<bool> OptionOperation(PlayerOption option, OptionInitializationInformation info);

    public static class OptionOperations
    {

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
