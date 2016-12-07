using System;
using System.Collections.Generic;
using FclEx.Node;

namespace FclEx.Collections
{
    public class AvlTreeNode<TKey, TValue> : BinaryTreeNode<KeyValuePair<TKey, TValue>, AvlTreeNode<TKey, TValue>>
    {

        public int BalanceFactor { get; set; }

        public AvlTreeNode(TKey key, TValue value, int balanceFactor = 0, AvlTreeNode<TKey, TValue> left = null, AvlTreeNode<TKey, TValue> right = null, AvlTreeNode<TKey, TValue> parent = null)
            : this(new KeyValuePair<TKey, TValue>(key, value), balanceFactor, left, right, parent) { }

        public AvlTreeNode(KeyValuePair<TKey, TValue> item, int balanceFactor = 0, AvlTreeNode<TKey, TValue> left = null,
            AvlTreeNode<TKey, TValue> right = null, AvlTreeNode<TKey, TValue> parent = null)
            : base(item, left, right, parent)
        {
            BalanceFactor = balanceFactor;
        }
    }

    /// <summary>
    /// AVL tree is a self-balancing binary search tree
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AvlTree<TKey, TValue> : BinarySearchTree<TKey, TValue, AvlTreeNode<TKey, TValue>>
    {
        public AvlTree(IComparer<TKey> comparer = null) : base(comparer) { }

        public AvlTree(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer = null) : base(dictionary, comparer) { }

        private void InsertBalance(AvlTreeNode<TKey, TValue> node, int balance)
        {
            while (node != null)
            {
                balance = (node.BalanceFactor += balance);
                switch (balance)
                {
                    case 0: return;

                    case 2:
                        if (node.LeftChild.BalanceFactor == 1) RotateRight(node);
                        else RotateLeftRight(node);
                        return;

                    case -2:
                        if (node.RightChild.BalanceFactor == -1) RotateLeft(node);
                        else RotateRightLeft(node);
                        return;
                }
                var parent = node.Parent;
                if (parent != null) balance = parent.LeftChild == node ? 1 : -1;
                node = parent;
            }
        }

        private AvlTreeNode<TKey, TValue> RotateLeft(AvlTreeNode<TKey, TValue> node)
        {
            var right = node.RightChild;
            var rightLeft = right.LeftChild;
            var parent = node.Parent;

            right.Parent = parent;
            right.LeftChild = node;
            node.RightChild = rightLeft;
            node.Parent = right;

            if (rightLeft != null) rightLeft.Parent = node;
            if (node == Root) Root = right;
            else if (parent.RightChild == node) parent.RightChild = right;
            else parent.LeftChild = right;

            right.BalanceFactor++;
            node.BalanceFactor = -right.BalanceFactor;
            return right;
        }

        private AvlTreeNode<TKey, TValue> RotateRight(AvlTreeNode<TKey, TValue> node)
        {
            var left = node.LeftChild;
            var leftRight = left.RightChild;
            var parent = node.Parent;

            left.Parent = parent;
            left.RightChild = node;
            node.LeftChild = leftRight;
            node.Parent = left;

            if (leftRight != null) leftRight.Parent = node;
            if (node == Root) Root = left;
            else if (parent.LeftChild == node) parent.LeftChild = left;
            else parent.RightChild = left;

            left.BalanceFactor--;
            node.BalanceFactor = -left.BalanceFactor;

            return left;
        }

        private AvlTreeNode<TKey, TValue> RotateLeftRight(AvlTreeNode<TKey, TValue> node)
        {
            var left = node.LeftChild;
            var leftRight = left.RightChild;
            var parent = node.Parent;
            var leftRightRight = leftRight.RightChild;
            var leftRightLeft = leftRight.LeftChild;

            leftRight.Parent = parent;
            node.LeftChild = leftRightRight;
            left.RightChild = leftRightLeft;
            leftRight.LeftChild = left;
            leftRight.RightChild = node;
            left.Parent = leftRight;
            node.Parent = leftRight;

            if (leftRightRight != null) leftRightRight.Parent = node;

            if (leftRightLeft != null) leftRightLeft.Parent = left;

            if (node == Root) Root = leftRight;
            else if (parent.LeftChild == node) parent.LeftChild = leftRight;
            else parent.RightChild = leftRight;

            switch (leftRight.BalanceFactor)
            {
                case -1:
                    node.BalanceFactor = 0;
                    left.BalanceFactor = 1;
                    break;

                case 0:
                    node.BalanceFactor = 0;
                    left.BalanceFactor = 0;
                    break;

                default:
                    node.BalanceFactor = -1;
                    left.BalanceFactor = 0;
                    break;
            }
            leftRight.BalanceFactor = 0;
            return leftRight;
        }

        private AvlTreeNode<TKey, TValue> RotateRightLeft(AvlTreeNode<TKey, TValue> node)
        {
            var right = node.RightChild;
            var rightLeft = right.LeftChild;
            var parent = node.Parent;
            var rightLeftLeft = rightLeft.LeftChild;
            var rightLeftRight = rightLeft.RightChild;

            rightLeft.Parent = parent;
            node.RightChild = rightLeftLeft;
            right.LeftChild = rightLeftRight;
            rightLeft.RightChild = right;
            rightLeft.LeftChild = node;
            right.Parent = rightLeft;
            node.Parent = rightLeft;

            if (rightLeftLeft != null) rightLeftLeft.Parent = node;

            if (rightLeftRight != null) rightLeftRight.Parent = right;

            if (node == Root) Root = rightLeft;
            else if (parent.RightChild == node) parent.RightChild = rightLeft;
            else parent.LeftChild = rightLeft;

            switch (rightLeft.BalanceFactor)
            {
                case 1:
                    node.BalanceFactor = 0;
                    right.BalanceFactor = -1;
                    break;

                case 0:
                    node.BalanceFactor = 0;
                    right.BalanceFactor = 0;
                    break;

                default:
                    node.BalanceFactor = 1;
                    right.BalanceFactor = 0;
                    break;
            }
            rightLeft.BalanceFactor = 0;
            return rightLeft;
        }

        public override void Add(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
            if (Root == null) Root = new AvlTreeNode<TKey, TValue>(item);
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
                        currentNode.Children[childIndex] = new AvlTreeNode<TKey, TValue>(item, parent: currentNode);
                        var balance = cmp > 0 ? 1 : -1;
                        InsertBalance(currentNode, balance);
                        break;
                    }
                }
            }
            _version++;
            _count++;
        }

        private void DeleteBalance(AvlTreeNode<TKey, TValue> node, int balance)
        {
            while (node != null)
            {
                balance = (node.BalanceFactor += balance);

                if (balance == 2)
                {
                    if (node.LeftChild.BalanceFactor >= 0)
                    {
                        node = RotateRight(node);

                        if (node.BalanceFactor == -1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateLeftRight(node);
                    }
                }
                else if (balance == -2)
                {
                    if (node.RightChild.BalanceFactor <= 0)
                    {
                        node = RotateLeft(node);

                        if (node.BalanceFactor == 1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = RotateRightLeft(node);
                    }
                }
                else if (balance != 0)
                {
                    return;
                }

                var parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.LeftChild == node ? -1 : 1;
                }

                node = parent;
            }
        }

        private static void Replace(AvlTreeNode<TKey, TValue> target, AvlTreeNode<TKey, TValue> source)
        {
            var left = source.LeftChild;
            var right = source.RightChild;
            target.BalanceFactor = source.BalanceFactor;
            target.Item = source.Item;
            target.LeftChild = left;
            target.RightChild = right;
            if (left != null) left.Parent = target;
            if (right != null) right.Parent = target;
        }

        protected override void RemoveNode(AvlTreeNode<TKey, TValue> node)
        {
            var left = node.LeftChild;
            var right = node.RightChild;
            if (left == null)
            {
                if (right == null)
                {
                    if (node == Root) Root = null;
                    else
                    {
                        var parent = node.Parent;
                        if (parent.LeftChild == node)
                        {
                            parent.LeftChild = null;
                            DeleteBalance(parent, -1);
                        }
                        else
                        {
                            parent.RightChild = null;
                            DeleteBalance(parent, 1);
                        }
                    }
                }
                else
                {
                    Replace(node, right);
                    DeleteBalance(node, 0);
                }
            }
            else if (right == null)
            {
                Replace(node, left);
                DeleteBalance(node, 0);
            }
            else
            {
                var successor = right;
                if (successor.LeftChild == null)
                {
                    var parent = node.Parent;
                    successor.Parent = parent;
                    successor.LeftChild = left;
                    successor.BalanceFactor = node.BalanceFactor;
                    left.Parent = successor;

                    if (node == Root) Root = successor;
                    else
                    {
                        if (parent.LeftChild == node) parent.LeftChild = successor;
                        else parent.RightChild = successor;
                    }

                    DeleteBalance(successor, 1);
                }
                else
                {
                    while (successor.LeftChild != null)
                    {
                        successor = successor.LeftChild;
                    }

                    var parent = node.Parent;
                    var successorParent = successor.Parent;
                    var successorRight = successor.RightChild;

                    if (successorParent.LeftChild == successor) successorParent.LeftChild = successorRight;
                    else successorParent.RightChild = successorRight;

                    if (successorRight != null) successorRight.Parent = successorParent;

                    successor.Parent = parent;
                    successor.LeftChild = left;
                    successor.BalanceFactor = node.BalanceFactor;
                    successor.RightChild = right;
                    right.Parent = successor;
                    left.Parent = successor;

                    if (node == Root) Root = successor;
                    else
                    {
                        if (parent.LeftChild == node) parent.LeftChild = successor;
                        else parent.RightChild = successor;
                    }
                    DeleteBalance(successorParent, -1);
                }
            }
            _count--;
            _version++;
        }
    }
}
