using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Kbtter3.Query;
using CoreTweet;

namespace Kbtter4.Models
{
    public sealed class StatusTimeline : NotificationObject
    {
        public ObservableSynchronizedCollection<Status> Statuses { get; private set; }
        public Kbtter3Query Query { get; private set; }

        public StatusTimeline()
        {
            Statuses = new ObservableSynchronizedCollection<Status>();
            Query = new Kbtter3Query("true");
        }

        public StatusTimeline(string q)
        {
            Statuses = new ObservableSynchronizedCollection<Status>();
            Query = new Kbtter3Query(q);
        }

        public void TryAddStatus(Status st)
        {
            Query.ClearVariables();
            Query.SetVariable("Status", st);
            if (Query.Execute().AsBoolean()) Statuses.Insert(0, st);
        }
    }
}
