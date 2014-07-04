using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Kbtter4.Cache
{
    /// <summary>
    /// お気に入りキャッシュオブジェクト
    /// </summary>
    public class Kbtter4FavoriteCache
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// CreatedAt
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        public string ScreenName { get; set; }
    }

    /// <summary>
    /// リツイートキャッシュオブジェクト
    /// </summary>
    public class Kbtter4RetweetCache
    {
        /// <summary>
        /// Rt自体のID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// RT元のID
        /// </summary>
        public long OriginalId { get; set; }

        /// <summary>
        /// CreatedAt
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        public string ScreenName { get; set; }

    }

    /// <summary>
    /// ブックマークキャッシュオブジェクト
    /// </summary>
    public class Kbtter4BookmarkCache
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// CreatedAt
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        public string ScreenName { get; set; }

        /// <summary>
        /// 当時の名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// テキスト
        /// </summary>
        public string Text { get; set; }

    }
}
