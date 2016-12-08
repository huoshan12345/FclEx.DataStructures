using FclEx.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FclEx.Collections
{
    public class TwoFourTree<TKey, TValue> : IKeyValueCollection<TKey, TValue>
    {
        private TwoFourTreeNode _root = new TwoFourTreeNode(true);
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
            if(dictionary == null) throw new ArgumentNullException(nameof(dictionary));
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
            Debug.Assert(key != null);
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
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
            /*
                 To insert a value, we start at the root of the 2–3–4 tree:-

                1.If the current node is a 4-node:
                    Remove and save the middle value to get a 3-node.
                    Split the remaining 3-node up into a pair of 2-nodes (the now missing middle value is handled in the next step).
                    If this is the root node (which thus has no parent):
                        the middle value becomes the new root 2-node and the tree height increases by 1. Ascend into the root.
                    Otherwise, push the middle value up into the parent node. Ascend into the parent node.
                2.Find the child whose interval contains the value to be inserted.
                3.If that child is a leaf, insert the value into the child node and finish.
                    Otherwise, descend into the child and repeat from step 1.[3][4]             
            */

            var node = _root;
            while (true)
            {
                Debug.Assert(node != null);
                if (node.IsKeyFull)
                {
                    var midKey = node.Items[1];
                    Split(node);
                    node = node.Parent; // Ascend into the parent node.
                }
                var index = 0;
                while (index < node.KeyNum)
                {
                    var cmp = _comparer.Compare(item.Key, node.Items[index].Key);
                    if (cmp == 0) throw new ArgumentException($"An item with the same key has already been added. Key: {item.Key}");
                    if (cmp < 0) break;
                    index++;
                }
                if (node.IsLeafNode)
                {
                    node.InsertItem(index, item);
                    _count++;
                    _version++;
                    return;
                }
                else node = node.Children[index];
            }
        }

        private void Split(TwoFourTreeNode node)
        {
            var parent = node.Parent;
            Debug.Assert(node.IsKeyFull);
            var midKey = node.Items[1];
            var newNode = node.Split();
            if (node == _root)
            {
                Debug.Assert(parent == null);
                var newRoot = new TwoFourTreeNode(false);
                newRoot.InsertItem(0, midKey);
                newRoot.InsertChild(0, node);
                newRoot.InsertChild(1, newNode);
                _root = newRoot;
            }
            else
            {
                Debug.Assert(parent != null);
                var nodeIndex = node.GetChildIndex();
                Debug.Assert(parent.Children[nodeIndex] == node);
                parent.InsertItem(nodeIndex, midKey);
                parent.InsertChild(nodeIndex + 1, newNode);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => FindItem(item.Key, true, item.Value) != null;

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _count + arrayIndex) throw new ArgumentException(nameof(array));
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
                if (result == null) throw new KeyNotFoundException();
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
            _root = new TwoFourTreeNode(true);
            _count = 0;
            _version++;
        }

        private class TwoFourTreeNode
        {
            private const int MinDegree = 2;
            private const int MaxDegree = 4;
            private const int MinKeyNum = MinDegree - 1;
            private const int MaxKeyNum = MaxDegree - 1;

            private readonly KeyValuePair<TKey, TValue>[] _items;
            private readonly TwoFourTreeNode[] _children;

            internal int KeyNum { get; private set; }
            public KeyValuePair<TKey, TValue>[] Items => _items;
            public TwoFourTreeNode[] Children => _children;
            public TwoFourTreeNode Parent { get; private set; }

            internal bool IsLeafNode => _children == null;
            // private const bool LowMemoryUsage = false; // low mem => low speed, high mem => high speed

            internal TwoFourTreeNode(bool isLeaf)
            {
                KeyNum = 0;
                _items = new KeyValuePair<TKey, TValue>[MaxKeyNum];
                if (!isLeaf) _children = new TwoFourTreeNode[MaxKeyNum + 1];
            }

            private void Invalidate()
            {
                KeyNum = 0;
                _children.Clear();
                _items.Clear();
                Parent = null;
            }

            public int GetChildIndex()
            {
                return Parent.Children.IndexOf(this);
            }

            internal IEnumerable<KeyValuePair<TKey, TValue>> InOrderTraverse()
            {
                for (var i = 0; i < KeyNum; i++)
                {
                    if (!IsLeafNode)
                    {
                        Debug.Assert(Children[i] != null);
                        foreach (var n in Children[i].InOrderTraverse())
                        {
                            yield return n;
                        }
                    }

                    yield return Items[i];
                }
                if (!IsLeafNode)
                {
                    Debug.Assert(Children[KeyNum] != null);
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
                        for (var i = 0; i < item.KeyNum; i++)
                        {
                            Debug.Assert(item.Children[i] != null);
                            queue.Enqueue(item.Children[i]);
                        }
                    }
                    item.Invalidate();
                }
            }

            public bool IsKeyFull => KeyNum == MaxKeyNum;

            public TwoFourTreeNode Split()
            {
                Debug.Assert(IsKeyFull);

                var node = new TwoFourTreeNode(IsLeafNode)
                {
                    _items = { [0] = _items[MaxKeyNum - 1] },
                    KeyNum = 1,
                    Parent = Parent
                };
#if DEBUG
                Array.Clear(_items, 1, MaxKeyNum - 1);
#endif

                if (!IsLeafNode)
                {
                    Array.Copy(_children, MaxKeyNum - 1, node._children, 0, 2);
                    for (var i = 0; i <= node.KeyNum; i++)
                    {
                        node._children[i].Parent = node;
                    }
#if DEBUG
                    Array.Clear(_children, MaxKeyNum - 1, 2);
#endif
                }

                KeyNum = 1;
                return node;
            }



            public void InsertChild(int index, TwoFourTreeNode node)
            {
                Debug.Assert(!IsLeafNode);
                Debug.Assert(index >= 0 && index <= KeyNum);
                var num = KeyNum - index;
                if (num > 0)
                {
                    Array.Copy(_children, index, _children, index + 1, num);
                }
                _children[index] = node;
                node.Parent = this;
            }

            public void InsertItem(int index, KeyValuePair<TKey, TValue> item)
            {
                Debug.Assert(!IsKeyFull);
                Debug.Assert(index >= 0 && index <= KeyNum);
                var num = KeyNum - index;
                if (num > 0)
                {
                    Array.Copy(_items, index, _items, index + 1, num);
                }
                _items[index] = item;
                KeyNum++;
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
                if (_version != _tree._version) throw new InvalidOperationException();
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
                if (_version != _tree._version) throw new InvalidOperationException();
                _enumerator = _tree.Traverse().GetEnumerator();
                _index = -1;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (_index < 0 || (_index >= _tree.Count)) throw new InvalidOperationException();
                    return _enumerator.Current;
                }
            }

            object IEnumerator.Current => Current;
        }
    }
}
