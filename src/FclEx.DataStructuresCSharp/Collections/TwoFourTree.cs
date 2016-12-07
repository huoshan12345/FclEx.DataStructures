using FclEx.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FclEx.Collections
{
    public class TwoFourTree<TKey, TValue> : IKeyValueCollection<TKey, TValue>
    {
        private readonly TwoFourTreeNode _root = new TwoFourTreeNode(true);
        private int _count = 0;
        private int _version = 0;
        private readonly IComparer<TKey> _comparer;
        private readonly EqualityComparer<TValue> _valueComparer;

        public TwoFourTree(IComparer<TKey> comparer = null)
        {
            _comparer = comparer ?? Comparer<TKey>.Default;
            _valueComparer = EqualityComparer<TValue>.Default;
        }

        public TwoFourTree(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer = null) : this(comparer)
        {
            Contract.Requires<ArgumentNullException>(dictionary != null);
            foreach (var item in dictionary)
            {
                Add(item);
            }
        }

        public int Count => _count;
        public bool IsReadOnly => false;

        public bool ContainsValue(TValue value) => Traverse().Any(item => _valueComparer.Equals(item.Value, value));

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Tuple<TwoFourTreeNode, int> FindItem(TKey key, bool checkValue = false, TValue value = default(TValue))
        {
            Contract.Requires<ArgumentNullException>(key != null);
            var node = _root;
            while (node != null)
            {
                var index = 0;
                while (index < node.KeyNum)
                {
                    var cmp = _comparer.Compare(node.Items[index].Key, key);
                    if (cmp == 0) return (checkValue && !_valueComparer.Equals(node.Items[index].Value, value)) ? null : Tuple.Create(node, index);
                    else if (cmp > 0) break;
                    else index++;
                }
                if (node.IsLeafNode) return null;
                node = node.Children[index];
            }
            return null;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Contract.Requires<ArgumentNullException>(item.Key != null);
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => FindItem(item.Key, true, item.Value) != null;

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Contract.Requires<ArgumentNullException>(array != null);
            Contract.Requires<ArgumentOutOfRangeException>((uint)arrayIndex <= (uint)array.Length);
            Contract.Requires<ArgumentException>(array.Length >= _count + arrayIndex);
            foreach (var item in Traverse())
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        public bool ContainsKey(TKey key) => FindItem(key) != null;

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = FindItem(key);
            if (result == null)
            {
                value = default(TValue);
                return false;
            }
            value = result.Item1.Items[result.Item2].Value;
            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                var result = FindItem(key);
                Contract.Requires<KeyNotFoundException>(result != null);
                return result.Item1.Items[result.Item2].Value;
            }
            set
            {
                var result = FindItem(key);
                if (result == null) Add(key, value);
                else result.Item1.Items[result.Item2] = new KeyValuePair<TKey, TValue>(key, value);
                _version++;
            }
        }

        public ICollection<TKey> Keys => new BaseKeyCollection<TKey, TValue>(this);

        public ICollection<TValue> Values => new BaseValueCollection<TKey, TValue>(this);

        private IEnumerable<KeyValuePair<TKey, TValue>> Traverse() => _root.InOrderTraverse();

        public void Clear()
        {
            _root.Clear();
            _count = 0;
            _version++;
        }

        private class TwoFourTreeNode
        {
            private const int MinDegree = 2;
            private const int MaxDegree = 4;
            private int MinKeyNum => MinDegree - 1;
            private int MaxKeyNum => MaxDegree - 1;

            internal int KeyNum { get; private set; }
            internal KeyValuePair<TKey, TValue>[] Items { get; private set; }
            internal TwoFourTreeNode[] Children { get; private set; }
            private TwoFourTreeNode Parent { get; set; }

            internal bool IsLeafNode => Children == null;
            private const bool LowMemoryUsage = false; // low mem => low speed, high mem => high speed

            internal TwoFourTreeNode(bool isLeaf)
            {
                KeyNum = 0;
                var keyCapacity = LowMemoryUsage ? MinKeyNum : MaxKeyNum;
                Items = new KeyValuePair<TKey, TValue>[keyCapacity]; // t-1 ~ 2t-1
                if (!isLeaf) Children = new TwoFourTreeNode[keyCapacity + 1];
            }

            private void Invalidate()
            {
                KeyNum = 0;
                if (Children != null) Array.Clear(Children, 0, Children.Length);
                if (Items != null) Array.Clear(Items, 0, Items.Length);
                Children = null;
                Items = null;
                Parent = null;
            }

            private int GetChildIndex()
            {
                return Parent?.Children.IndexOf(this) ?? -1;
            }

            internal IEnumerable<KeyValuePair<TKey, TValue>> InOrderTraverse()
            {
                for (var i = 0; i < KeyNum; i++)
                {
                    if (!IsLeafNode)
                    {
                        Contract.Ensures(Children[i] != null);
                        foreach (var n in Children[i].InOrderTraverse())
                        {
                            yield return n;
                        }
                    }

                    yield return Items[i];
                }
                if (!IsLeafNode)
                {
                    Contract.Ensures(Children[KeyNum] != null);
                    foreach (var n in Children[KeyNum].InOrderTraverse())
                    {
                        yield return n;
                    }
                }
            }

            internal void Clear()
            {
                var queue = new Queue<TwoFourTreeNode>();
                queue.Enqueue(this);
                while (queue.Count != 0)
                {
                    var item = queue.Dequeue();
                    if (!item.IsLeafNode)
                    {
                        foreach (var t in item.Children)
                        {
                            Contract.Ensures(t != null);
                            queue.Enqueue(t);
                        }
                    }
                    item.Invalidate();
                }
            }
        }

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;
            private readonly TwoFourTree<TKey, TValue> _tree;
            private readonly int _version;
            private int _index;

            internal Enumerator(TwoFourTree<TKey, TValue> tree)
            {
                _tree = tree;
                _enumerator = tree.Traverse().GetEnumerator();
                _version = tree._version;
                _index = -1;
            }

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext()
            {
                Contract.Requires<InvalidOperationException>(_version != _tree._version);
                if (!_enumerator.MoveNext())
                {
                    _index = _tree.Count;
                    return false;
                }
                ++_index;
                return true;
            }

            public void Reset()
            {
                Contract.Requires<InvalidOperationException>(_version != _tree._version);
                _enumerator = _tree.Traverse().GetEnumerator();
                _index = -1;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    Contract.Requires<InvalidOperationException>(_index >= 0 && _index < _tree.Count);
                    return _enumerator.Current;
                }
            }

            object IEnumerator.Current => Current;
        }
    }
}
