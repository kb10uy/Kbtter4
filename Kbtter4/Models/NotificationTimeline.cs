using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Kbtter3.Query;

namespace Kbtter4.Models
{
    public sealed class NotificationTimeline : NotificationObject
    {
        public ObservableSynchronizedCollection<Kbtter4Notification> Notifications { get; private set; }
        public Kbtter3Query Query { get; private set; }
        Kbtter4Setting Setting;

        public NotificationTimeline(Kbtter4Setting kb)
        {
            Setting = kb;
            Notifications = new ObservableSynchronizedCollection<Kbtter4Notification>();
            Query = new Kbtter3Query("true");
        }

        public NotificationTimeline(Kbtter4Setting kb,string q)
        {
            Setting = kb;
            Notifications = new ObservableSynchronizedCollection<Kbtter4Notification>();
            Query = new Kbtter3Query(q);
        }

        public void TryAddNotification(Kbtter4Notification nt)
        {
            Query.ClearVariables();
            Query.SetVariable("Notification", nt);
            if (Query.Execute().AsBoolean()) Notifications.Insert(0, nt);
            if (Notifications.Count > Setting.Timelines.HomeNotificationTimelineMax) Notifications.RemoveAt(Notifications.Count - 1);
        }
        /*
        public bool CheckQuery(string q)
        {
            try
            {
                var newq = new Kbtter3Query(q);

            }
            catch
            {
                return false;
            }
            return true;
        }
        */
    }
}
