using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace Kbtter5
{

    public static class PlayerMovingOperations
    {
        public static IEnumerator<bool> MouseOperaiton(PlayerUser player)
        {
            while (true)
            {
                if (player.Operatable)
                {
                    player.X = player.CurrentInput.MouseX;
                    player.Y = player.CurrentInput.MouseY;
                }
                yield return true;
            }
        }
    }
}
