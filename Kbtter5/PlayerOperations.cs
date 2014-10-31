using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace Kbtter5
{

    public static class PlayerOperations
    {
        private static int PlayerBulletLayer = (int)GameLayer.PlayerBullet;
        public static IEnumerator<bool> MouseOperaiton(PlayerUser player)
        {
            int count = 0;

            while (true)
            {
                int x, y;
                DX.GetMousePoint(out x, out y);
                if (player.Operatable)
                {
                    player.X = x;
                    player.Y = y;
                }
                var msi = DX.GetMouseInput();
                if ((msi & DX.MOUSE_INPUT_LEFT) != 0 && count % player.ShotInterval == 0)
                {
                    var dr = (count / player.ShotInterval) % 8;
                    var pad = Gamepad.GetState();
                    if (DX.CheckHitKey(DX.KEY_INPUT_D) == 1) dr = 0;
                    if (DX.CheckHitKey(DX.KEY_INPUT_C) == 1) dr = 1;
                    if (DX.CheckHitKey(DX.KEY_INPUT_X) == 1) dr = 2;
                    if (DX.CheckHitKey(DX.KEY_INPUT_Z) == 1) dr = 3;
                    if (DX.CheckHitKey(DX.KEY_INPUT_A) == 1) dr = 4;
                    if (DX.CheckHitKey(DX.KEY_INPUT_Q) == 1) dr = 5;
                    if (DX.CheckHitKey(DX.KEY_INPUT_W) == 1) dr = 6;
                    if (DX.CheckHitKey(DX.KEY_INPUT_E) == 1) dr = 7;
                    if (pad.Direction == GamepadDirection.Right) dr = 0;
                    if (pad.Direction == (GamepadDirection.Right | GamepadDirection.Down)) dr = 1;
                    if (pad.Direction == GamepadDirection.Down) dr = 2;
                    if (pad.Direction == (GamepadDirection.Down | GamepadDirection.Left)) dr = 3;
                    if (pad.Direction == GamepadDirection.Left) dr = 4;
                    if (pad.Direction == (GamepadDirection.Left | GamepadDirection.Up)) dr = 5;
                    if (pad.Direction == GamepadDirection.Up) dr = 6;
                    if (pad.Direction == (GamepadDirection.Up | GamepadDirection.Right)) dr = 7;

                    player.TryShot(Math.PI / 4 * dr, 8);
                }
                if ((msi & DX.MOUSE_INPUT_RIGHT) != 0)
                {
                    player.TryBomb();
                }
                count++;
                yield return true;
            }
        }

        public static IEnumerator<bool> KeyboardOperation(PlayerUser player)
        {
            int count = 0;
            //TODO: ユーザー固有値
            double speed = 4.0;
            player.X = 320;
            player.Y = 240;
            int stdr = -1, dr = 0, drs = 0;
            GamepadState pad;

            while (true)
            {

                dr = 0;
                drs = 0;
                pad = Gamepad.GetState();

                if (pad.Direction.HasFlag(GamepadDirection.Right))
                {
                    dr += 0;
                    drs++;
                    player.X += speed;
                }
                if (pad.Direction.HasFlag(GamepadDirection.Down))
                {
                    dr += 10;
                    drs++;
                    player.Y += speed;
                }
                if (pad.Direction.HasFlag(GamepadDirection.Left))
                {
                    dr += 12;
                    drs++;
                    player.X -= speed;
                }
                if (pad.Direction.HasFlag(GamepadDirection.Up))
                {
                    dr += 14;
                    drs++;
                    player.Y -= speed;
                }

                //TODO: 発射方向固定

                dr = (drs > 0) ? (dr / drs) % 8 : ((count / player.ShotInterval) % 8);

                if (pad.Buttons[0] && (count % player.ShotInterval) == 0)
                    player.ParentManager.Add(new PlayerImageBullet(player, BulletPatterns.Linear(Math.PI / 4 * dr, 8, 90), CommonObjects.ImageShot, player.ShotStrength)
                  {
                      X = player.X,
                      Y = player.Y,
                      HomeX = 8,
                      HomeY = 8,
                  }, PlayerBulletLayer);

                if (pad.Buttons[1]) player.TryBomb();

                count++;
                yield return true;
            }
        }
    }
}
