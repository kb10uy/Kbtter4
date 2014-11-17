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
    #region PlayerOption
    public class PlayerOption : UserSprite
    {
        public PlayerUser Parent { get; protected set; }
        public UserInformation Information { get; protected set; }
        public IEnumerator<bool> ExecuteCoroutine { get; protected set; }
        private bool IsDefaultOperationPrevented { get; set; }
        public OptionStateRequest RequestedState { get; protected set; }

        public PlayerOption()
        {
            CollisionRadius = 0;
            GrazeRadius = 0;
            DamageKind = ObjectKind.None;
            TargetKind = ObjectKind.None;
            MyKind = ObjectKind.Player;
            ScaleX = ScaleY = 0.8;
            ExecuteCoroutine = Execute();
        }

        public PlayerOption(PlayerUser p, UserInformation user)
            : this()
        {
            Information = user;
            SourceUser = Information.SourceUser;
            Image = UserImageManager.GetUserImage(SourceUser);
        }

        public override IEnumerator<bool> Tick()
        {
            while (true)
            {
                IsDefaultOperationPrevented = false;
                IsDead = !(ExecuteCoroutine.MoveNext() && ExecuteCoroutine.Current);
                if (!IsDefaultOperationPrevented) ApplyRequest();
                yield return true;
            }
        }

        public virtual IEnumerator<bool> Execute()
        {
            while (true) yield return true;
        }

        public void PreventParentOperation()
        {
            IsDefaultOperationPrevented = true;
        }

        public void RequestState(OptionStateRequest st)
        {
            RequestedState = st;
        }

        protected void ApplyRequest()
        {
            X = RequestedState.X ?? X;
            Y = RequestedState.Y ?? Y;
            Angle = RequestedState.Angle ?? Angle;
            Alpha = RequestedState.Alpha ?? Alpha;
            ScaleX = RequestedState.ScaleX ?? ScaleX;
            ScaleY = RequestedState.ScaleY ?? ScaleY;
        }
    }

    public class OptionStateRequest
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Angle { get; set; }
        public double? Alpha { get; set; }
        public double? ScaleX { get; set; }
        public double? ScaleY { get; set; }
    }
    #endregion
}
