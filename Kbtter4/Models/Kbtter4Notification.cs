using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using CoreTweet;
using CoreTweet.Streaming;

namespace Kbtter4.Models
{
    public sealed class Kbtter4Notification : NotificationObject
    {
        public EventMessage SourceEvent { get;private set; }

        public StatusMessage SourceStatus { get; private set; }

        public Kbtter4NotificationKind Kind { get; private set; }

        public Kbtter4Notification(EventMessage ev)
        {
            SourceEvent = ev;
            switch (SourceEvent.Event)
            {
                case EventCode.Follow:
                    Kind = Kbtter4NotificationKind.Followed;
                    break;
                case EventCode.Unfollow:
                    Kind = Kbtter4NotificationKind.Unfollowed;
                    break;
                case EventCode.Favorite:
                    Kind = Kbtter4NotificationKind.Favorited;
                    break;
                case EventCode.Unfavorite:
                    Kind = Kbtter4NotificationKind.Unfavorited;
                    break;
                case EventCode.ListMemberAdded:
                    Kind = Kbtter4NotificationKind.ListAdded;
                    break;
                case EventCode.ListMemberRemoved:
                    Kind = Kbtter4NotificationKind.ListRemoved;
                    break;
                case EventCode.Block:
                    Kind = Kbtter4NotificationKind.Blocked;
                    break;
            }
        }

        public Kbtter4Notification(StatusMessage st)
        {
            SourceStatus = st;
            Kind = Kbtter4NotificationKind.Retweeted;
        }
    }


    public enum Kbtter4NotificationKind
    {
        Undefined,
        None,
        Favorited,
        Unfavorited,
        Followed,
        Unfollowed,
        Retweeted,
        Blocked,
        Unblocked,
        ListAdded,
        ListRemoved,
    }
}
