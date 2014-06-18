using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using CoreTweet;

namespace Kbtter4.Models
{
    public sealed class Kbtter4User : NotificationObject
    {
        public User SourceUser { get; private set; }

        public Kbtter4User()
        {

        }

        public Kbtter4User(User user)
        {
            SourceUser = user;
        }
    }
}
