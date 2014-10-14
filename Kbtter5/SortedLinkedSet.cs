using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kb10uy.Collections.Generic
{
    public class SortedLinkedSet<T, TKey> : IEnumerable<T>, ICollection<T>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private SortedNode<T, TKey> Top { get; set; }
        private Func<T, TKey> KeyFunction { get; set; }
        private int count;

        public SortedLinkedSet(Func<T, TKey> pred)
        {
            KeyFunction = pred;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Top == null) yield break;
            var c = Top;
            do
            {
                yield return c.Value;
            } while ((c = c.Next) != null);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (Top == null) yield break;
            var c = Top;
            do
            {
                yield return c.Value;
            } while ((c = c.Next) != null);
        }

        public void Add(T item)
        {
            var obj = new SortedNode<T, TKey>(item, KeyFunction(item));
            count++;
            if (Top == null)
            {
                Top = obj;
                obj.Top = obj;
                obj.Last = obj;
                return;
            }

            var target = Top;
            while (true)
            {
                switch (Math.Sign(target.Key.CompareTo(obj.Key)))
                {
                    case -1:
                        //targetより後ろ側に新規キー
                        if (target.Last.Next == null)
                        {
                            //最後尾
                            obj.Top = obj;
                            obj.Last = obj;
                            obj.Previous = target.Last;
                            target.Last.Next = obj;
                            return;
                        }
                        target = target.Last.Next;
                        continue;
                    case 0:
                        //target内
                        if (target.Top == target.Last)
                        {
                            //targetのキーが1つ
                            obj.Previous = target;
                            obj.Top = target;
                            obj.Last = obj;
                            target.Next = obj;
                            target.Last = obj;
                            return;
                        }
                        //target.Topの次
                        obj.Next = target.Next;
                        target.Next = obj;
                        obj.Previous = target;
                        break;
                    case 1:
                        //targetより前側に新規キー
                        obj.Top = obj;
                        obj.Last = obj;
                        if (target == Top)
                        {
                            //最前列
                            Top.Previous = obj;
                            obj.Next = Top;
                            Top = obj;
                            return;
                        }
                        target.Previous.Next = obj;
                        obj.Previous = target.Previous;
                        obj.Next = target;
                        break;
                }
            }

        }

        public void AddRange(IEnumerable<T> items, bool checkKeyEquality)
        {
            if (items.FirstOrDefault() == null) return;
            if (checkKeyEquality && items.Any(p => !KeyFunction(items.First()).Equals(KeyFunction(p))))
            {
                throw new InvalidOperationException("指定されたシーケンス内でのキー値が一致しません");
            }

            var ai = items.Select(p => new SortedNode<T, TKey>(p, KeyFunction(p))).ToArray();
            var at = ai[0];
            var al = ai[ai.Length - 1];
            for (int i = 1; i < ai.Length - 1; i++)
            {
                ai[i].Previous = ai[i - 1];
                ai[i].Next = ai[i + 1];
            }
            ai[ai.Length - 1].Previous = ai[ai.Length - 2];

            if (Top == null)
            {
                Top = at;
                at.Top = at;
                at.Last = al;
                al.Top = at;
                al.Last = al;
                return;
            }

            var target = Top;
            while (true)
            {
                switch (Math.Sign(target.Key.CompareTo(at.Key)))
                {
                    case -1:
                        //targetより後ろ側に新規キー
                        if (target.Last.Next == null)
                        {
                            //最後尾
                            at.Top = at;
                            at.Last = al;
                            al.Top = at;
                            al.Last = al;
                            at.Previous = target.Last;
                            target.Last.Next = at;
                            return;
                        }
                        target = target.Last.Next;
                        continue;
                    case 0:
                        //target内
                        if (target.Top == target.Last)
                        {
                            //targetのキーが1つ
                            at.Previous = target;
                            at.Top = target;
                            at.Last = al;
                            al.Top = target;
                            al.Last = al;
                            target.Next = at;
                            target.Last = al;
                            return;
                        }
                        //target.Topの次
                        al.Next = target.Next;
                        target.Next = at;
                        at.Previous = target;
                        break;
                    case 1:
                        //targetより前側に新規キー
                        at.Top = at;
                        at.Last = al;
                        al.Top = at;
                        al.Last = al;
                        if (target == Top)
                        {
                            //最前列
                            Top.Previous = al;
                            al.Next = Top;
                            Top = at;
                            return;
                        }
                        target.Previous.Next = at;
                        at.Previous = target.Previous;
                        al.Next = target;
                        break;
                }
            }

        }

        public void Clear()
        {
            Top = null;
            count = 0;
        }

        public bool Contains(T item)
        {
            if (Top == null) return false;
            var c = Top;
            do
            {
                if (c.Value.Equals(item)) return true;
            } while ((c = c.Next) != null);
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return count = 0; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            if (Top == null) return false;
            var c = Top;
            do
            {
                if (c.Value.Equals(item))
                {
                    c.Previous.Next = c.Next;
                    c.Next.Previous = c.Previous;
                    if (c == c.Top)
                    {
                        c.Next.Top = c.Next;
                        c.Next.Last = c.Last;
                    }
                    if (c == c.Last)
                    {
                        c.Previous.Last = c.Previous;
                        c.Previous.Top = c.Top;
                    }
                    count--;
                    return true;
                }
            } while ((c = c.Next) != null);
            return false;
        }

        public void RemoveAll(Predicate<T> cond)
        {
            if (Top == null) return;
            var c = Top;
            do
            {
                if (cond(c.Value))
                {
                    c.Previous.Next = c.Next;
                    c.Next.Previous = c.Previous;
                    if (c == c.Top)
                    {
                        c.Next.Top = c.Next;
                        c.Next.Last = c.Last;
                    }
                    if (c == c.Last)
                    {
                        c.Previous.Last = c.Previous;
                        c.Previous.Top = c.Top;
                    }
                    count--;
                }
            } while ((c = c.Next) != null);
        }
    }

    class SortedNode<T, TKey>
        where TKey : IComparable<TKey>
    {
        public T Value { get; private set; }
        public TKey Key { get; private set; }

        public SortedNode<T, TKey> Previous { get; set; }
        public SortedNode<T, TKey> Next { get; set; }
        public SortedNode<T, TKey> Top { get; set; }
        public SortedNode<T, TKey> Last { get; set; }

        public SortedNode(T obj, TKey key)
        {
            Value = obj;
            Key = key;
        }
    }
}
