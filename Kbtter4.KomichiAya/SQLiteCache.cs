using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.SQLite.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;

namespace Kbtter4.KomichiAya
{
    public class SQLiteCache
    {
        /// <summary>
        /// こねくちん
        /// </summary>
        public SQLiteConnection Connection { get; protected set; }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="dbName">ファイル名</param>
        public SQLiteCache(string dbName)
        {
            var csb = new SQLiteConnectionStringBuilder()
            {
                DataSource = dbName,
                Version = 3,
                SyncMode = SynchronizationModes.Off,
                JournalMode = SQLiteJournalModeEnum.Memory
            };
            Connection = new SQLiteConnection(csb.ToString());
        }

        public bool CreateTable(Type targetType, string tableName)
        {
            var ttat = targetType.GetCustomAttributes(typeof(TableAttribute), true);
            if (ttat.Length == 0) throw new TypeLoadException("Table属性を指定しろ💢");
            
            var prs = targetType.GetProperties();
            if (prs.Length == 0) throw new TypeLoadException("プロパティを登録しろ💢");

            var rgp = prs.Where(p => p.GetCustomAttributes(typeof(ColumnAttribute), true).Length != 0);
            if (prs.Length == 0) throw new TypeLoadException("Column属性を登録しろ💢");
            return true;
            
            //TODO : Columnの内容に応じたCREATE TABLE文の作成
        }
    }
}
