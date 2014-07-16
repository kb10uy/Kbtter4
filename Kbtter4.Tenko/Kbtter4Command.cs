using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter4.Tenko
{

    /// <summary>
    /// Kbtter4用コマンドを管理します。
    /// helpコマンドが標準で有効です。
    /// </summary>
    public sealed class Kbtter4CommandManager
    {
        /// <summary>
        /// コマンド
        /// </summary>
        public IList<Kbtter4Command> Commands { get; private set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public Kbtter4CommandManager()
        {
            Commands = new List<Kbtter4Command>();
            RegisterDefaultCommand();
        }

        /// <summary>
        /// コマンドを追加します。
        /// 名前が重複する場合は追加されません。
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <returns>追加できた場合はtrue</returns>
        public bool AddCommand(Kbtter4Command cmd)
        {
            if (Commands.Count(p => p.Name == cmd.Name) != 0) return false;
            Commands.Add(cmd);
            return true;
        }

        /// <summary>
        /// コマンドを追加します。
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="overwrite">コマンドが重複する場合に上書きする場合はtrue</param>
        /// <returns>追加できた場合はtrue</returns>
        public bool AddCommand(Kbtter4Command cmd, bool overwrite)
        {
            var ov = Commands.FirstOrDefault(p => p.Name == cmd.Name);
            if (!overwrite && ov != null) return false;
            if (ov != null) Commands.Remove(ov);
            Commands.Add(cmd);
            return true;
        }

        /// <summary>
        /// 実行します。
        /// </summary>
        /// <param name="cmd">コマンドライン</param>
        /// <returns>結果</returns>
        public async Task<string> Execute(string cmd)
        {
            var cmdret = Kbtter4CommandlineParser.Parse(cmd);
            if (cmdret == null)
            {
                return "構文が間違っているようです";
            }
            var ecm = Commands.FirstOrDefault(p => p.Name == cmdret.Name);
            if (ecm == null)
            {
                return "指定されたコマンドがありません : " + cmdret.Name;
            }

            foreach (var i in cmdret.Parameters)
            {
                if (!ecm.Parameters.Any(p => p.Name == i.Key))
                {
                    return "不明なパラメータです : " + i.Key;
                }
            }

            var reql = ecm.Parameters.Where(p => p.IsRequired).Select(p => p.Name);
            var alpl = cmdret.Parameters.Select(p => p.Key);
            if (!reql.All(p => alpl.Any(q => p == q)))
            {
                return "必須パラメータが不足しています\n必須パラメータ : " + string.Join(" , ", reql);
            }

            if (ecm.IsAsync)
            {
                if (ecm.AsynchronousFunction != null)
                {
                    return await ecm.AsynchronousFunction(cmdret.Parameters);
                }
                else
                {
                    return "動作が定義されていません";
                }
            }
            else
            {
                if (ecm.Function != null)
                {
                    return ecm.Function(cmdret.Parameters);
                }
                else
                {
                    return "動作が定義されていません";
                }
            }
            
            
        }

        /// <summary>
        /// コマンドを名前で取得します。
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>コマンド</returns>
        public Kbtter4Command this[string name]
        {
            get
            {
                return Commands.FirstOrDefault(p => p.Name == name);
            }
        }

        #region デフォコマンド関係
        private void RegisterDefaultCommand()
        {
            var hc = new Kbtter4Command
            {
                Name = "help",
                Description = "登録されているコマンドの詳細を表示します。",
                Function = Help,
            };
            hc.Parameters.Add(new Kbtter4CommandParameter { Name = "cmd"});
            AddCommand(hc);
        }

        private string Help(IDictionary<string, object> args)
        {
            if (!args.ContainsKey("cmd"))
            {
                var r = "コマンド一覧\n";
                foreach (var i in Commands) r += i.Name + "\n";
                return r;
            }
            if (this[args["cmd"] as string] == null) return "指定されたコマンドがありません";
            var ret = args["cmd"] + "コマンドの詳細 : \n";
            ret += this[args["cmd"] as string].Description;
            return ret;
        }

        #endregion
    }

    /// <summary>
    /// Kbtter4コマンドライン用のコマンドを定義します。
    /// </summary>
    public sealed class Kbtter4Command
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// コマンドの詳細。
        /// helpコマンドで表示されます。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// パラメータ
        /// </summary>
        public IList<Kbtter4CommandParameter> Parameters { get; set; }

        /// <summary>
        /// 実行されるファンクション
        /// </summary>
        public Func<IDictionary<string, object>, string> Function { get; set; }

        /// <summary>
        /// 実行されるアクション。非同期メソッドを使用したい場合はこちらにメソッドを割り当てて
        /// IsAsyncをにtrueしてください。
        /// </summary>
        public Func<IDictionary<string, object>, Task<string>> AsynchronousFunction { get; set; }

        /// <summary>
        /// アクションを非同期で実行するかどうかのフラグを取得・設定します。
        /// trueの場合、FunctionのかわりにAsynchronousFunctionが呼び出されます。
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public Kbtter4Command()
        {
            Name = "";
            Description = "There is no description for this command.";
            Parameters = new List<Kbtter4CommandParameter>();
            IsAsync = false;
        }
    }

    /// <summary>
    /// Kbtter4コマンドライン用のコマンドのパラメータ情報を定義します。
    /// </summary>
    public sealed class Kbtter4CommandParameter
    {
        /// <summary>
        /// パラメータ名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 必須パラメータの場合はtrue。
        /// その際にこのパラメータが指定されないコマンドが処理されるとエラー文字列を返します。
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public Kbtter4CommandParameter()
        {
            Name = "";
            IsRequired = false;
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="req">必須フラグ</param>
        public Kbtter4CommandParameter(string name, bool req)
        {
            Name = name;
            IsRequired = req;
        }
    }
}
