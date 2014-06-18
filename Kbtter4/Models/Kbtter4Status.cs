using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using CoreTweet;

namespace Kbtter4.Models
{
    public sealed class Kbtter4Status : NotificationObject
    {
        public Status SourceStatus { get; private set; }

        public Kbtter4Status(Status src)
        {
            SourceStatus = src;
        }

        public long Id { get { return SourceStatus.Id; } }

        public IList<Contributors> Contributors { get { return SourceStatus.Contributors; } }

        public Coordinates Coordinates { get { return SourceStatus.Coordinates; } }

        public DateTime CreatedAt { get { return SourceStatus.CreatedAt.LocalDateTime; } }

        public long CurrentUserRetweet { get { return SourceStatus.CurrentUserRetweet ?? 0; } }

        public Entities Entities { get { return SourceStatus.Entities; } }

        public Entities ExtendedEntities { get { return SourceStatus.ExtendedEntities; } }

        public int FavoriteCount { get { return SourceStatus.FavoriteCount ?? 0; } }

        public bool IsFavorited
        {
            get
            { return SourceStatus.IsFavorited ?? false; }
            set
            {
                if (SourceStatus.IsFavorited == value)
                    return;
                SourceStatus.IsFavorited = value;
                RaisePropertyChanged();
            }
        }

        public string InReplyToScreenName { get { return SourceStatus.InReplyToScreenName; } }

        public long InReplyToStatusId { get { return SourceStatus.InReplyToStatusId ?? 0; } }

        public long InReplyToUserId { get { return SourceStatus.InReplyToUserId ?? 0; } }

        public Place Place { get { return SourceStatus.Place; } }

        public bool PossiblySensitive { get { return SourceStatus.PossiblySensitive ?? false; } }

        public IDictionary<string, object> Scopes { get { return SourceStatus.Scopes; } }

        public int RetweetCount { get { return SourceStatus.RetweetCount ?? 0; } }

        public bool IsRetweeted
        {
            get
            { return SourceStatus.IsRetweeted ?? false; }
            set
            {
                if (SourceStatus.IsRetweeted == value)
                    return;
                SourceStatus.IsRetweeted = value;
                RaisePropertyChanged();
            }
        }

        public Kbtter4Status RetweetedStatus { get; private set; }

        public string Source { get { return SourceStatus.Source; } }

        public string Text { get { return SourceStatus.Text; } }
       
        /*
        public Place Place { get { return SourceStatus.Place; } }

        public Place Place { get { return SourceStatus.Place; } }

        public Place Place { get { return SourceStatus.Place; } }

        public Place Place { get { return SourceStatus.Place; } }
        */

    }
}
