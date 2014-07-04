using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony;
using Irony.Parsing;
using Irony.Ast;

namespace Kbtter3.Query.Grammer
{
    /// <summary>
    /// 汎用的なクエリ
    /// </summary>
    [Language("Kbtter3Query", "1.0.0", "Kbtter3 Query")]
    public class Kbtter3QueryGrammar : Grammar
    {
        /// <summary>
        /// 使うな
        /// </summary>
        public Kbtter3QueryGrammar()
            : base()
        {
            //コメント
            var comment = new CommentTerminal("Comment", "/*", "*/");
            NonGrammarTerminals.Add(comment);

            //リテラル
            var number = new NumberLiteral("Number", NumberOptions.AllowSign | NumberOptions.AllowStartEndDot);
            var str = new StringLiteral("String", "\"");
            var regex = new RegexLiteral("Regex", '/', '\\');
            var ident = new IdentifierTerminal("Identifer");

            //非終端
            var Value = new NonTerminal("Value");
            var Term = new NonTerminal("Term");
            var Expression = new NonTerminal("Expression");
            var BinExpression = new NonTerminal("BinExpression");
            var ParExpression = new NonTerminal("ParExpression");
            var PostfixExpression = new NonTerminal("PostfixExpression");
            var Operator = new NonTerminal("Operator");

            //非終端定義
            Value.Rule = number | str | ident | regex | "null" | "true" | "false";
            Term.Rule = Value | ParExpression;
            Operator.Rule = ToTerm("==") | "!=" | ">" | "<" | ">=" | "<=" | "match" | "&&" | "||" | "+" | "-" | "*" | "/" | "%" | "&" | "|" | "^" | ".";
            BinExpression.Rule = Expression + Operator + Expression;
            PostfixExpression.Rule = (ToTerm("+") + Term) | ("-" + Term) | ("!" + Term);
            Expression.Rule = BinExpression | Term | PostfixExpression;
            ParExpression.Rule = ToTerm("(") + Expression + ")";

            RegisterOperators(10, ".");
            RegisterOperators(9, "*", "/", "%");
            RegisterOperators(8, "+", "-");
            RegisterOperators(7, ">", "<", ">=", "<=", "match");
            RegisterOperators(6, "==", "!=");
            RegisterOperators(5, "&");
            RegisterOperators(4, "^");
            RegisterOperators(3, "|");
            RegisterOperators(2, "&&");
            RegisterOperators(1, "||");

            Root = Expression;
            MarkPunctuation("(", ")");
            MarkTransient(Expression, ParExpression, Value, Operator, Term);
        }
    }
}
