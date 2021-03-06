﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel;

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
using Kbtter4.Cache;
using Kbtter3.Query;

namespace Kbtter4.Models
{
    public static class Kbtter4Extensions
    {
        public static string TrimLineFeeds(this string t)
        {
            return t.Replace("\n", "").Replace("\r", "");
        }

        public static bool EndsWith(this string t, IEnumerable<string> es, StringComparison comparison)
        {
            foreach (var i in es)
            {
                if (t.EndsWith(i, comparison)) return true;
            }
            return false;
        }
    }
}
