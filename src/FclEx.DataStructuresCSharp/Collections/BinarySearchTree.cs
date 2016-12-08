using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FclEx.Node;

namespace FclEx.Collections
{
    public abstract class BinarySearchTree<TKey, TValue, TNode> : IKeyValueCollection<TKey, TValue> where TNode : BinaryNode<KeyValuePair<TKey, TValue>, TNode>
    {
        protected virtual TNode Root { get; set; }
        protected int _count;
        protected int _version;
        protected readonly IComparer<TKey> _comparer;
        protected readonly EqualityComparer<TValue> _valueComparer;

        protected BinarySearchTree(IComparer<TKey> comparer = null)
        {
            _count = 0;
            _version = 0;
            _comparer = comparer ?? Comparer<TKey>.Default;
            _valueComparer = EqualityComparer<TValue>.Default;
        }

        protected BinarySearchTree(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer = null) : this(comparer)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            foreach (var item in dictionary)
            {
                Add(item);
            }
        }

        public virtual bool ContainsValue(TValue value) => Traverse().Any(node => _valueComparer.Equals(node.Item.Value, value));

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new BaseEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract void Add(KeyValuePair<TKey, TValue> item);

        protected static void Clear(TNode root)
        {
            if (root == null) return;
            var queue = new Queue<TNode>();
            queue.Enqueue(root);
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                if (item.HasLeftChild) queue.Enqueue(item.LeftChild);
                if (item.HasRightChild) queue.Enqueue(item.RightChild);
                item.Invalidate();
            }
        }

        public virtual void Clear()
        {
            Clear(Root);
            Root = null;
            _count = 0;
            _version++;
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item) => BinarySearch(item.Key, true, item.Value) != null;

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _count + arrayIndex) throw new ArgumentException(nameof(array));

            foreach (var node in Traverse())
            {
                array[arrayIndex++] = node.Item;
            }
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        public virtual bool Remove(TKey key)
        {
            var node = BinarySearch(key);
            if (node == null) return false;
            RemoveNode(node);
            return true;
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var node = BinarySearch(item.Key, true, item.Value);
            if (node == null) return false;
            RemoveNode(node);
            return true;
        }

        protected abstract void RemoveNode(TNode node);

        public virtual bool ContainsKey(TKey key) => BinarySearch(key) != null;

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            var node = BinarySearch(key);
            if (node == null)
            {
                value = default(TValue);
                return false;
            }
            value = node.Item.Value;
            return true;
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                var node = BinarySearch(key);
                if (node == null) throw new KeyNotFoundException();
                return node.Item.Value;
            }

            set
            {
                var node = BinarySearch(key);
                if (node == null) Add(key, value);
                else node.Item = new KeyValuePair<TKey, TValue>(key, value);
                _version++;
            }
        }

        public virtual ICollection<TKey> Keys => new BaseKeyCollection<TKey, TValue>(this);

        public virtual ICollection<TValue> Values => new BaseValueCollection<TKey, TValue>(this);

        public IEnumerable<TNode> PreOrderTraverse() => BinaryNode<KeyValuePair<TKey, TValue>, TNode>.PreOrderTraverse(Root);
        public IEnumerable<TNode> PreOrderTraverseNonRec()
        {
            /*
                根据前序遍历访问的顺序，优先访问根结点，然后再分别访问左孩子和右孩子。即对于任一结点，
                其可看做是根结点，因此可以直接访问，访问完之后，若其左孩子不为空，按相同规则访问它的左子树；
                当访问其左子树时，再访问它的右子树。因此其处理过程如下：
                 对于任一结点P：
                 1)访问结点P，并将结点P入栈;
                 2)判断结点P的左孩子是否为空，若为空，则取栈顶结点并进行出栈操作，并将栈顶结点的右孩子置为当前的结点P，循环至1);若不为空，则将P的左孩子置为当前的结点P;
                 3)直到P为NULL并且栈为空，则遍历结束。
            */
            var p = Root;
            var stack = new Stack<TNode>();
            while (p != null || stack.Count != 0)
            {
                while (p != null)
                {
                    yield return p;
                    stack.Push(p);
                    p = p.LeftChild;
                }
                if (stack.Count != 0) p = stack.Pop().RightChild;
            }
        }

        public IEnumerable<TNode> InOrderTraverse() => BinaryNode<KeyValuePair<TKey, TValue>, TNode>.InOrderTraverse(Root);
        public IEnumerable<TNode> InOrderTraverseNonRec()
        {
            /*
            根据中序遍历的顺序，对于任一结点，优先访问其左孩子，而左孩子结点又可以看做一根结点，
            然后继续访问其左孩子结点，直到遇到左孩子结点为空的结点才进行访问，
            然后按相同的规则访问其右子树。因此其处理过程如下：
               对于任一结点P，
              1)若其左孩子不为空，则将P入栈并将P的左孩子置为当前的P，然后对当前结点P再进行相同的处理；
              2)若其左孩子为空，则取栈顶元素并进行出栈操作，访问该栈顶结点，然后将当前的P置为栈顶结点的右孩子；
              3)直到P为NULL并且栈为空则遍历结束
            */
            var s = new Stack<TNode>();
            var p = Root;
            while (p != null || s.Count != 0)
            {
                while (p != null)
                {
                    s.Push(p);
                    p = p.LeftChild;
                }
                if (s.Count != 0)
                {
                    var top = s.Pop();
                    yield return top;
                    p = top.RightChild;
                }
            }
        }

        public IEnumerable<TNode> PostOrderTraverse() => BinaryNode<KeyValuePair<TKey, TValue>, TNode>.PostOrderTraverse(Root);
        public IEnumerable<TNode> PostOrderTraverseNonRec()
        {
            // 后序遍历的非递归实现是三种遍历方式中最难的一种。因为在后序遍历中，
            // 要保证左孩子和右孩子都已被访问并且左孩子在右孩子前访问才能访问根结点，这就为流程的控制带来了难题。
            /*
            要保证根结点在左孩子和右孩子访问之后才能访问，因此对于任一结点P，先将其入栈。
            如果P不存在左孩子和右孩子，则可以直接访问它；或者P存在左孩子或者右孩子，
            但是其左孩子和右孩子都已被访问过了，则同样可以直接访问该结点。若非上述两种情况，
            则将P的右孩子和左孩子依次入栈，这样就保证了每次取栈顶元素的时候，
            左孩子在右孩子前面被访问，左孩子和右孩子都在根结点前面被访问。
            */

            TNode preNode = null;
            var stack = new Stack<TNode>();
            stack.Push(Root);
            while (stack.Count != 0)
            {
                var curNode = stack.Peek();
                if ((curNode.LeftChild == null && curNode.RightChild == null)
                    || (preNode != null && (preNode == curNode.LeftChild || preNode == curNode.RightChild)))
                {
                    yield return curNode;  // 如果当前结点没有孩子结点或者孩子节点都已被访问过 
                    stack.Pop();
                    preNode = curNode;      // 如果上一个节点是当前节点的孩子节点，说明当前节点的所有孩子节点已经被访问过了
                }
                else
                {
                    if (curNode.RightChild != null) stack.Push(curNode.RightChild);
                    if (curNode.LeftChild != null) stack.Push(curNode.LeftChild);
                }
            }
        }

        public IEnumerable<TNode> LayerTraverse()=> BinaryNode<KeyValuePair<TKey, TValue>, TNode>.LayerTraverse(Root);

        protected virtual IEnumerable<TNode> Traverse() => InOrderTraverse();

        protected TNode BinarySearch(TKey key, bool checkValue = false, TValue value = default(TValue))
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var node = Root;
            while (node != null)
            {
                var cmp = _comparer.Compare(node.Item.Key, key);
                if (cmp == 0) return (checkValue && !_valueComparer.Equals(node.Item.Value, value)) ? null : node;
                var childIndex = cmp > 0 ? 0 : 1;
                node = node.Children[childIndex];
            }
            return null;
        }

        private struct BaseEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private IEnumerator<TNode> _enumerator;
            private readonly BinarySearchTree<TKey, TValue, TNode> _tree;
            private readonly int _version;
            private int _index;

            internal BaseEnumerator(BinarySearchTree<TKey, TValue, TNode> tree)
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
                    return _enumerator.Current.Item;
                }
            }
            object IEnumerator.Current => Current;
        }
    }

    public class BinarySearchTreeNode<TKey, TValue> : BinaryNode<KeyValuePair<TKey, TValue>, BinarySearchTreeNode<TKey, TValue>>
    {
        public BinarySearchTreeNode(TKey key, TValue value, BinarySearchTreeNode<TKey, TValue> left = null,
            BinarySearchTreeNode<TKey, TValue> right = null, BinarySearchTreeNode<TKey, TValue> parent = null)
            : this(new KeyValuePair<TKey, TValue>(key, value), left, right, parent) { }

        public BinarySearchTreeNode(KeyValuePair<TKey, TValue> item, BinarySearchTreeNode<TKey, TValue> left = null,
            BinarySearchTreeNode<TKey, TValue> right = null, BinarySearchTreeNode<TKey, TValue> parent = null)
        : base(item, left, right, parent) { }
    }

    public class BinarySearchTree<TKey, TValue> : BinarySearchTree<TKey, TValue, BinarySearchTreeNode<TKey, TValue>>
    {
        private readonly BinarySearchTreeNode<TKey, TValue> _head = new BinarySearchTreeNode<TKey, TValue>(default(KeyValuePair<TKey, TValue>));

        protected override BinarySearchTreeNode<TKey, TValue> Root
        {
            get { return _head.RightChild; }
            set { _head.RightChild = value; }
        }

        public BinarySearchTree(IComparer<TKey> comparer = null) : base(comparer) { }

        public BinarySearchTree(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer = null) : base(dictionary, comparer) { }

        public override void Add(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
            var node = new BinarySearchTreeNode<TKey, TValue>(item);
            if (Root == null)
            {
                Root = node;
                Root.Parent = _head;
            }
            else
            {
                var currentNode = Root;
                while (currentNode != null)
                {
                    var cmp = _comparer.Compare(currentNode.Item.Key, item.Key);
                    if (cmp == 0) throw new ArgumentException($"An item with the same key has already been added. Key: {item.Key}");
                    var childIndex = cmp > 0 ? 0 : 1;
                    if (currentNode.Children[childIndex] != null) currentNode = currentNode.Children[childIndex];
                    else
                    {
                        currentNode.Children[childIndex] = node;
                        node.Parent = currentNode;
                        break;
                    }
                }

            }
            _count++;
            _version++;
        }

        private static BinarySearchTreeNode<TKey, TValue> GetMin(BinarySearchTreeNode<TKey, TValue> root)
        {
            if (root == null) return null;
            var cur = root;
            while (cur.LeftChild != null)
            {
                cur = cur.LeftChild;
            }
            return cur;
        }

        protected override void RemoveNode(BinarySearchTreeNode<TKey, TValue> node)
        {
            var parent = node.Parent;
            var removeIndex = parent.LeftChild == node ? 0 : 1;
            if (node.IsLeafNode) parent.Children[removeIndex] = null;
            else if (node.OnlyHasOneChild)
            {
                var childIndex = node.OnlyHasLeftChild ? 0 : 1;
                parent.Children[removeIndex] = node.Children[childIndex];
                node.Children[childIndex].Parent = parent;
            }
            else
            {
                var successor = GetMin(node.RightChild);
                if (successor == node.RightChild)
                {
                    var nodeLeft = node.LeftChild;
                    parent.Children[removeIndex] = successor;
                    successor.Parent = parent;
                    successor.LeftChild = nodeLeft;
                    nodeLeft.Parent = successor;
                }
                else
                {
                    successor.Parent.LeftChild = successor.RightChild;
                    if (successor.HasRightChild) successor.RightChild.Parent = successor.Parent;
                    node.Item = successor.Item;
                    node = successor;
                }
            }
            node.Invalidate();
            --_count;
            ++_version;
        }
    }
}
