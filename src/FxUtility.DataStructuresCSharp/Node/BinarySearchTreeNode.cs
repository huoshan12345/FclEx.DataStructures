using System.Collections.Generic;

namespace FxUtility.Node
{
    public class BinarySearchTreeNode<TKey, TValue> : BaseBinaryTreeNode<KeyValuePair<TKey, TValue>, BinarySearchTreeNode<TKey, TValue>>
    {
        public BinarySearchTreeNode(TKey key, TValue value, BinarySearchTreeNode<TKey, TValue> left = null,
            BinarySearchTreeNode<TKey, TValue> right = null, BinarySearchTreeNode<TKey, TValue> parent = null)
            : this(new KeyValuePair<TKey, TValue>(key, value), left, right, parent) { }

        public BinarySearchTreeNode(KeyValuePair<TKey, TValue> item, BinarySearchTreeNode<TKey, TValue> left = null,
            BinarySearchTreeNode<TKey, TValue> right = null, BinarySearchTreeNode<TKey, TValue> parent = null)
        : base(item, left, right, parent) { }
    }
}
