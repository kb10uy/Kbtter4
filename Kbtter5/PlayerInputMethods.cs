using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using DxLibDLL;

namespace Kbtter5
{
    public static class PlayerInputMethods
    {
        public static PlayerInput DefaultStyle()
        {
            var ret = new PlayerInput();
            int x, y;
            DX.GetMousePoint(out x, out y);
            ret.MouseX = (short)x;
            ret.MouseY = (short)y;

            var msi = DX.GetMouseInput();
            if ((msi & DX.MOUSE_INPUT_LEFT) != 0) ret.Button |= PlayerInputButton.Shot;
            if ((msi & DX.MOUSE_INPUT_RIGHT) != 0) ret.Button |= PlayerInputButton.Bomb;

            var pad = Gamepad.GetState();
            if ((pad.Direction & GamepadDirection.Right) != 0) ret.Direction |= PlayerInputDirection.Right;
            if ((pad.Direction & GamepadDirection.Left) != 0) ret.Direction |= PlayerInputDirection.Left;
            if ((pad.Direction & GamepadDirection.Up) != 0) ret.Direction |= PlayerInputDirection.Up;
            if ((pad.Direction & GamepadDirection.Down) != 0) ret.Direction |= PlayerInputDirection.Down;

            if (DX.CheckHitKey(DX.KEY_INPUT_D) == 1) ret.ExtraDirection |= PlayerInputDirection.Right;
            if (DX.CheckHitKey(DX.KEY_INPUT_C) == 1) ret.ExtraDirection |= PlayerInputDirection.Right | PlayerInputDirection.Down;
            if (DX.CheckHitKey(DX.KEY_INPUT_X) == 1) ret.ExtraDirection |= PlayerInputDirection.Down;
            if (DX.CheckHitKey(DX.KEY_INPUT_Z) == 1) ret.ExtraDirection |= PlayerInputDirection.Down | PlayerInputDirection.Left;
            if (DX.CheckHitKey(DX.KEY_INPUT_A) == 1) ret.ExtraDirection |= PlayerInputDirection.Left;
            if (DX.CheckHitKey(DX.KEY_INPUT_Q) == 1) ret.ExtraDirection |= PlayerInputDirection.Left | PlayerInputDirection.Up;
            if (DX.CheckHitKey(DX.KEY_INPUT_W) == 1) ret.ExtraDirection |= PlayerInputDirection.Up;
            if (DX.CheckHitKey(DX.KEY_INPUT_E) == 1) ret.ExtraDirection |= PlayerInputDirection.Up | PlayerInputDirection.Right;

            return ret;
        }
    }
}
