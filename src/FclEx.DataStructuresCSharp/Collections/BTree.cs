using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FclEx.Collections
{
    public class BTree<TKey, TValue> : IKeyValueCollection<TKey, TValue>
    {
        private const int DefaultMinDegree = 5;
        public int MinDegree { get; } // t
        private int MaxDegree => 2 * MinDegree;
        private int MinKeyNum => MinDegree - 1;
        private int MaxKeyNum => 2 * MinDegree - 1;

        private BTreeNode _root;
        private int _count;
        private int _version;
        private readonly IComparer<TKey> _comparer;
        private readonly EqualityComparer<TValue> _valueComparer;

        public BTree(int minDegree, IComparer<TKey> comparer)
        {
            if (minDegree < 2) throw new ArgumentOutOfRangeException(nameof(minDegree), "min degree cannot be less than 2");
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            MinDegree = minDegree;
            _root = null;
            _count = 0;
            _version = 0;
            _comparer = comparer;
            _valueComparer = EqualityComparer<TValue>.Default;
        }

        public BTree(int minDegree = DefaultMinDegree) : this(minDegree, Comparer<TKey>.Default) { }

        public BTree(IComparer<TKey> comparer) : this(DefaultMinDegree, comparer) { }

        public BTree(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer = null)
            : this(DefaultMinDegree, comparer ?? Comparer<TKey>.Default)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            foreach (var item in dictionary)
            {
                Add(item);
            }
        }

        private Tuple<BTreeNode, int> SearchItem(TKey key, bool checkValue = false, TValue value = default(TValue))
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
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

        public TValue this[TKey key]
        {
            get
            {
                var result = SearchItem(key);
                if (result == null) throw new KeyNotFoundException();
                return result.Item1.Items[result.Item2].Value;
            }

            set
            {
                var result = SearchItem(key);
                if (result == null) Add(key, value);
                else result.Item1.Items[result.Item2] = new KeyValuePair<TKey, TValue>(key, value);
                _version++;
            }
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => new BaseKeyCollection<TKey, TValue>(this);

        public ICollection<TValue> Values => new BaseValueCollection<TKey, TValue>(this);

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
            if (_root == null) _root = new BTreeNode(true, MinDegree);
            var node = _root;
            if (node.KeyNum == MaxKeyNum)
            {
                var s = new BTreeNode(false, MinDegree);
                _root = s;
                s.Children[0] = node;
                node.Parent = s;
                SplitNode(s, 0);
                InsertNonFull(s, item);
            }
            else InsertNonFull(node, item);
            _count++;
            _version++;
        }

        private int SearchInNode(BTreeNode x, TKey key, bool checkValue = false, TValue value = default(TValue))
        {
            for (var j = 0; j < x.KeyNum; j++)
            {
                if (_comparer.Compare(key, x.Items[j].Key) == 0)
                {
                    return (checkValue && !_valueComparer.Equals(x.Items[j].Value, value)) ? -1 : j;
                }
            }
            return -1;
        }

        private void InsertNonFull(BTreeNode x, KeyValuePair<TKey, TValue> item)
        {
            var i = x.KeyNum;
            if (x.IsLeafNode)
            {
                if (SearchInNode(x, item.Key) >= 0) throw new ArgumentException($"An item with the same key has already been added. Key: {item.Key}");
                while (i > 0 && _comparer.Compare(item.Key, x.Items[i - 1].Key) < 0)
                {
                    x.Items[i] = x.Items[i - 1];
                    i--;
                }
                x.Items[i] = item;
                x.KeyNum++;
            }
            else
            {
                while (i > 0 && _comparer.Compare(item.Key, x.Items[i - 1].Key) < 0)
                {
                    i--;
                }
                if (x.Children[i].KeyNum == MaxKeyNum)
                {
                    SplitNode(x, i);
                    if (_comparer.Compare(item.Key, x.Items[i].Key) > 0) i++;
                }
                InsertNonFull(x.Children[i], item);
            }
        }

        private void SplitNode(BTreeNode parentNode, int splitIndex)
        {
            var splitNode = parentNode.Children[splitIndex];
            var newNode = new BTreeNode(splitNode.IsLeafNode, MinDegree) { KeyNum = MinKeyNum }; // the new node is the right brother of the splitNode
            for (var j = 0; j < MinKeyNum; j++)
            {
                newNode.Items[j] = splitNode.Items[j + MinDegree];
                splitNode.Items[j + MinDegree] = default(KeyValuePair<TKey, TValue>);
            }
            if (!splitNode.IsLeafNode)
            {
                for (var j = 0; j < MinDegree; j++)
                {
                    newNode.Children[j] = splitNode.Children[j + MinDegree];
                    newNode.Children[j].Parent = newNode;
                    splitNode.Children[j + MinDegree] = null;
                }
            }
            splitNode.KeyNum = MinKeyNum;
            for (var j = parentNode.KeyNum; j > splitIndex; j--)
            {
                parentNode.Children[j + 1] = parentNode.Children[j];
            }
            parentNode.Children[splitIndex + 1] = newNode;
            newNode.Parent = parentNode;
            for (var j = parentNode.KeyNum; j > splitIndex; j--)
            {
                parentNode.Items[j] = parentNode.Items[j - 1];
            }
            parentNode.Items[splitIndex] = splitNode.Items[MinDegree - 1];
            parentNode.KeyNum++;
        }

        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        private static void Clear(BTreeNode root)
        {
            if (root == null) return;
            var queue = new Queue<BTreeNode>();
            queue.Enqueue(root);
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                if (!item.IsLeafNode)
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
            _root = null;
            _count = 0;
            _version++;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => SearchItem(item.Key, true, item.Value) != null;

        public bool ContainsKey(TKey key) => SearchItem(key) != null;

        public bool ContainsValue(TValue value) => Traverse().Any(item => _valueComparer.Equals(item.Value, value));

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

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this);

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = SearchItem(item.Key, true, item.Value);
            if (result == null) return false;
            RemoveItemFromNode(result.Item1, result.Item2);
            return true;
        }

        public bool Remove(TKey key)
        {
            var result = SearchItem(key);
            if (result == null) return false;
            RemoveItemFromNode(result.Item1, result.Item2);
            return true;
        }

        private void RemoveItemFromNode(BTreeNode node, int itemIndex)
        {
            DeleteItem(node, itemIndex);
            _count--;
            _version++;
        }

        private void DeleteItem(BTreeNode node, int itemIndex)
        {
            if (node.IsLeafNode)
            {
                DeleteItemAndChildFromNode(node, itemIndex);
                if (node != _root && node.KeyNum < MinKeyNum) RebalanceForDeletion(node);
            }
            else
            {
                var left = node.Children[itemIndex];
                var right = node.Children[itemIndex + 1];
                var leftMaxNode = FindMaxNode(left);
                var rightMinNode = FindMinNode(right);
                var successor = leftMaxNode.KeyNum > MinKeyNum // choose the successor in leftMaxNode
                    ? Tuple.Create(leftMaxNode, leftMaxNode.KeyNum - 1)
                    : Tuple.Create(rightMinNode, 0);
                node.Items[itemIndex] = successor.Item1.Items[successor.Item2];
                DeleteItem(successor.Item1, successor.Item2);
            }
        }

        private static void DeleteItemAndChildFromNode(BTreeNode node, int itemIndex, int childIndex = -1)
        {
            Debug.Assert(itemIndex >= 0 && itemIndex < node.KeyNum, $"{nameof(itemIndex)} is out of range");
            for (var i = itemIndex; i < node.KeyNum - 1; i++)
            {
                node.Items[i] = node.Items[i + 1];
            }
            node.Items[node.KeyNum - 1] = default(KeyValuePair<TKey, TValue>);

            if (!node.IsLeafNode)
            {
                Debug.Assert(childIndex >= 0 && childIndex <= node.KeyNum, $"{nameof(childIndex)} is out of range");
                if (node.Children[childIndex] != null) node.Children[childIndex].Parent = null;
                for (var i = childIndex; i < node.KeyNum; i++)
                {
                    node.Children[i] = node.Children[i + 1];
                }
                node.Children[node.KeyNum] = null;
            }
            node.KeyNum--;
        }

        private static void InsertItemAndChildIntoNode(BTreeNode node, KeyValuePair<TKey, TValue> item, int itemInsertIndex, BTreeNode child = null, int childInsertIndex = -1)
        {
            Debug.Assert(itemInsertIndex >= 0 && itemInsertIndex <= node.KeyNum, $"{nameof(itemInsertIndex)} is out of range");
            for (var i = node.KeyNum - 1; i >= itemInsertIndex; i--)
            {
                node.Items[i + 1] = node.Items[i];
            }
            node.Items[itemInsertIndex] = item;

            if (!node.IsLeafNode)
            {
                Debug.Assert(child != null);
                Debug.Assert(childInsertIndex >= 0 && childInsertIndex <= node.KeyNum + 1, $"{nameof(childInsertIndex)} is out of range");
                for (var i = node.KeyNum; i >= childInsertIndex; i--)
                {
                    node.Children[i + 1] = node.Children[i];
                }
                node.Children[childInsertIndex] = child;
                child.Parent = node;
            }
            node.KeyNum++;
        }

        private void RebalanceForDeletion(BTreeNode node)
        {
            Debug.Assert(node != _root && node.KeyNum < MinKeyNum, $"{nameof(node)} does not need to rebalance");
            var parent = node.Parent;
            var childIndex = node.GetChildIndex();
            Debug.Assert(childIndex >= 0, "pointers between child and parent are not correct");

            var rightSiblingIndex = childIndex + 1;
            var leftSiblingIndex = childIndex - 1;
            // case 1: the deficient node's right sibling exists and has more than the minimum number of elements, then rotate left
            if (rightSiblingIndex <= parent.KeyNum && parent.Children[rightSiblingIndex].KeyNum > MinKeyNum)
            {
                var sibling = parent.Children[rightSiblingIndex];
                // Copy the separator from the parent to the end of the deficient node 
                // (the separator moves down; the deficient node now has the minimum number of elements)
                node.Items[node.KeyNum++] = parent.Items[childIndex];
                // Replace the separator in the parent with the first element of the right sibling
                // (right sibling loses one node but still has at least the minimum number of elements)
                parent.Items[childIndex] = sibling.Items[0];
                // remove the first element of the right sibling
                if (sibling.IsLeafNode) DeleteItemAndChildFromNode(sibling, 0);
                else
                {
                    var firstChildOfRight = sibling.Children[0];
                    DeleteItemAndChildFromNode(sibling, 0, 0);
                    node.Children[node.KeyNum] = firstChildOfRight;
                    firstChildOfRight.Parent = node;
                }
            }
            // case 2: the deficient node's left sibling exists and has more than the minimum number of elements, then rotate right
            else if (leftSiblingIndex >= 0 && parent.Children[leftSiblingIndex].KeyNum > MinKeyNum)
            {
                var sibling = parent.Children[leftSiblingIndex];
                // Copy the separator from the parent to the start of the deficient node
                // (the separator moves down; deficient node now has the minimum number of elements)
                if (sibling.IsLeafNode) InsertItemAndChildIntoNode(node, parent.Items[leftSiblingIndex], 0);
                else
                {
                    var lastChildOfLeft = sibling.Children[sibling.KeyNum];
                    sibling.Children[sibling.KeyNum] = null;
                    InsertItemAndChildIntoNode(node, parent.Items[leftSiblingIndex], 0, lastChildOfLeft, 0);
                }
                // Replace the separator in the parent with the last element of the left sibling
                // (left sibling loses one node but still has at least the minimum number of elements)
                parent.Items[leftSiblingIndex] = sibling.Items[sibling.KeyNum - 1];
                // remove the last element of the left sibling
                sibling.Items[--sibling.KeyNum] = default(KeyValuePair<TKey, TValue>);
            }
            // case 3: if both immediate siblings have only the minimum number of elements, then merge with a sibling sandwiching their separator taken off from their parent
            // case 3-a: the deficient node's right sibling exists
            else if (rightSiblingIndex <= parent.KeyNum) MergeTwoNodes(parent, childIndex);
            // case 3-b: the deficient node's left sibling exists
            else if (leftSiblingIndex >= 0) MergeTwoNodes(parent, leftSiblingIndex);
            else
            {
                Debug.Assert(false, "cannot reach here!");
            }
        }

        private void MergeTwoNodes(BTreeNode parent, int leftIndex)
        {
            Debug.Assert(leftIndex >= 0 && leftIndex < parent.KeyNum, $"{nameof(leftIndex)} is out of range");

            var left = parent.Children[leftIndex];
            var right = parent.Children[leftIndex + 1];

            Debug.Assert(left.KeyNum <= MinKeyNum, $"{nameof(left)} has more than {MinKeyNum} items");
            Debug.Assert(right.KeyNum <= MinKeyNum, $"{nameof(right)} has more than {MinKeyNum} items");

            left.Items[left.KeyNum++] = parent.Items[leftIndex];
            for (var i = 0; i < right.KeyNum; i++)
            {
                left.Items[left.KeyNum + i] = right.Items[i];
                right.Items[i] = default(KeyValuePair<TKey, TValue>);
            }
            if (!right.IsLeafNode)
            {
                for (var i = 0; i < right.KeyNum + 1; i++)
                {
                    left.Children[left.KeyNum + i] = right.Children[i];
                    left.Children[left.KeyNum + i].Parent = left;
                    right.Children[i] = null;
                }
            }
            left.KeyNum += right.KeyNum;
            right.Invalidate();
            DeleteItemAndChildFromNode(parent, leftIndex, leftIndex + 1);

            if (ReferenceEquals(parent, _root) && parent.KeyNum == 0)
            {
                _root = left;
                left.Parent = null;
            }
            else if (parent != _root && parent.KeyNum < MinKeyNum) RebalanceForDeletion(parent);
        }

        private static BTreeNode FindMinNode(BTreeNode node)
        {
            Debug.Assert(node != null);
            var p = node;
            while (!p.IsLeafNode)
            {
                p = p.Children[0];
            }
            return p;
        }

        private static BTreeNode FindMaxNode(BTreeNode node)
        {
            Debug.Assert(node != null);
            var p = node;
            while (!p.IsLeafNode)
            {
                p = p.Children[p.KeyNum];
            }
            return p;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = SearchItem(key);
            if (result == null)
            {
                value = default(TValue);
                return false;
            }
            value = result.Item1.Items[result.Item2].Value;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<KeyValuePair<TKey, TValue>> Traverse() => InOrderTraverse(_root);

        private static IEnumerable<KeyValuePair<TKey, TValue>> InOrderTraverse(BTreeNode node)
        {
            if (node == null) yield break;
            for (var i = 0; i < node.KeyNum; i++)
            {
                if (!node.IsLeafNode)
                {
                    Debug.Assert(node.Children[i] != null);
                    foreach (var n in InOrderTraverse(node.Children[i]))
                    {
                        yield return n;
                    }
                }

                yield return node.Items[i];
            }
            if (!node.IsLeafNode)
            {
                Debug.Assert(node.Children[node.KeyNum] != null);
                foreach (var n in InOrderTraverse(node.Children[node.KeyNum]))
                {
                    yield return n;
                }
            }
        }

        // for testing
        /*
                private IEnumerable<BTreeNode> NodeLayerTraverse()
                {
                    if (_root == null) yield break;
                    var queue = new Queue<BTreeNode>();
                    queue.Enqueue(_root);
                    while (queue.Count != 0)
                    {
                        var node = queue.Dequeue();
                        yield return node;
                        if (!node.IsLeafNode)
                        {
                            foreach (var child in node.Children)
                            {
                                if (child != null)
                                {
                                    queue.Enqueue(child);
                                }
                            }
                        }
                    }
                }

                private IEnumerable<BTreeNode> GetErrorNode() => NodeLayerTraverse().Where(item =>
                {
                    if (item != _root && item.Parent == null) return true;
                    if (item == _root && item.Parent != null) return true;
                    if (item.Children != null)
                    {
                        if (item.IsLeafNode) return true;
                        if (item.Children.Count(child => child != null) != item.KeyNum + 1) return true;
                        if (item.Children.Take(item.KeyNum + 1).Any(child => child == null)) return true;
                    }
                    else
                    {
                        if (!item.IsLeafNode) return true;
                    }
                    for (var i = 0; i < item.KeyNum - 1; i++)
                    {
                        if (_less.Compare(item.Items[i].Key, item.Items[i + 1].Key) >= 0) return true;
                    }
                    return false;
                });

                public bool ErrorNodeExists() => GetErrorNode().Any();

                public bool IsOrdered()
                {
                    var list = Traverse().ToList();
                    for (var i = 0; i < list.Count - 1; i++)
                    {
                        if (_less.Compare(list[i].Key, list[i + 1].Key) >= 0) return false;
                    }
                    return true;
                }
        */

        private class BTreeNode
        {
            public int KeyNum { get; set; } // 关键字个数, n
            private int MinDegree { get; } // t
            private int MaxDegree => 2 * MinDegree;
            private int MaxKeyNum => 2 * MinDegree - 1;
            public KeyValuePair<TKey, TValue>[] Items { get; private set; }
            public bool IsLeafNode
            {
                get { return Children == null; }
                private set
                {
                    if (value)
                    {
                        if (Children != null) Array.Clear(Children, 0, Children.Length);
                        Children = null;
                    }
                    else
                    {
                        if (Children == null) Children = new BTreeNode[MaxDegree];
                    }
                }
            }
            public BTreeNode[] Children { get; private set; }
            public BTreeNode Parent { get; set; }

            public BTreeNode(bool isLeaf, int minDegree)
            {
                MinDegree = minDegree;
                KeyNum = 0;
                Items = new KeyValuePair<TKey, TValue>[MaxKeyNum]; // t-1 ~ 2t-1
                IsLeafNode = isLeaf;
            }

            public void Invalidate()
            {
                KeyNum = 0;
                if (Children != null) Array.Clear(Children, 0, Children.Length);
                if (Items != null) Array.Clear(Items, 0, Items.Length);
                Children = null;
                Items = null;
                Parent = null;
            }

            public int GetChildIndex()
            {
                if (Parent != null)
                {
                    for (var i = 0; i < Parent.KeyNum + 1; i++)
                    {
                        if (ReferenceEquals(Parent.Children[i], this)) return i;
                    }
                }
                return -1;
            }
        }

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;
            private readonly BTree<TKey, TValue> _tree;
            private readonly int _version;
            private int _index;

            internal Enumerator(BTree<TKey, TValue> tree)
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
