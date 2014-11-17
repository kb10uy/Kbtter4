using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;

namespace Kbtter5
{
    public static class PlayerShotOperations
    {
        private static int PlayerBulletLayer = (int)GameLayer.PlayerBullet;

        public static IEnumerator<bool> Default(PlayerUser player)
        {
            int count = 0;

            while (true)
            {
                if (player.IsTriggerShottableTiming && player.Operatable)
                {
                    var dr = (count / player.ShotInterval) % 8;
                    var pad = player.CurrentInput;

                    if (pad.ExtraDirection == PlayerInputDirection.Right) dr = 0;
                    if (pad.ExtraDirection == (PlayerInputDirection.Right | PlayerInputDirection.Down)) dr = 1;
                    if (pad.ExtraDirection == PlayerInputDirection.Down) dr = 2;
                    if (pad.ExtraDirection == (PlayerInputDirection.Down | PlayerInputDirection.Left)) dr = 3;
                    if (pad.ExtraDirection == PlayerInputDirection.Left) dr = 4;
                    if (pad.ExtraDirection == (PlayerInputDirection.Left | PlayerInputDirection.Up)) dr = 5;
                    if (pad.ExtraDirection == PlayerInputDirection.Up) dr = 6;
                    if (pad.ExtraDirection == (PlayerInputDirection.Up | PlayerInputDirection.Right)) dr = 7;

                    player.ParentManager.Add(
                        new PlayerImageBullet(player, BulletPatterns.Linear(Math.PI / 4.0 * dr, 8, 90),
                        CommonObjects.ImageShot, player.ShotStrength)
                    {
                        X = player.X,
                        Y = player.Y,
                        HomeX = 8,
                        HomeY = 8,
                    }, PlayerBulletLayer);
                }
                count++;
                yield return true;
            }
        }
    }
}
