using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using DxLibDLL;
using CoreTweet;

namespace Kbtter5
{
    public class DisplayObject
    {
        public double X { get; protected set; }
        public double Y { get; protected set; }

        public virtual IEnumerator<bool> Tick()
        {
            while (true) yield return true;
        }

        public virtual IEnumerator<bool> Draw()
        {
            while (true) yield return true;
        }
    }

    public class Bullet : DisplayObject
    {
        public bool IsDead { get; protected set; }
    }

    public class UserSprite : DisplayObject
    {
        public int Image { get; set; }

        public UserSprite()
        {
            Image = CommonObjects.ImageLogo;
        }
    }

    public class EnemyUser : UserSprite
    {

    }

    public class PlayerUser : UserSprite
    {

    }

    public struct Point
    {
        public double X;
        public double Y;
    }
}