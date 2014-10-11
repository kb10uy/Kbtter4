using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace Kbtter5
{
    public static class Gamepad
    {
        public static GamepadState GetState()
        {
            var s = DX.GetJoypadInputState(DX.DX_INPUT_KEY_PAD1);
            return new GamepadState(s);
        }
    }

    public sealed class GamepadState
    {
        public bool[] Buttons { get; private set; }
        public GamepadDirection Direction { get; private set; }


        public GamepadState(int s)
        {
            Buttons = new bool[28];
            for (int i = 0; i < 28; i++)
            {
                Buttons[i] = (s & (16 << i)) != 0;
            }
            Direction = (GamepadDirection)(s & 15);
        }
    }

    [Flags]
    public enum GamepadDirection
    {
        Down = 1,
        Left = 2,
        Right = 4,
        Up = 8,
    }
}
