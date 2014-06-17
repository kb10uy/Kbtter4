using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Kbtter3.Query.Grammer
{
    /// <summary>
    /// Kbtter3Queryでの値を定義します。
    /// </summary>
    public sealed class Kbtter3QueryValue
    {
        #region プロパティ
        /// <summary>
        /// 現在の値がどの型で有効か取得します。
        /// 暗黙的に変換可能な型も含まれます。
        /// </summary>
        /// <returns></returns>
        public Kbtter3QueryValueType AvailableTypes { get; private set; }

        /// <summary>
        /// 現在の値の本来の型を取得します。
        /// </summary>
        /// <returns></returns>
        public Kbtter3QueryValueType OriginalType { get; private set; }

        /// <summary>
        /// 現在の値を取得します。
        /// </summary>
        /// <returns></returns>
        public dynamic Value { get; private set; }
        #endregion

        #region コンストラクタ

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public Kbtter3QueryValue()
        {
            SetValue();
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="val">セットする値</param>
        public Kbtter3QueryValue(bool val)
        {
            SetValue(val);
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="val">セットする値</param>
        public Kbtter3QueryValue(int val)
        {
            SetValue(val);
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="val">セットする値</param>
        public Kbtter3QueryValue(string val)
        {
            SetValue(val);
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="val">セットする値</param>
        public Kbtter3QueryValue(Regex val)
        {
            SetValue(val);
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="val">セットする値</param>
        public Kbtter3QueryValue(object val)
        {
            SetValue(val);
        }

        #endregion

        #region As変換
        /// <summary>
        /// 変換可能ならば真偽型に変換します。
        /// </summary>
        /// <returns>変換された値</returns>
        public bool AsBoolean()
        {
            switch (OriginalType)
            {
                case Kbtter3QueryValueType.Boolean:
                    return (bool)Value;
                case Kbtter3QueryValueType.Number:
                case Kbtter3QueryValueType.String:
                case Kbtter3QueryValueType.Regex:
                    return Value != null;
                default:
                    throw new InvalidCastException(string.Format("現在の型{0}に対してBooleanへの変換は出来ません", OriginalType));
            }
        }

        /// <summary>
        /// 変換可能ならば数値型に変換します。
        /// </summary>
        /// <returns>変換された値</returns>
        public int AsNumber()
        {
            switch (OriginalType)
            {
                case Kbtter3QueryValueType.Boolean:
                    return Value ? 1 : 0;
                case Kbtter3QueryValueType.Number:
                    return (int)Value;
                default:
                    throw new InvalidCastException(string.Format("現在の型{0}に対してNumberへの変換は出来ません", OriginalType));
            }
        }

        /// <summary>
        /// 変換可能ならば文字列型に変換します。
        /// </summary>
        /// <returns>変換された値</returns>
        public string AsString()
        {
            switch (OriginalType)
            {
                case Kbtter3QueryValueType.Boolean:
                    return Value ? "true" : "false";
                case Kbtter3QueryValueType.Number:
                    return Value.ToString();
                case Kbtter3QueryValueType.String:
                    return Value as string;
                default:
                    throw new InvalidCastException(string.Format("現在の型{0}に対してStringへの変換は出来ません", OriginalType));
            }
        }

        /// <summary>
        /// 変換可能ならば正規表現型に変換します。
        /// </summary>
        /// <returns>変換された値</returns>
        public Regex AsRegex()
        {
            switch (OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Regex(Value.ToString());
                case Kbtter3QueryValueType.String:
                    return new Regex(Value);
                case Kbtter3QueryValueType.Regex:
                    return Value as Regex;
                default:
                    throw new InvalidCastException(string.Format("現在の型{0}に対してRegexへの変換は出来ません", OriginalType));
            }
        }

        /// <summary>
        /// 現在の値を返します。
        /// </summary>
        /// <returns>値</returns>
        public object AsUndefined()
        {
            return Value as object;
        }
        #endregion

        #region 値セット
        /// <summary>
        /// Nullをセットします。
        /// </summary>
        public void SetValue()
        {
            Value = null;
            AvailableTypes = Kbtter3QueryValueType.Null;
        }

        /// <summary>
        /// 値をセットします。
        /// </summary>
        /// <param name="val">代入する値</param>
        public void SetValue(bool val)
        {
            AvailableTypes = Kbtter3QueryValueType.Boolean | Kbtter3QueryValueType.Number | Kbtter3QueryValueType.String | Kbtter3QueryValueType.Undefined;
            OriginalType = Kbtter3QueryValueType.Boolean;
            Value = val;
        }

        /// <summary>
        /// 値をセットします。
        /// </summary>
        /// <param name="val">代入する値</param>
        public void SetValue(int val)
        {
            AvailableTypes = Kbtter3QueryValueType.Boolean | Kbtter3QueryValueType.Number | Kbtter3QueryValueType.String | Kbtter3QueryValueType.Regex | Kbtter3QueryValueType.Undefined;
            OriginalType = Kbtter3QueryValueType.Number;
            Value = val;
        }

        /// <summary>
        /// 値をセットします。
        /// </summary>
        /// <param name="val">代入する値</param>
        public void SetValue(string val)
        {
            AvailableTypes = Kbtter3QueryValueType.Boolean | Kbtter3QueryValueType.String | Kbtter3QueryValueType.Regex | Kbtter3QueryValueType.Undefined;
            OriginalType = Kbtter3QueryValueType.String;
            Value = val;
        }

        /// <summary>
        /// 値をセットします。
        /// </summary>
        /// <param name="val">代入する値</param>
        public void SetValue(Regex val)
        {
            AvailableTypes = Kbtter3QueryValueType.Boolean | Kbtter3QueryValueType.Regex | Kbtter3QueryValueType.Undefined;
            OriginalType = Kbtter3QueryValueType.Regex;
            Value = val;
        }

        /// <summary>
        /// 値をセットします。
        /// </summary>
        /// <param name="val">代入する値</param>
        public void SetValue(object val)
        {
            AvailableTypes = Kbtter3QueryValueType.Boolean | Kbtter3QueryValueType.Undefined;
            OriginalType = Kbtter3QueryValueType.Undefined;
            Value = val;
        }
        #endregion

        #region 特殊
        /// <summary>
        /// 現在の値がUndefinedに対して有効な時、
        /// 対象のオブジェクトのフィールド・プロパティを検索し、
        /// 見つかればその値を代入したオブジェクトを取得します。
        /// </summary>
        /// <param name="name">検索する名前</param>
        /// <returns>見つかれば値</returns>
        public Kbtter3QueryValue Dive(string name)
        {
            var ret = new Kbtter3QueryValue();
            var type = Value.GetType() as Type;
            var pr = type.GetProperty(name);
            var fl = type.GetField(name);
            if (pr == null && fl == null) return null;

            if (pr != null)
            {
                var val = pr.GetValue(Value as object);
                var vt = val.GetType();
                if (vt.Name == "Int32")
                {
                    ret.SetValue((int)val);
                }
                else if (vt.Name == "Boolean")
                {
                    ret.SetValue((bool)val);
                }
                else if (vt.Name == "String")
                {
                    ret.SetValue(val as string);
                }
                else if (vt.Name == "Regex")
                {
                    ret.SetValue(val as Regex);
                }
                else
                {
                    ret.SetValue(val);
                }
            }
            else
            {
                var val = pr.GetValue(Value as object);
                var vt = val.GetType();
                if (vt.Name == "Int32")
                {
                    ret.SetValue((int)val);
                }
                else if (vt.Name == "Boolean")
                {
                    ret.SetValue((bool)val);
                }
                else if (vt.Name == "String")
                {
                    ret.SetValue(val as string);
                }
                else if (vt.Name == "Regex")
                {
                    ret.SetValue(val as Regex);
                }
                else
                {
                    ret.SetValue(val);
                }
            }
            return ret;
        }
        #endregion

        #region 型変換・準型変換
        public static Kbtter3QueryValue operator +(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() + y.AsNumber());
                case Kbtter3QueryValueType.String:
                    return new Kbtter3QueryValue(x.AsString() + y.AsString());
                default:
                    throw new InvalidCastException("+演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator -(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() - y.AsNumber());
                default:
                    throw new InvalidCastException("-演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator *(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() * y.AsNumber());
                case Kbtter3QueryValueType.String:
                    if (y.AvailableTypes.HasFlag(Kbtter3QueryValueType.Number))
                    {
                        var s = x.AsString();
                        for (int i = 0; i < y.AsNumber() - 1; i++) s += x.AsString();
                        return new Kbtter3QueryValue(s);
                    }
                    else
                    {
                        throw new InvalidCastException("*演算出来ません");
                    }
                default:
                    throw new InvalidCastException("*演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator /(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() / y.AsNumber());
                default:
                    throw new InvalidCastException("/演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator %(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() % y.AsNumber());
                default:
                    throw new InvalidCastException("%演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator &(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() & y.AsNumber());
                default:
                    throw new InvalidCastException("&演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator |(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() | y.AsNumber());
                default:
                    throw new InvalidCastException("|演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator ^(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() ^ y.AsNumber());
                default:
                    throw new InvalidCastException("^演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator ==(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() == y.AsNumber());
                case Kbtter3QueryValueType.String:
                    return new Kbtter3QueryValue(x.AsString() == y.AsString());
                case Kbtter3QueryValueType.Boolean:
                    return new Kbtter3QueryValue(x.AsBoolean() == y.AsBoolean());
                default:
                    throw new InvalidCastException("==演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator !=(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() != y.AsNumber());
                case Kbtter3QueryValueType.String:
                    return new Kbtter3QueryValue(x.AsString() != y.AsString());
                case Kbtter3QueryValueType.Boolean:
                    return new Kbtter3QueryValue(x.AsBoolean() != y.AsBoolean());
                default:
                    throw new InvalidCastException("!=演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator >(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() > y.AsNumber());
                case Kbtter3QueryValueType.String:
                    return new Kbtter3QueryValue(x.AsString().CompareTo(y.AsString()) == 1 ? true : false);
                default:
                    throw new InvalidCastException(">演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator <(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() < y.AsNumber());
                case Kbtter3QueryValueType.String:
                    return new Kbtter3QueryValue(x.AsString().CompareTo(y.AsString()) == -1 ? true : false);
                default:
                    throw new InvalidCastException("<演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator >=(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() >= y.AsNumber());
                case Kbtter3QueryValueType.String:
                    return new Kbtter3QueryValue(x.AsString().CompareTo(y.AsString()) >= 0 ? true : false);
                default:
                    throw new InvalidCastException(">=演算出来ません");
            }
        }

        public static Kbtter3QueryValue operator <=(Kbtter3QueryValue x, Kbtter3QueryValue y)
        {
            switch (x.OriginalType)
            {
                case Kbtter3QueryValueType.Number:
                    return new Kbtter3QueryValue(x.AsNumber() <= y.AsNumber());
                case Kbtter3QueryValueType.String:
                    return new Kbtter3QueryValue(x.AsString().CompareTo(y.AsString()) <= 0 ? true : false);
                default:
                    throw new InvalidCastException("<=演算出来ません");
            }
        }

        public static bool operator true(Kbtter3QueryValue x)
        {
            return x.AsBoolean();
        }

        public static bool operator false(Kbtter3QueryValue x)
        {
            return x.AsBoolean();
        }

        public static explicit operator Kbtter3QueryValue(bool x)
        {
            return new Kbtter3QueryValue(x);
        }

        public override bool Equals(object obj)
        {
            var x = obj as Kbtter3QueryValue;
            if (x == null) return false;
            if (ReferenceEquals(this, x)) return true;
            return (this == x).AsBoolean();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// Kbtter3Queryでの値の型を列挙します。
    /// </summary>
    [Flags]
    public enum Kbtter3QueryValueType
    {
        /// <summary>
        /// Null型
        /// </summary>
        Null = 1,
        /// <summary>
        /// 未定義型
        /// </summary>
        Undefined = 2,
        /// <summary>
        /// 真偽型
        /// </summary>
        Boolean = 4,
        /// <summary>
        /// 数値型
        /// </summary>
        Number = 8,
        /// <summary>
        /// 文字列型
        /// </summary>
        String = 16,
        /// <summary>
        /// 正規表現型
        /// </summary>
        Regex = 32,
    }
}
