using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Kbtter3.Query;
using CoreTweet;

namespace Kbtter4.Models
{
    public sealed class DirectMessageTimeline
    {
        public ObservableSynchronizedCollection<DirectMessage> DirectMessages { get; private set; }
        public Kbtter3Query Query { get; private set; }
        public User Party { get; private set; }
        Kbtter4Setting Setting;

        public DirectMessageTimeline(Kbtter4Setting kb, User p)
        {
            Setting = kb;
            DirectMessages = new ObservableSynchronizedCollection<DirectMessage>();
            Query = new Kbtter3Query("true");
            Party = p;
        }

        public DirectMessageTimeline(Kbtter4Setting kb, string q, User p)
        {
            Setting = kb;
            DirectMessages = new ObservableSynchronizedCollection<DirectMessage>();
            Query = new Kbtter3Query(q);
            Party = p;
        }

        public void TryAddDirectMessage(DirectMessage dm)
        {
            if (dm.Recipient.Id != Party.Id && dm.Sender.Id != Party.Id) throw new InvalidOperationException("宛先のDMTLが間違ってます");
            Query.ClearVariables();
            Query.SetVariable("DirectMessage", dm);
            if (Query.Execute().AsBoolean()) DirectMessages.Add(dm);
            if (DirectMessages.Count > Setting.Timelines.HomeDirectMessageTimelineMax) DirectMessages.RemoveAt(DirectMessages.Count - 1);
        }

    }
}
