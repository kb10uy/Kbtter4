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
        public EventMessage Source { get;private set; }

        public Kbtter4Notification(EventMessage ev)
        {
            Source = ev;
        }
    }
}
