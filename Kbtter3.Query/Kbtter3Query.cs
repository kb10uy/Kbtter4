using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kbtter3.Query.Grammer;
using Irony.Parsing;

namespace Kbtter3.Query
{
    /// <summary>
    /// ダイレクトメッセージを検証するクエリを定義します。
    /// </summary>
    public class Kbtter3Query
    {
        private ParseTree Tree;

        /// <summary>
        /// セットされている変数
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Kbtter3QueryValue> Variables { get; protected set; }

        public string QueryText { get; private set; }

        /// <summary>
        /// クエリを指定して初期化します。
        /// </summary>
        /// <param name="query">クエリ</param>
        public Kbtter3Query(string query)
        {
            Kbtter3QueryGrammar g = new Kbtter3QueryGrammar();
            Parser ps = new Parser(g);
            Variables = new Dictionary<string, Kbtter3QueryValue>();

            Tree = ps.Parse(query);
            if (Tree.Root == null)
            {
                throw new InvalidOperationException(Tree.ParserMessages[0].Message);
            }
            QueryText = query;
        }

        /// <summary>
        /// 変数を消去します。
        /// </summary>
        public void ClearVariables()
        {
            Variables.Clear();
        }

        /// <summary>
        /// 変数をセットします。
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="obj">オブジェクト</param>
        public void SetVariable(string name, object obj)
        {
            Variables[name] = new Kbtter3QueryValue(obj);
        }

        /// <summary>
        /// 変数をセットします。
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="obj">オブジェクト</param>
        public void SetVariable(string name, bool obj)
        {
            Variables[name] = new Kbtter3QueryValue(obj);
        }

        /// <summary>
        /// 変数をセットします。
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="obj">オブジェクト</param>
        public void SetVariable(string name, int obj)
        {
            Variables[name] = new Kbtter3QueryValue(obj);
        }

        /// <summary>
        /// 変数をセットします。
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="obj">オブジェクト</param>
        public void SetVariable(string name, string obj)
        {
            Variables[name] = new Kbtter3QueryValue(obj);
        }

        /// <summary>
        /// 変数をセットします。
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="obj">オブジェクト</param>
        public void SetVariable(string name, Regex obj)
        {
            Variables[name] = new Kbtter3QueryValue(obj);
        }

        /// <summary>
        /// クエリを実行します。
        /// </summary>
        /// <returns>結果</returns>
        public Kbtter3QueryValue Execute()
        {
            var t = "";
            return ParseExpression(Tree.Root, out t);
        }

        #region 内部
        private Kbtter3QueryValue ParseExpression(ParseTreeNode node, out string type)
        {
            if (node.ChildNodes.Count == 0)
            {
                type = node.Term.Name;
                switch (node.Term.Name)
                {
                    //ここではIdentiferは文字列扱い
                    case "String":
                    case "Identifer":
                        return new Kbtter3QueryValue(node.Token.ValueString);
                    case "Number":
                        return new Kbtter3QueryValue((int)node.Token.Value);
                    case "Regex":
                        return new Kbtter3QueryValue(new Regex(node.Token.ValueString));
                    case "true":
                    case "false":
                        return new Kbtter3QueryValue(Convert.ToBoolean(node.Token.ValueString));
                    case "null":
                        return new Kbtter3QueryValue();
                    default:
                        throw new InvalidOperationException("対応していない値です");
                }
            }
            else if (node.ChildNodes.Count == 2)
            {
                //おそらくPostfix
                var t = "";
                var orgv = ParseExpression(node.ChildNodes[1], out t);
                switch (node.ChildNodes[0].Token.ValueString)
                {
                    case "+":
                        type = "non-term";
                        return new Kbtter3QueryValue(orgv.AsNumber());
                    case "-":
                        type = "non-term";
                        return new Kbtter3QueryValue(-orgv.AsNumber());
                    case "!":
                        type = "non-term";
                        return new Kbtter3QueryValue(orgv.AsNumber() == 0 ? 1 : 0);
                    default:
                        throw new InvalidOperationException("対応していないPostfixです");
                }
            }
            else
            {
                var t1 = "";
                var t2 = "";
                var val1 = ParseExpression(node.ChildNodes[0], out t1);
                var val2 = ParseExpression(node.ChildNodes[2], out t2);
                type = "non-term";
                if (t1 == "Identifer")
                {
                    if (!Variables.ContainsKey(val1.AsString())) throw new InvalidOperationException("変数が定義されていません");
                    val1 = Variables[val1.AsString()];
                    if (t2 == "Identifer" && node.ChildNodes[1].Token.ValueString == ".")
                    {
                        return val1.Dive(val2.AsString());
                    }
                    else
                    {
                        if (!Variables.ContainsKey(val2.AsString())) throw new InvalidOperationException("変数が定義されていません");
                    }
                }

                switch (node.ChildNodes[1].Token.ValueString)
                {
                    case ".":
                        return val1.Dive(val2.AsString());
                    case "==":
                        return val1 == val2;
                    case "!=":
                        return val1 != val2;
                    case ">":
                        return val1 > val2;
                    case "<":
                        return val1 < val2;
                    case ">=":
                        return val1 >= val2;
                    case "<=":
                        return val1 <= val2;
                    case "match":
                       return new Kbtter3QueryValue(val2.AsRegex().IsMatch(val1.AsString()));
                    case "&&":
                        return val1 && val2;
                    case "||":
                        return val1 || val2;
                    case "+":
                        return val1 + val2;
                    case "-":
                        return val1 - val2;
                    case "*":
                        return val1 * val2;
                    case "/":
                        return val1 / val2;
                    case "%":
                        return val1 % val2;
                    case "&":
                        return val1 & val2;
                    case "|":
                        return val1 | val2;
                    case "^":
                        return val1 ^ val2;
                    default:
                        throw new InvalidOperationException("対応していない演算子です");
                }
            }
        }

        #endregion
    }
}
