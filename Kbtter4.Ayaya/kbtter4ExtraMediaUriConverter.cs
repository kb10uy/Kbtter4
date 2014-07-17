using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kbtter4.Ayaya
{
    public static class Kbtter4ExtraMediaUriConverter
    {
        public static readonly string RegexPatternFileName = "config/eximg_regex.json";

        private static Dictionary<string, Regex> Regexes;

        public static IEnumerable<Uri> TryGetDirectUri(IEnumerable<Uri> uris)
        {
            var ret = new List<Uri>();
            using (var wc = new WebClient())
            {
                if (Regexes == null)
                {
                    Regexes = new Dictionary<string, Regex>();
                    if (!File.Exists(RegexPatternFileName))
                    {
                        var ls = wc.DownloadString("http://img.azyobuzi.net/api/regex.json");
                        File.WriteAllText(RegexPatternFileName, ls);
                    }
                    var r = JsonConvert.DeserializeObject<List<AzyobuziNetRegexPattern>>(File.ReadAllText(RegexPatternFileName));
                    Console.WriteLine();
                    foreach (var i in r)
                    {
                        Regexes[i.name] = new Regex(i.regex, RegexOptions.IgnoreCase);
                    }
                }
                foreach (var i in uris)
                {
                    var mk = Regexes.Select(p => p.Value).FirstOrDefault(p => p.IsMatch(i.ToString()));
                    if (mk == null) continue;
                    try
                    {
                        var rs = wc.DownloadString("http://img.azyobuzi.net/api/all_sizes.json?uri=" + i.ToString());
                        dynamic res = JObject.Parse(rs);
                        if (res.error != null) continue;
                        ret.Add((Uri)res.full);
                    }
                    catch { }
                }
            }
            return ret;
        }
    }

    public class AzyobuziNetRegexPattern
    {
        public string name;
        public string regex;
    }
}
