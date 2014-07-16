using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using Irony.Ast;

namespace Kbtter4.Tenko
{
    /// <summary>
    /// Kbtter4用コマンドラインパーサ
    /// </summary>
    [Language("Kbtter4コマンドライン", "1.0.0", "Kbtter4用コマンドラインパーサ")]
    public class Kbtter4CommandlineGrammar : Grammar
    {
        /// <summary>
        /// 使うな
        /// </summary>
        public Kbtter4CommandlineGrammar()
            : base()
        {
            var Number = new NumberLiteral("Number");
            Number.DefaultIntTypes = new[] { TypeCode.Int32, TypeCode.Int64 };
            Number.DefaultFloatType = TypeCode.Double;
            var String = new StringLiteral("String", "\"", StringOptions.AllowsAllEscapes);
            var Identifer = new IdentifierTerminal("Identifer");

            var Value = new NonTerminal("Value");
            var Parameter = new NonTerminal("Parameter");
            var Parameters = new NonTerminal("Parameters");
            var Command = new NonTerminal("Command");

            Value.Rule = Number | String | "true" | "false";
            Parameter.Rule = Identifer + "=>" + Value;
            Parameters.Rule = MakeStarRule(Parameters, ToTerm(","), Parameter);
            Command.Rule = Identifer + Parameters;

            Root = Command;

            MarkTransient(Parameters, Value);
            MarkPunctuation("=>");
        }
    }

    /// <summary>
    /// Kbtter4用コマンドライン構文解析のサポート
    /// </summary>
    public static class Kbtter4CommandlineParser
    {
        /// <summary>
        /// 構文解析器
        /// </summary>
        public static Grammar Grammar { get; private set; }

        /// <summary>
        /// パーサ
        /// </summary>
        public static Parser Parser { get; private set; }


        static Kbtter4CommandlineParser()
        {
            Grammar = new Kbtter4CommandlineGrammar();
            Parser = new Parser(Grammar);
        }

        /// <summary>
        /// 解析して結果を返します。
        /// </summary>
        /// <param name="cmdline">コマンドライン文字列</param>
        /// <returns>結果</returns>
        public static Kbtter4CommandlineParseResult Parse(string cmdline)
        {
            var tree = Parser.Parse(cmdline).Root;
            if (tree == null) return null;

            var ret = new Kbtter4CommandlineParseResult();
            ret.Name = tree.ChildNodes[0].Token.ValueString;

            if (tree.ChildNodes.Count == 1) return ret;
            foreach (var i in tree.ChildNodes[1].ChildNodes)
            {
                if (i.ChildNodes[1].Term.Name == "true" || i.ChildNodes[1].Term.Name == "false")
                {
                    ret.Parameters[i.ChildNodes[0].Token.ValueString] = Convert.ToBoolean(i.ChildNodes[1].Token.ValueString);
                }
                else
                {
                    ret.Parameters[i.ChildNodes[0].Token.ValueString] = i.ChildNodes[1].Token.Value;
                }
            }
            return ret;
        }
    }

    /// <summary>
    /// 構文解析の結果
    /// </summary>
    public class Kbtter4CommandlineParseResult
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string Name { get; internal set; }

        public IDictionary<string, object> Parameters { get; private set; }

        public Kbtter4CommandlineParseResult()
        {
            Name = "";
            Parameters = new Dictionary<string, object>();
        }
    }
}
