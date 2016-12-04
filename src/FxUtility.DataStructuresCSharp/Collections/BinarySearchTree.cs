using System;
using System.Collections.Generic;
using FxUtility.Node;

namespace FxUtility.Collections
{
    public class BinarySearchTree<TKey, TValue> : BaseBinarySearchTree<TKey, TValue, BinarySearchTreeNode<TKey, TValue>>
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
