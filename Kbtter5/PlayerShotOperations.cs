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
                        new PlayerImageBullet(player, Linear(Math.PI / 4.0 * dr, 8, 90),
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

        private static CoroutineFunction<UserSprite, PlayerBullet> Linear(double angle, double speed, int time)
        {
            return (par, b) => Linear(par, b, angle, speed, time);
        }

        private static IEnumerator<bool> Linear(UserSprite par, PlayerBullet b, double angle, double speed, int time)
        {
            for (int i = 0; i < time; i++)
            {
                b.X += Math.Cos(angle) * speed;
                b.Y += Math.Sin(angle) * speed;
                yield return true;
            }
            b.IsDead = true;
            yield break;
        }
    }
}
