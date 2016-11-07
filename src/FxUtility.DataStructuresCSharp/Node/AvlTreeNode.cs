using System.Collections.Generic;

namespace DataStructuresCSharp.Node
{
    public class AvlTreeNode<TKey, TValue> : BaseBinaryTreeNode<KeyValuePair<TKey, TValue>, AvlTreeNode<TKey, TValue>>
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
}
