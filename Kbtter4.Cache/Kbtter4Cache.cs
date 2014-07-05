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
    /// <summary>
    /// Kbtter4用各種キャッシュを提供します。
    /// </summary>
    public sealed class Kbtter4Cache : IDisposable
    {
        /// <summary>
        /// 接続
        /// </summary>
        public SQLiteConnection Connection { get; private set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        public Kbtter4Cache(string fileName)
        {
            var csb = new SQLiteConnectionStringBuilder()
            {
                DataSource = fileName,
                Version = 3,
                SyncMode = SynchronizationModes.Normal,
                JournalMode = SQLiteJournalModeEnum.Wal
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

        /// <summary>
        /// お気に入りを追加
        /// </summary>
        /// <param name="data">お気に入りキャッシュ用データ</param>
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

        /// <summary>
        /// お気に入りを削除
        /// </summary>
        /// <param name="id">ID</param>
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

        /// <summary>
        /// お気に入りを取得
        /// </summary>
        /// <returns>お気に入りキャッシュリスト</returns>
        public IEnumerable<Kbtter4FavoriteCache> Favorites()
        {
            return Connection.Query<Kbtter4FavoriteCache>("select * from Favorites");
        }

        /// <summary>
        /// リツイートを追加
        /// </summary>
        /// <param name="data">リツイートキャッシュ用データ</param>
        public void AddRetweet(Kbtter4RetweetCache data)
        {
            using (var tr = Connection.BeginTransaction())
            {
                try
                {
                    Connection.Execute("insert or ignore into Retweets values(@Id,@OriginalId,@CreatedDate,@ScreenName)", data);
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                }
            }
        }

        /// <summary>
        /// リツイートを削除
        /// </summary>
        /// <param name="id">RT元ID</param>
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

        /// <summary>
        /// リツイートを取得
        /// </summary>
        /// <returns>リツイートキャッシュリスト</returns>
        public IEnumerable<Kbtter4RetweetCache> Retweets()
        {
            return Connection.Query<Kbtter4RetweetCache>("select * from Retweets");
        }

        /// <summary>
        /// 開放
        /// </summary>
        public void Dispose()
        {
            Connection.Dispose();
        }
    }


}
