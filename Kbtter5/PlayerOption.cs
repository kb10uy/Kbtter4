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
    public class PlayerOption : UserSprite
    {
        public PlayerUser Parent { get; protected set; }
        public UserInformation Information { get; protected set; }

        public PlayerOption()
        {
            CollisionRadius = 0;
            GrazeRadius = 0;
            DamageKind = ObjectKind.None;
            TargetKind = ObjectKind.None;
            MyKind = ObjectKind.Player;
        }

        public PlayerOption(PlayerUser p,UserInformation user)
            : this()
        {
            Information = user;
            SourceUser = Information.SourceUser;
        }
    }
}
