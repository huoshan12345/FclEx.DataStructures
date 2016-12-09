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
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
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

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
            /*
                To insert a value, we start at the root of the 2–3–4 tree:-
                1.	If the current node is a 4-node:
                    •	Remove and save the middle value to get a 3-node.
                    •	Split the remaining 3-node up into a pair of 2-nodes (the now missing middle value is handled in the next step).
                    •	If this is the root node (which thus has no parent):
                    •	the middle value becomes the new root 2-node and the tree height increases by 1. Ascend into the root.
                    •	Otherwise, push the middle value up into the parent node. Ascend into the parent node.
                2.	Find the child whose interval contains the value to be inserted.
                3.	If that child is a leaf, insert the value into the child node and finish.
                    •	Otherwise, descend into the child and repeat from step 1.
            */

            var node = _root;
            while (true)
            {
                Debug.Assert(node != null);
                if (node.IsKeyFull)
                {
                    Split(node);
                    node = node.Parent; // Ascend into the parent node.
                    Debug.Assert(node != null);
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
                var newRoot = new TwoFourTreeNode(midKey, node, newNode);
                _root = newRoot;
            }
            else
            {
                Debug.Assert(parent != null);
                var nodeIndex = node.GetChildIndex();
                Debug.Assert(parent.Children[nodeIndex] == node);
                parent.InsertItemChild(nodeIndex, midKey, newNode);
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
            var node = FindItem(item.Key, true, item.Value);
            if (node == null) return false;
            RemoveItem(node.Item1, node.Item2);
            return true;
        }

        public bool Remove(TKey key)
        {
            var node = FindItem(key);
            if (node == null) return false;
            RemoveItem(node.Item1, node.Item2);
            return true;
        }

        private void RemoveItem(TwoFourTreeNode node, int index)
        {
            /*
             To remove a value from the 2–3–4 tree:
                1.	Find the element to be deleted.
                    •	If the element is not in a leaf node, remember its location and continue searching until a leaf, which will contain the element’s successor, is reached. 
                        The successor can be either the largest key that is smaller than the one to be removed, or the smallest key that is larger than the one to be removed. 
                        It is simplest to make adjustments to the tree from the top down such that the leaf node found is not a 2-node. That way, after the swap, there will not be an empty leaf node.
                    •	If the element is in a 2-node leaf, just make the adjustments below.
                Make the following adjustments when a 2-node – except the root node – is encountered on the way to the leaf we want to remove:
                1.	If a sibling on either side of this node is a 3-node or a 4-node (thus having more than 1 key), perform a rotation with that sibling:
                    •	The key from the other sibling closest to this node moves up to the parent key that overlooks the two nodes.
                    •	The parent key moves down to this node to form a 3-node.
                    •	The child that was originally with the rotated sibling key is now this node's additional child.
                2.	If the parent is a 2-node and the sibling is also a 2-node, combine all three elements to form a new 4-node and shorten the tree. (This rule can only trigger if the parent 2-node is the root, 
                    since all other 2-nodes along the way will have been modified to not be 2-nodes. This is why "shorten the tree" here preserves balance; this is also an important assumption for the fusion operation.)
                3.	If the parent is a 3-node or a 4-node and all adjacent siblings are 2-nodes, do a fusion operation with the parent and an adjacent sibling:
                    •	The adjacent sibling and the parent key overlooking the two sibling nodes come together to form a 4-node.
                    •	Transfer the sibling's children to this node.
                Once the sought value is reached, it can now be placed at the removed entry's location without a problem because we have ensured that the leaf node has more than 1 key.
             */

            if (node.IsLeafNode)
            {
                if (node.IsKeyMin && node != _root)
                {
                    Debug.Assert(node.Parent != null);
                    Debug.Assert(node.IsLeafNode);
                    AdjustNodeWhenRemove(node);
                    node.Invalidate();
                }
                else node.RemoveItem(index);
                _count--;
                _version++;
            }
            else
            {
                var successor = node.Children[index].GetMinLeafNode();
                var item = successor.Items[successor.KeyNum - 1];
                node.Items[index] = item;
                RemoveItem(successor, successor.KeyNum - 1);
            }
        }

        private static void AdjustNodeWhenRemove(TwoFourTreeNode node)
        {
            Debug.Assert(node.Parent != null);

            var parent = node.Parent;
            var childIndex = node.GetChildIndex();
            var leftSiblingIndex = childIndex - 1;
            var rightSiblingIndex = childIndex + 1;
            // If a sibling on either side of this node is a 3-node or a 4-node (thus having more than 1 key)
            if (rightSiblingIndex <= parent.KeyNum && !parent.Children[rightSiblingIndex].IsKeyMin)
            {
                // rotate anticlockwise
                var sibling = parent.Children[rightSiblingIndex];
                var item = sibling.RemoveFirstItem();
                node.Items[0] = parent.Items[childIndex];
                parent.Items[childIndex] = item;

            }
            else if (leftSiblingIndex >= 0 && !parent.Children[leftSiblingIndex].IsKeyMin)
            {
                // rotate clockwise
                var sibling = parent.Children[leftSiblingIndex];
                var item = sibling.RemoveLastItem();
                node.Items[0] = parent.Items[childIndex];
                parent.Items[childIndex] = item;
            }
            // If the parent is a 3-node or a 4-node and all adjacent siblings are 2-nodes
            else if (!parent.IsKeyMin && rightSiblingIndex <= parent.KeyNum && !parent.Children[rightSiblingIndex].IsKeyMin)
            {
                var sibling = parent.Children[rightSiblingIndex];
                var item = parent.Items[childIndex];
                parent.RemoveItemChild(childIndex);
                sibling.InsertItem(0, item);
                parent.Children[childIndex] = sibling;
            }
            else if (!parent.IsKeyMin && leftSiblingIndex >= 0 && parent.Children[leftSiblingIndex].IsKeyMin)
            {
                var sibling = parent.Children[leftSiblingIndex];
                var item = parent.Items[childIndex - 1];
                parent.RemoveItemChild(childIndex - 1);
                sibling.InsertItem(sibling.KeyNum, item);
            }
            // If the parent is a 2-node and the sibling is also a 2-node
            else if (parent.IsKeyMin)
            {
                var siblingIndex = leftSiblingIndex >= 0 ? 0 : 1; // parent has only two children

                parent.MergeWithChild(siblingIndex);
                if (parent.Parent == null)
                {
                    // parent is root
                    parent.MergeWithChild(siblingIndex);
                    return;
                }
                else AdjustNodeWhenRemove(parent);
            }
            else
            {
                Debug.Assert(false, "cannot reach here!");
            }
        }

        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        public bool ContainsKey(TKey key) => FindItem(key) != null;

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

        // For testing
        public IEnumerable<List<TKey[]>> ToLayerItems()
        {
            if (_root == null) yield break;
            var queue = new Queue<List<TwoFourTreeNode>>();
            queue.Enqueue(new List<TwoFourTreeNode> { _root });
            while (queue.Count != 0)
            {
                var nodes = queue.Dequeue();
                var subNodes = new List<TwoFourTreeNode>();
                var result = new List<TKey[]>();
                foreach (var node in nodes)
                {
                    result.Add(node.Items.Take(node.KeyNum).Select(m => m.Key).ToArray());
                    if (node.IsLeafNode) continue;
                    subNodes.AddRange(node.Children.Take(node.KeyNum + 1));
                }
                if (subNodes.Count != 0) queue.Enqueue(subNodes);
                yield return result;
            }
        }

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
            private TwoFourTreeNode[] _children;
            public int _keyNum;

            public int KeyNum => _keyNum;
            public KeyValuePair<TKey, TValue>[] Items => _items;
            public TwoFourTreeNode[] Children => _children;
            public TwoFourTreeNode Parent { get; private set; }
            public bool IsLeafNode
            {
                get { return _children == null; }
                set
                {
                    if (value)
                    {
                        if (Children == null) return;
                        Array.Clear(Children, 0, Children.Length);
                        _children = null;
                    }
                    else
                    {
                        if (Children == null) _children = new TwoFourTreeNode[MaxDegree];
                    }
                }
            }
            // private const bool LowMemoryUsage = false; // low mem => low speed, high mem => high speed

            public TwoFourTreeNode(bool isLeaf)
            {
                _keyNum = 0;
                _items = new KeyValuePair<TKey, TValue>[MaxKeyNum];
                IsLeafNode = isLeaf;
            }

            public TwoFourTreeNode(KeyValuePair<TKey, TValue> item, TwoFourTreeNode child1, TwoFourTreeNode child2) : this(false)
            {
                _items[0] = item;
                _children[0] = child1;
                _children[1] = child2;
                child1.Parent = this;
                child2.Parent = this;
                _keyNum = 1;
            }

            public void Invalidate()
            {
                _keyNum = 0;
                _children.Clear();
                _items.Clear();
                Parent = null;
            }

            public int GetChildIndex()
            {
                return Parent.Children.IndexOf(this);
            }

            public IEnumerable<KeyValuePair<TKey, TValue>> InOrderTraverse()
            {
                for (var i = 0; i < _keyNum; i++)
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
                    Debug.Assert(Children[_keyNum] != null);
                    foreach (var n in Children[_keyNum].InOrderTraverse())
                    {
                        yield return n;
                    }
                }
            }

            public void Clear()
            {
                var queue = new Queue<TwoFourTreeNode>();
                queue.Enqueue(this);
                while (queue.Count != 0)
                {
                    var item = queue.Dequeue();
                    if (!item.IsLeafNode)
                    {
                        for (var i = 0; i <= item._keyNum; i++)
                        {
                            Debug.Assert(item.Children[i] != null);
                            queue.Enqueue(item.Children[i]);
                        }
                    }
                    item.Invalidate();
                }
            }

            public bool IsKeyFull => _keyNum == MaxKeyNum;
            public bool IsKeyMin => _keyNum == MinKeyNum;

            public TwoFourTreeNode Split()
            {
                Debug.Assert(IsKeyFull);

                var node = new TwoFourTreeNode(IsLeafNode)
                {
                    _items = { [0] = _items[MaxKeyNum - 1] },
                    _keyNum = 1,
                    Parent = Parent
                };
#if DEBUG
                Array.Clear(_items, 1, MaxKeyNum - 1);
#endif

                if (!IsLeafNode)
                {
                    Array.Copy(_children, MaxKeyNum - 1, node._children, 0, 2);
                    for (var i = 0; i <= node._keyNum; i++)
                    {
                        node._children[i].Parent = node;
                    }
#if DEBUG
                    Array.Clear(_children, MaxKeyNum - 1, 2);
#endif
                }

                _keyNum = 1;
                return node;
            }

            public TwoFourTreeNode GetMinLeafNode()
            {
                var p = this;
                while (!p.IsLeafNode)
                {
                    p = p.Children[0];
                }
                return p;
            }

            public void InsertItemChild(int index, KeyValuePair<TKey, TValue> item, TwoFourTreeNode node)
            {
                Debug.Assert(!IsKeyFull);
                Debug.Assert(index >= 0 && index <= _keyNum);
                Debug.Assert(!IsLeafNode);
                var num = _keyNum - index;
                if (num > 0)
                {
                    Array.Copy(_items, index, _items, index + 1, num);
                    Array.Copy(_children, index + 1, _children, index + 2, num);
                }
                _items[index] = item;
                _children[index + 1] = node;
                node.Parent = this;
                _keyNum++;
            }

            public void InsertItem(int index, KeyValuePair<TKey, TValue> item)
            {
                Debug.Assert(!IsKeyFull);
                Debug.Assert(index >= 0 && index <= _keyNum);
                var num = _keyNum - index;
                if (num > 0)
                {
                    Array.Copy(_items, index, _items, index + 1, num);
                }
                _items[index] = item;
                _keyNum++;
            }

            public void RemoveItem(int index)
            {
                Debug.Assert(IsLeafNode);
                Debug.Assert(index >= 0 && index < _keyNum);
                if(Parent != null) Debug.Assert(_keyNum > MinKeyNum); // for non-root node
                var num = _keyNum - index - 1;
                if (num > 0)
                {
                    Array.Copy(_items, index + 1, _items, index, num);
                }
                --_keyNum;
#if DEBUG
                _items[_keyNum] = default(KeyValuePair<TKey, TValue>);
#endif
            }

            public KeyValuePair<TKey, TValue> RemoveFirstItem()
            {
                var item = _items[0];
                RemoveItem(0);
                return item;
            }

            public KeyValuePair<TKey, TValue> RemoveLastItem()
            {
                var item = _items[_keyNum - 1];
                RemoveItem(_keyNum - 1);
                return item;
            }

            public void RemoveItemChild(int index)
            {
                Debug.Assert(!IsLeafNode);
                Debug.Assert(index >= 0 && index < _keyNum);
                Debug.Assert(_keyNum > MinKeyNum);
                var num = _keyNum - index - 1;
                if (num > 0)
                {
                    Array.Copy(_items, index + 1, _items, index, num);
                    Array.Copy(_children, index + 2, _children, index + 1, num);
                }
                --_keyNum;
#if DEBUG
                _items[_keyNum] = default(KeyValuePair<TKey, TValue>);
                _children[_keyNum + 1] = null;
#endif
            }

            public void MergeWithChild(int index)
            {
                Debug.Assert(IsKeyMin);
                var child = _children[index];
                if (index == 0) MergeWithLeftChild(child);
                else MergeWithRightChild(child);
                _keyNum += child.KeyNum;
                child.Invalidate();
            }

            private void MergeWithRightChild(TwoFourTreeNode child)
            {
                Array.Copy(child._items, 0, _items, 1, child._keyNum);
                if (child.IsLeafNode)
                {
                    IsLeafNode = true;
                    return;
                }
                Debug.Assert(!IsLeafNode);
                for (var i = 0; i < child._keyNum + 1; i++)
                {
                    _children[i + 1] = child._children[i];
                    _children[i + 1].Parent = this;
                }
            }

            private void MergeWithLeftChild(TwoFourTreeNode child)
            {
                _items[child._keyNum] = _items[0];
                Array.Copy(child._items, 0, _items, 0, child._keyNum);
                if (child.IsLeafNode)
                {
                    IsLeafNode = true;
                    return;
                }
                Debug.Assert(!IsLeafNode);
                _children[child._keyNum + 1] = _children[1];
                for (var i = 0; i < child._keyNum + 1; i++)
                {
                    _children[i] = child._children[i];
                    _children[i].Parent = this;
                }
            }

            public void MergeWithParent()
            {
                Debug.Assert(IsKeyMin);
                Debug.Assert(Parent != null);
                Debug.Assert(Parent.IsKeyMin);
                Debug.Assert(GetChildIndex() == 0);
                _items[1] = Parent._items[0];
                _children[2] = Parent._children[1];
                _children[2].Parent = this;
                Parent = Parent.Parent;
                Parent.Invalidate();
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
