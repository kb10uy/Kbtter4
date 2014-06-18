using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Kbtter3.Query;

namespace Kbtter4.Models
{
    public sealed class DirectMessageTimeline : NotificationObject
    {
        public ObservableSynchronizedCollection<Kbtter4DirectMessage> DirectMessages { get;private set; }
        public Kbtter3Query Query { get;private set; }

        public DirectMessageTimeline()
        {
            DirectMessages = new ObservableSynchronizedCollection<Kbtter4DirectMessage>();
            Query = new Kbtter3Query("true");
        }

        public DirectMessageTimeline(string q)
        {
            DirectMessages = new ObservableSynchronizedCollection<Kbtter4DirectMessage>();
            Query = new Kbtter3Query(q);
        }
    }
}
