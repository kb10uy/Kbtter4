using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;


namespace Kbtter4.Models.Caching
{
    [Table(Name = "Favorites")]
    internal class FavoriteCache : IEqualityComparer<FavoriteCache>
    {
        [Column(Name = "ID", IsPrimaryKey = true)]
        public long Id { get; set; }

        [Column(Name = "DATE")]
        public DateTime CreatedDate { get; set; }

        [Column(Name = "NAME")]
        public string ScreenName { get; set; }

        public bool Equals(FavoriteCache x, FavoriteCache y)
        {
            if (x == y) return true;
            return x.Id == y.Id;
        }

        public int GetHashCode(FavoriteCache obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    [Table(Name = "Retweets")]
    internal class RetweetCache : IEqualityComparer<RetweetCache>
    {
        [Column(Name = "ID", IsPrimaryKey = true)]
        public long Id { get; set; }

        [Column(Name = "ORIGINALID")]
        public long OriginalId { get; set; }

        [Column(Name = "DATE")]
        public DateTime CreatedDate { get; set; }

        [Column(Name = "NAME")]
        public string ScreenName { get; set; }

        public bool Equals(RetweetCache x, RetweetCache y)
        {
            if (x == y) return true;
            return x.Id == y.Id;
        }

        public int GetHashCode(RetweetCache obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    [Table(Name = "Bookmarks")]
    internal class BookmarkCache : IEqualityComparer<BookmarkCache>
    {
        [Column(Name = "ID", IsPrimaryKey = true)]
        public long Id { get; set; }

        [Column(Name = "DATE")]
        public DateTime CreatedDate { get; set; }

        [Column(Name = "SCREENNAME")]
        public string ScreenName { get; set; }

        [Column(Name = "NAME")]
        public string Name { get; set; }

        [Column(Name = "STATUS")]
        public string Text { get; set; }

        public bool Equals(BookmarkCache x, BookmarkCache y)
        {
            if (x == y) return true;
            return x.Id == y.Id;
        }

        public int GetHashCode(BookmarkCache obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
