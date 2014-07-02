using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace Kbtter4.Cache
{
    public sealed class Kbtter4Cache : IDisposable
    {
        public SQLiteConnection Connection { get; private set; }

        public Kbtter4Cache(string fileName)
        {
            var csb = new SQLiteConnectionStringBuilder()
            {
                DataSource = fileName,
                Version = 3,
                SyncMode = SynchronizationModes.Off,
                JournalMode = SQLiteJournalModeEnum.Memory
            };
            Connection = new SQLiteConnection(csb.ToString());
            Connection.Open();

            CreateTables();
        }

        private void CreateTables()
        {
            using (var tr = Connection.BeginTransaction())
            {
                try
                {
                    Connection.Execute("create table if not exists Favorites(Id unique,CreatedDate,ScreenName)");
                    Connection.Execute("create table if not exists Retweets(Id unique,OriginalId,CreatedDate,ScreenName)");
                    Connection.Execute("create table if not exists Bookmarks(Id unique,CreatedDate,ScreenName,Name,Text)");
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                }
            }
        }

        public void AddFavorite(Kbtter4FavoriteCache data)
        {
            using (var tr = Connection.BeginTransaction())
            {
                try
                {
                    Connection.Execute("insert or ignore into Favorites values(@Id,@CreatedDate,@ScreenName)", data);
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                }
            }
        }

        public void RemoveFavorite(long id)
        {
            using (var tr = Connection.BeginTransaction())
            {
                try
                {
                    Connection.Execute("delete from Favorites where Id=@Id", new { Id = id });
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                }
            }
        }

        public IEnumerable<Kbtter4FavoriteCache> Favorites()
        {
            return Connection.Query<Kbtter4FavoriteCache>("select * from Favorites");
        }


        public void AddRetweet(Kbtter4RetweetCache data)
        {
            using (var tr = Connection.BeginTransaction())
            {
                try
                {
                    Connection.Execute("insert or ignore into Favorites values(@Id,@OriginalId,@CreatedDate,@ScreenName)", data);
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                }
            }
        }

        public void RemoveRetweet(long id)
        {
            using (var tr = Connection.BeginTransaction())
            {
                try
                {
                    Connection.Execute("delete from Retweets where Id=@Id", new { Id = id });
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                }
            }
        }

        public IEnumerable<Kbtter4RetweetCache> Retweets()
        {
            return Connection.Query<Kbtter4RetweetCache>("select * from Retweets");
        }


        public void Dispose()
        {
            Connection.Dispose();
        }
    }


}
