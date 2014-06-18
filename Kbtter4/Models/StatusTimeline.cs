using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Kbtter3.Query;

namespace Kbtter4.Models
{
    public sealed class StatusTimeline : NotificationObject
    {
        public ObservableSynchronizedCollection<Kbtter4Status> Statuses { get; private set; }
        public Kbtter3Query Query { get; private set; }

        public StatusTimeline()
        {
            Statuses = new ObservableSynchronizedCollection<Kbtter4Status>();
            Query = new Kbtter3Query("true");
        }

        public StatusTimeline(string q)
        {
            Statuses = new ObservableSynchronizedCollection<Kbtter4Status>();
            Query = new Kbtter3Query(q);
        }
    }
}
