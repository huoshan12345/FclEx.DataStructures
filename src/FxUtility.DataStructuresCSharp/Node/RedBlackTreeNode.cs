using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataStructuresCSharp.Node
{
    public enum RedBlackTreeNodeColor : byte
    {
        Red,
        Black
    }

    public class RedBlackTreeNode<TKey, TValue> : BaseBinaryTreeNode<KeyValuePair<TKey, TValue>, RedBlackTreeNode<TKey, TValue>>
    {
        public bool IsRed => Color == RedBlackTreeNodeColor.Red;
        public bool IsBlack => Color == RedBlackTreeNodeColor.Black;
        public RedBlackTreeNodeColor Color { get; set; }

        public RedBlackTreeNode(TKey key, TValue value, RedBlackTreeNodeColor color = RedBlackTreeNodeColor.Black, 
            RedBlackTreeNode<TKey, TValue> left = null, RedBlackTreeNode<TKey, TValue> right = null, RedBlackTreeNode<TKey, TValue> parent = null)
            : this(new KeyValuePair<TKey, TValue>(key, value), color, left, right, parent) { }

        public RedBlackTreeNode(KeyValuePair<TKey, TValue> item, RedBlackTreeNodeColor color = RedBlackTreeNodeColor.Black, 
            RedBlackTreeNode<TKey, TValue> left = null, RedBlackTreeNode<TKey, TValue> right = null, RedBlackTreeNode<TKey, TValue> parent = null)
            : base(item, left, right, parent)
        {
            Color = color;
        }

        public void SetBlack() => Color = RedBlackTreeNodeColor.Black;
        public void SetRed() => Color = RedBlackTreeNodeColor.Red;
    }
}
