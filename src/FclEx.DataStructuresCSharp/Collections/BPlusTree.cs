using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FclEx.Collections
{
    public class BPlusTree<TKey, TValue> : IKeyValueCollection<TKey, TValue>
    {
        private const int DefaultMinDegree = 2;
        public int MinDegree { get; } // t
        private int MaxDegree => 2 * MinDegree;
        private int MinKeyNum => MinDegree;
        private int MaxKeyNum => MaxDegree;

        private BPlusTreeNode _root;
        private BPlusTreeNode _firstLeafNode;
        private int _level;
        private int _count;
        private int _version;
        private readonly IComparer<TKey> _comparer;
        private readonly EqualityComparer<TValue> _valueComparer;

        public BPlusTree(int minDegree, IComparer<TKey> comparer)
        {
            if (minDegree < 2) throw new ArgumentOutOfRangeException(nameof(minDegree), "min degree cannot be less than 2");

            MinDegree = minDegree;
            _level = 0;
            _root = null;
            _count = 0;
            _version = 0;
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _valueComparer = EqualityComparer<TValue>.Default;
        }

        public BPlusTree(int minDegree = DefaultMinDegree) : this(minDegree, Comparer<TKey>.Default) { }

        public BPlusTree(IComparer<TKey> comparer) : this(DefaultMinDegree, comparer) { }

        public BPlusTree(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer = null)
            : this(DefaultMinDegree, comparer ?? Comparer<TKey>.Default)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            foreach (var item in dictionary)
            {
                Add(item);
            }
        }

        private static void Clear(BPlusTreeNode root)
        {
            if (root == null) return;
            var queue = new Queue<BPlusTreeNode>();
            queue.Enqueue(root);
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                if (item.Children != null && item.Children.Length != 0)
                {
                    foreach (var t in item.Children)
                    {
                        if (t != null) queue.Enqueue(t);
                    }
                }
                item.Invalidate();
            }
        }

        public void Clear()
        {
            Clear(_root);
            _firstLeafNode = null;
            _root = null;
            _count = 0;
            _level = 0;
            _version++;
        }

        private Tuple<BPlusTreeNode, int> SearchItem(TKey key, bool checkValue = false, TValue value = default(TValue))
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var node = _root;
            while (node != null)
            {
                var result = node.FindLastOfLessOrEqual(key);
                if (result.Item1)
                {
                    var leaf = node.IsLeafNode ? Tuple.Create(node, result.Item2)
                        : Tuple.Create(node.Children[result.Item2].GetMinLeafNode(), 0);
                    return (checkValue && !_valueComparer.Equals(leaf.Item1.Values[leaf.Item2], value)) ? null : leaf;
                }
                if (result.Item2 < 0 || node.IsLeafNode) return null;
                else node = node.Children[result.Item2];
            }
            return null;
        }

        private Tuple<BPlusTreeNode, int> FindLeafNodeToInsert(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var node = _root;
            while (true)
            {
                var result = node.FindLastOfLessOrEqual(key);
                if (result.Item1) throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
                if (result.Item2 < 0) return Tuple.Create(node.GetMinLeafNode(), 0);
                if (node.IsLeafNode) return Tuple.Create(node, result.Item2 + 1);
                else node = node.Children[result.Item2];
            }
        }

        // For testing
        //public IEnumerable<List<TKey[]>> ToLayerItems()
        //{
        //    if (_root == null) yield break;
        //    var queue = new Queue<List<BPlusTreeNode>>();
        //    queue.Enqueue(new List<BPlusTreeNode> { _root });
        //    while (queue.Count != 0)
        //    {
        //        var nodes = queue.Dequeue();
        //        var subNodes = new List<BPlusTreeNode>();
        //        var result = new List<TKey[]>();
        //        foreach (var node in nodes)
        //        {
        //            result.Add(node.Keys.Take(node.KeyNum).ToArray());
        //            if (node.IsLeafNode) continue;
        //            subNodes.AddRange(node.Children.Take(node.KeyNum));
        //        }
        //        if (subNodes.Count != 0) queue.Enqueue(subNodes);
        //        yield return result;
        //    }
        //}

        //private IEnumerable<BPlusTreeNode> NodeLayerTraverse()
        //{
        //    if (_root == null) yield break;
        //    var queue = new Queue<BPlusTreeNode>();
        //    queue.Enqueue(_root);
        //    while (queue.Count != 0)
        //    {
        //        var node = queue.Dequeue();
        //        yield return node;
        //        if (!node.IsLeafNode)
        //        {
        //            foreach (var child in node.Children.Take(node.KeyNum))
        //            {
        //                queue.Enqueue(child);
        //            }
        //        }
        //    }
        //}

        //private IEnumerable<BPlusTreeNode> GetErrorNode() => NodeLayerTraverse().Where(node =>
        //{
        //    // 取异或
        //    // if (item == _root ^ item.Parent == null) return true;
        //    if (node != _root && (node.Parent == null || node.KeyNum < MinKeyNum || node.KeyNum >= MaxKeyNum)) return true;
        //    if (node == _root && (node.Parent != null || (!node.IsLeafNode && node.KeyNum < 2))) return true;

        //    if (node.Keys == null) return true;
        //    if (node.IsLeafNode && (node.Values == null || node.Children != null)) return true;
        //    if (!node.IsLeafNode && (node.Values != null || node.Children == null)) return true;

        //    if (!node.IsLeafNode)
        //    {
        //        if (node.Children.Count(child => child != null) != node.KeyNum) return true;
        //        if (node.Children.Take(node.KeyNum).Any(child => child == null)) return true;
        //    }
        //    for (var i = 0; i < node.KeyNum - 1; i++)
        //    {
        //        if (_less.Compare(node.Keys[i], node.Keys[i + 1]) >= 0) return true;
        //    }

        //    if (node != _root)
        //    {
        //        if (node.Parent.Children[node.ChildIndex] != node) return true;
        //    }

        //    var p = node;
        //    while (p != _root)
        //    {
        //        var parent = p.Parent;
        //        if (_less.Compare(parent.Keys[p.ChildIndex], p.Keys[0]) != 0) return true;
        //        p = p.Parent;
        //    }

        //    return false;
        //});

        //private bool ErrorNodeExists() => GetErrorNode().Any();

        public bool ContainsValue(TValue value) => Traverse().Any(item => _valueComparer.Equals(item.Value, value));

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<TKey, TValue> item) => SearchItem(item.Key, true, item.Value) != null;

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
            var result = SearchItem(item.Key, true, item.Value);
            if (result == null) return false;
            RemoveItem(result.Item1, result.Item2);
            return true;
        }

        private void RemoveItem(BPlusTreeNode node, int index)
        {
            Debug.Assert(node.IsLeafNode);
            node.RemoveKeyValue(index);
            // update the index of a node in its parent.
            if (index == 0 && node != _root) UpdateIndex(node);
            if (node != _root && node.KeyNum < MinKeyNum) RebalanceForDeletion(node);
            --_count;
            ++_version;
        }

        private void RebalanceForDeletion(BPlusTreeNode node)
        {
            Debug.Assert(node != null);
            Debug.Assert(node != _root && node.KeyNum < MinKeyNum, $"{nameof(node)} does not need to rebalance");
            var parent = node.Parent;
            Debug.Assert(parent != null);
            var childIndex = node.ChildIndex;
            Debug.Assert(childIndex >= 0 && parent.Children[childIndex] == node, "pointers between child and parent are not correct");

            var leftSiblingIndex = childIndex - 1;
            var rightSiblingIndex = childIndex + 1;
            // case 1: the deficient node's right sibling exists and has more than the minimum number of elements, then rotate left
            if (rightSiblingIndex < parent.KeyNum && parent.Children[rightSiblingIndex].KeyNum > MinKeyNum)
            {
                var sibling = parent.Children[rightSiblingIndex];
                var borrowKey = sibling.Keys[0];
                // remove the first element of the right sibling
                if (sibling.IsLeafNode)
                {
                    var borrowValue = sibling.Values[0];
                    node.InsertKeyValue(borrowKey, borrowValue, node.KeyNum);
                    sibling.RemoveKeyValue(0);
                }
                else
                {
                    var borrowChild = sibling.Children[0];
                    node.InsertKeyChild(borrowKey, borrowChild, node.KeyNum);
                    sibling.RemoveKeyChild(0);
                }
                parent.Keys[rightSiblingIndex] = sibling.Keys[0];
            }
            // case 2: the deficient node's left sibling exists and has more than the minimum number of elements, then rotate right
            else if (leftSiblingIndex >= 0 && parent.Children[leftSiblingIndex].KeyNum > MinKeyNum)
            {
                var sibling = parent.Children[leftSiblingIndex];
                var borrowKey = sibling.Keys[sibling.KeyNum - 1];
                // remove the last element of the left sibling
                if (sibling.IsLeafNode)
                {
                    var borrowValue = sibling.Values[sibling.KeyNum - 1];
                    node.InsertKeyValue(borrowKey, borrowValue, 0);
                    sibling.RemoveKeyValue(sibling.KeyNum - 1);
                }
                else
                {
                    var borrowChild = sibling.Children[sibling.KeyNum - 1];
                    node.InsertKeyChild(borrowKey, borrowChild, 0);
                    sibling.RemoveKeyChild(sibling.KeyNum - 1);
                }
                parent.Keys[childIndex] = node.Keys[0];
            }
            // case 3: if both immediate siblings have only the minimum number of elements, then merge with a sibling sandwiching their separator taken off from their parent
            // case 3-a: the deficient node's right sibling exists
            else if (rightSiblingIndex < parent.KeyNum) MergeNodes(node);
            // case 3-b: the deficient node's left sibling exists
            else if (leftSiblingIndex >= 0) MergeNodes(parent.Children[leftSiblingIndex]);
            else
            {
                Debug.Assert(false, "cannot reach here!");
            }
        }

        private void MergeNodes(BPlusTreeNode node)
        {
            Debug.Assert(node != null);
            var parent = node.Parent;
            var right = node.MergeWithRight();
            var rightIndex = right.ChildIndex;
            right.Invalidate();
            parent.RemoveKeyChild(rightIndex);
            if (ReferenceEquals(parent, _root) && parent.KeyNum < 2)
            {
                _root = node;
                node.Parent = null;
                --_level;
            }
            else if (parent != _root && parent.KeyNum < MinKeyNum) RebalanceForDeletion(parent);
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => new BaseKeyCollection<TKey, TValue>(this);

        public ICollection<TValue> Values => new BaseValueCollection<TKey, TValue>(this);

        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_root == null)
            {
                _root = new BPlusTreeNode(this, true);
                _firstLeafNode = _root;
            }
            var leaf = FindLeafNodeToInsert(key);
            var leafNode = leaf.Item1;
            var index = leaf.Item2;
            leafNode.InsertKeyValue(key, value, index);
            if (leafNode.IsKeyFull) SplitNode(leafNode);

            // update the index of a node in its parent.
            if (index == 0 && leafNode != _root) UpdateIndex(leafNode);
            _count++;
            _version++;
        }

        private void UpdateIndex(BPlusTreeNode node)
        {
            Debug.Assert(node.IsLeafNode);
            var p = node;
            while (p != _root)
            {
                var parent = p.Parent;
                parent.Keys[p.ChildIndex] = p.Keys[0];
                p = p.Parent;
            }
        }

        private void SplitNode(BPlusTreeNode node)
        {
            Debug.Assert(node != null);
            var slibing = node.Split();
            if (node == _root)
            {
                var newRoot = new BPlusTreeNode(this, false);
                newRoot.InsertKeyChild(_root.Keys[0], _root, 0);
                newRoot.InsertKeyChild(slibing.Keys[0], slibing, 1);
                _root = newRoot;
                ++_level;
            }
            else
            {
                var parent = node.Parent;
                Debug.Assert(parent != null);
                Debug.Assert(node.ChildIndex >= 0 && node.ChildIndex < parent.KeyNum);
                parent.InsertKeyChild(slibing.Keys[0], slibing, node.ChildIndex + 1);
                if (parent.IsKeyFull) SplitNode(parent);
            }
        }

        public bool ContainsKey(TKey key) => SearchItem(key) != null;

        public bool Remove(TKey key)
        {
            var result = SearchItem(key);
            if (result == null) return false;
            RemoveItem(result.Item1, result.Item2);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = SearchItem(key);
            if (result == null)
            {
                value = default(TValue);
                return false;
            }
            value = result.Item1.Values[result.Item2];
            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                var result = SearchItem(key);
                if (result == null) throw new KeyNotFoundException();
                return result.Item1.Values[result.Item2];
            }

            set
            {
                var result = SearchItem(key);
                if (result == null) Add(key, value);
                else result.Item1.Values[result.Item2] = value;
                _version++;
            }
        }

        private IEnumerable<KeyValuePair<TKey, TValue>> Traverse()
        {
            var p = _firstLeafNode;
            while (p != null)
            {
                Debug.Assert(p.IsLeafNode);
                for (var i = 0; i < p.KeyNum; i++)
                {
                    yield return new KeyValuePair<TKey, TValue>(p.Keys[i], p.Values[i]);
                }
                p = p.Next;
            }
        }

        private class BPlusTreeNode
        {
            private BPlusTree<TKey, TValue> _tree;
            private TKey[] _keys;
            private TValue[] _values;
            private BPlusTreeNode[] _children;

            public int KeyNum { get; private set; } // 关键字个数, n
            public bool IsKeyFull => KeyNum == _tree.MaxKeyNum;
            public bool IsLeafNode => _values != null; 
            public TKey[] Keys => _keys;
            public TValue[] Values => _values;
            public BPlusTreeNode[] Children => _children;
            public BPlusTreeNode Parent { get; set; } = null;
            public BPlusTreeNode Next { get; private set; } = null;
            public int ChildIndex { get; private set; } = -1;

            public BPlusTreeNode(BPlusTree<TKey, TValue> tree, bool isLeaf)
            {
                _tree = tree;
                KeyNum = 0;
                _keys = new TKey[tree.MinKeyNum];
                _children = isLeaf ? null : new BPlusTreeNode[tree.MinKeyNum];
                _values = !isLeaf ? null : new TValue[tree.MinKeyNum];
            }

            public void InsertKeyValue(TKey key, TValue value, int index)
            {
                Debug.Assert(key != null);
                Debug.Assert(index >= 0 && index <= KeyNum);
                Debug.Assert(IsLeafNode);
                Debug.Assert(KeyNum + 1 <= _tree.MaxKeyNum);
                if (KeyNum + 1 > _tree.MinKeyNum)
                {
                    Array.Resize(ref _keys, _tree.MaxKeyNum);
                    Array.Resize(ref _values, _tree.MaxKeyNum);
                }
                if (index < KeyNum)
                {
                    Array.Copy(_keys, index, _keys, index + 1, KeyNum - index);
                    Array.Copy(_values, index, _values, index + 1, KeyNum - index);
                }
                _keys[index] = key;
                _values[index] = value;
                ++KeyNum;
            }

            public void RemoveKeyValue(int index)
            {
                Debug.Assert(index >= 0 && index < KeyNum);
                Debug.Assert(IsLeafNode);
                Debug.Assert(this == _tree._root || KeyNum >= _tree.MinKeyNum);
                if (index < KeyNum - 1)
                {
                    Array.Copy(_keys, index + 1, _keys, index, KeyNum - index - 1);
                    Array.Copy(_values, index + 1, _values, index, KeyNum - index - 1);
                }
                _keys[KeyNum - 1] = default(TKey);
                _values[KeyNum - 1] = default(TValue);
                --KeyNum;
            }

            public BPlusTreeNode MergeWithRight()
            {
                Debug.Assert(Parent != null);
                Debug.Assert(ChildIndex >= 0 && ChildIndex < Parent.KeyNum - 1);
                Debug.Assert(Parent.Children[ChildIndex] == this);
                var siblingIndex = ChildIndex + 1;
                var sibling = Parent.Children[siblingIndex];
                Debug.Assert(KeyNum < _tree.MinKeyNum && sibling.KeyNum == _tree.MinKeyNum
                    || KeyNum == _tree.MinKeyNum && sibling.KeyNum < _tree.MinKeyNum);
                Debug.Assert(sibling.Parent == Parent);
                if (Keys.Length < _tree.MaxKeyNum) Array.Resize(ref _keys, _tree.MaxKeyNum);
                Array.Copy(sibling._keys, 0, _keys, KeyNum, sibling.KeyNum);
                if (IsLeafNode)
                {
                    if (_values.Length < _tree.MaxKeyNum) Array.Resize(ref _values, _tree.MaxKeyNum);
                    Array.Copy(sibling._values, 0, _values, KeyNum, sibling.KeyNum);
                }
                else
                {
                    if (_children.Length < _tree.MaxKeyNum) Array.Resize(ref _children, _tree.MaxKeyNum);
                    Array.Copy(sibling._children, 0, _children, KeyNum, sibling.KeyNum);
                    for (var i = KeyNum; i < KeyNum + sibling.KeyNum; i++)
                    {
                        _children[i].Parent = this;
                        _children[i].ChildIndex = i;
                    }
                }
                KeyNum += sibling.KeyNum;
                if (IsLeafNode) Next = sibling.Next;
                return sibling;
            }

            public void InsertKeyChild(TKey key, BPlusTreeNode node, int index)
            {
                Debug.Assert(key != null);
                Debug.Assert(node != null);
                Debug.Assert(index >= 0 && index <= KeyNum);
                Debug.Assert(!IsLeafNode);
                Debug.Assert(KeyNum + 1 <= _tree.MaxKeyNum);

                if (KeyNum + 1 > _tree.MinKeyNum)
                {
                    Array.Resize(ref _keys, _tree.MaxKeyNum);
                    Array.Resize(ref _children, _tree.MaxKeyNum);
                }
                if (index < KeyNum)
                {
                    Array.Copy(_keys, index, _keys, index + 1, KeyNum - index);
                    Array.Copy(_children, index, _children, index + 1, KeyNum - index);
                }
                _keys[index] = key;
                _children[index] = node;
                node.ChildIndex = index;
                node.Parent = this;
                ++KeyNum;
                // update index of children
                for (var i = index + 1; i < KeyNum; i++)
                {
                    ++_children[i].ChildIndex;
                }

            }

            public void RemoveKeyChild(int index)
            {
                Debug.Assert(index >= 0 && index < KeyNum);
                Debug.Assert(!IsLeafNode);
                Debug.Assert(this != _tree._root && KeyNum >= _tree.MinKeyNum || this == _tree._root && KeyNum >= 2);
                if (index < KeyNum - 1)
                {
                    Array.Copy(_keys, index + 1, _keys, index, KeyNum - index - 1);
                    Array.Copy(_children, index + 1, _children, index, KeyNum - index - 1);
                }
                _keys[KeyNum - 1] = default(TKey);
                _children[KeyNum - 1] = null;
                --KeyNum;
                // update index of children
                for (var i = index; i < KeyNum; i++)
                {
                    --_children[i].ChildIndex;
                }
            }

            public void Invalidate()
            {
                _tree = null;
                Next = null;
                Parent = null;
                KeyNum = 0;

                if (_children != null)
                {
                    Array.Clear(_children, 0, _children.Length);
                    _children = null;
                }
                if (_keys != null)
                {
                    Array.Clear(_keys, 0, _keys.Length);
                    _keys = null;
                }
                if (_values != null)
                {
                    Array.Clear(_values, 0, _values.Length);
                    _values = null;
                }
            }

            public Tuple<bool, int> FindLastOfLessOrEqual(TKey key)
            {
                Debug.Assert(key != null);
                var low = -1;
                var high = KeyNum - 1;
                while (low < high)
                {
                    var mid = (low + high + 1) >> 1;
                    var cmp = _tree._comparer.Compare(Keys[mid], key);
                    if (cmp < 0) low = mid;
                    else if (cmp == 0) return Tuple.Create(true, mid);
                    else high = mid - 1;
                }
                return Tuple.Create(low >= 0 && _tree._comparer.Compare(Keys[low], key) == 0, low);
            }

            public BPlusTreeNode GetMinLeafNode()
            {
                var p = this;
                while (!p.IsLeafNode)
                {
                    p = p.Children[0];
                }
                return p;
            }

            private BPlusTreeNode GetMaxLeafNode()
            {
                var p = this;
                while (!p.IsLeafNode)
                {
                    p = p.Children[p.KeyNum - 1];
                }
                return p;
            }

            public BPlusTreeNode Split()
            {
                Debug.Assert(IsKeyFull);
                Debug.Assert(_keys != null);

                var newNode = new BPlusTreeNode(_tree, IsLeafNode);
                Array.Copy(_keys, _tree.MinKeyNum, newNode._keys, 0, _tree.MinKeyNum);
                Array.Clear(_keys, _tree.MinKeyNum, _tree.MinKeyNum);
                // Array.Resize(ref _keys, _tree.MinKeyNum);

                if (IsLeafNode)
                {
                    Debug.Assert(_values != null);
                    Array.Copy(_values, _tree.MinKeyNum, newNode._values, 0, _tree.MinKeyNum);
                    Array.Clear(_values, _tree.MinKeyNum, _tree.MinKeyNum);
                    // Array.Resize(ref _values, _tree.MinKeyNum);
                }
                else
                {
                    Debug.Assert(_children != null);
                    Array.Copy(_children, _tree.MinKeyNum, newNode._children, 0, _tree.MinKeyNum);
                    for (var i = 0; i < _tree.MinKeyNum; i++)
                    {
                        newNode._children[i].Parent = newNode;
                        newNode._children[i].ChildIndex = i;
                    }
                    Array.Clear(_children, _tree.MinKeyNum, _tree.MinKeyNum);
                    // Array.Resize(ref _children, _tree.MinKeyNum);
                }
                newNode.KeyNum = _tree.MinKeyNum;
                KeyNum -= _tree.MinKeyNum;
                if (IsLeafNode)
                {
                    newNode.Next = Next;
                    Next = newNode;
                }
                return newNode;
            }
        }

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;
            private readonly BPlusTree<TKey, TValue> _tree;
            private readonly int _version;
            private int _index;

            internal Enumerator(BPlusTree<TKey, TValue> tree)
            {
                _tree = tree;
                _enumerator = tree.Traverse().GetEnumerator();
                _version = tree._version;
                _index = -1;
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

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

