using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Specialized;

namespace Kbtter4.Ayaya
{
    /// <summary>
    /// 診断メーカーに直接アクセスして結果を取得するやつ
    /// </summary>
    public static class Kbtter4ShindanMakerCooperator
    {
        public static readonly string ShindanMakerBaseAddress = "http://shindanmaker.com/";
        public static readonly Regex ResultRegex = new Regex("=\"https://twitter\\.com/intent/tweet\\?text=(?<text>.+?)&url=http://shindanmaker\\.com/[0-9]+");
        /// <summary>
        /// 診断する
        /// </summary>
        /// <param name="sid">診断ID</param>
        /// <param name="name">名前</param>
        /// <returns>結果</returns>
        public static async Task<string> DiagnoseAsync(int sid, string name)
        {
            using (var wc = new WebClient())
            {
                var ruri = new Uri(ShindanMakerBaseAddress + sid.ToString());
                var data = await wc.UploadValuesTaskAsync(ruri, new NameValueCollection() { { "u", name } });
                var enc = Encoding.UTF8;
                var ret = enc.GetString(data);
                var m = ResultRegex.Match(ret);
                if (!m.Success) return null;
                var uet = m.Groups["text"].Value;
                var res = Uri.UnescapeDataString(uet.Replace("+", "%20"));
                Console.WriteLine(res);
                return res;
            }
        }
    }
}
