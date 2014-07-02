using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Kbtter4.Cache
{

    public class Kbtter4FavoriteCache
    {
        
        public long Id { get; set; }

        
        public DateTime CreatedDate { get; set; }

        
        public string ScreenName { get; set; }
    }

    public class Kbtter4RetweetCache
    {
        
        public long Id { get; set; }

        
        public long OriginalId { get; set; }

        
        public DateTime CreatedDate { get; set; }

        
        public string ScreenName { get; set; }

    }

    public class Kbtter4BookmarkCache
    {

        public long Id { get; set; }


        public DateTime CreatedDate { get; set; }


        public string ScreenName { get; set; }


        public string Name { get; set; }


        public string Text { get; set; }

    }
}
