using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Net;
using System.Threading.Tasks;

using CoreTweet;
using CoreTweet.Core;
using CoreTweet.Rest;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Kbtter4.Models.Plugin;
using Kbtter4.Tenko;
using Kbtter4.Ayaya;
using Kbtter4.Cache;
using Kbtter3.Query;


using Livet;
namespace Kbtter4.Models
{
    public class Kbtter4Draft : NotificationObject
    {
        public DateTime CreatedDate { get; set; }

        public string Text { get; set; }

        public bool IsReply { get; set; }

        public long InReplyToStatusId { get; set; }

        public Uri InReplyToUserProfileImageUri { get; set; }

        public string InReplyToStatusOnelineText { get; set; }

        public string InReplyToUserScreenName { get; set; }

        public Kbtter4Draft()
        {

        }

        public Kbtter4Draft(string text, DateTime time, bool isReply, Status reply)
        {
            Text = text;
            CreatedDate = time;
            if (isReply)
            {
                IsReply = true;
                InReplyToStatusId = reply.Id;
                InReplyToStatusOnelineText = reply.Text
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Replace("&gt;", ">")
                    .Replace("&lt;", "<")
                    .Replace("&amp;", "&");
                InReplyToUserScreenName = reply.User.ScreenName;
                InReplyToUserProfileImageUri = reply.User.ProfileImageUrlHttps;
            }
        }

        public Status CreateVirtualStatus()
        {
            return new Status
            {
                Text = this.InReplyToStatusOnelineText,
                Id = this.InReplyToStatusId,
                User = new User
                {
                    ScreenName = this.InReplyToUserScreenName,
                    ProfileImageUrlHttps = this.InReplyToUserProfileImageUri
                }
            };
        }
    }
}
