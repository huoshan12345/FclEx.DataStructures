using System;
using System.Collections.Generic;
using DataStructuresCSharp.Node;

namespace FxUtility.Collections
{
    public class RedBlackTree<TKey, TValue> : BaseBinarySearchTree<TKey, TValue, RedBlackTreeNode<TKey, TValue>>
    {
        public RedBlackTree(IComparer<TKey> comparer = null) : base(comparer) { }

        public RedBlackTree(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer = null) : base(dictionary, comparer) { }

        /*
         * 对红黑树的节点(x)进行左旋转
         *
         * 左旋示意图(对节点x进行左旋)：
         *      px                              px
         *     /                               /
         *    x                               y
         *   /  \      --(左旋)-.           / \                #
         *  lx   y                         x  ry
         *     /   \                      /    \
         *    ly   ry                    lx    ly
         */
        private void LeftRotate(RedBlackTreeNode<TKey, TValue> x)
        {
            var y = x.RightChild;
            x.RightChild = y.LeftChild;
            if (y.LeftChild != null) y.LeftChild.Parent = x;
            y.Parent = x.Parent;
            if (x == Root) Root = y; // 情况1
            else if (x == x.Parent.LeftChild) x.Parent.LeftChild = y;// 情况2
            else x.Parent.RightChild = y; // 情况3
            y.LeftChild = x;
            x.Parent = y;
        }

        /*
         * 对红黑树的节点(y)进行右旋转
         *
         * 右旋示意图(对节点y进行左旋)：
         *            py                               py
         *           /                                /
         *          y                                x
         *         /  \      --(右旋)-.            /  \                     #
         *        x   ry                          lx   y
         *       / \                                  / \                   #
         *      lx  rx                               rx  ry
         */
        private void RightRotate(RedBlackTreeNode<TKey, TValue> y)
        {
            var x = y.LeftChild;
            y.LeftChild = x.RightChild;
            if (x.RightChild != null) x.RightChild.Parent = y;
            x.Parent = y.Parent;
            if (y == Root) Root = x; // 情况1
            else if (y == y.Parent.RightChild) y.Parent.RightChild = x;// 情况2
            else y.Parent.LeftChild = x; // 情况3
            x.RightChild = y;
            y.Parent = x;
        }

        public override void Add(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
            var node = new RedBlackTreeNode<TKey, TValue>(item);
            if (Root == null) Root = node;
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
                        node.SetRed();
                        InsertFixUp(node);
                        break;
                    }
                }
            }
            _version++;
            _count++;
        }

        /// <summary>
        /// 红黑树插入修正函数
        /// 在向红黑树中插入节点之后(失去平衡)，再调用该函数；
        /// 目的是将它重新塑造成一颗红黑树。
        /// </summary>
        /// <param name="node"></param>
        private void InsertFixUp(RedBlackTreeNode<TKey, TValue> node)
        {
            while (node?.Parent != null && node.Parent.IsRed)
            {
                var parent = node.Parent;
                var gParent = parent.Parent;
                if (parent == gParent.LeftChild) // 若“父节点”是“祖父节点的左孩子”
                {
                    var uncle = gParent.RightChild;
                    if ((uncle != null) && uncle.IsRed) // Case 1条件：叔叔节点是红色
                    {
                        uncle.SetBlack();
                        parent.SetBlack();
                        gParent.SetRed();
                        node = gParent;
                        continue;
                    }
                    else if (parent.RightChild == node) // Case 2条件：叔叔是黑色，且当前节点是右孩子
                    {
                        LeftRotate(parent);
                        var tmp = parent;
                        parent = node;
                        node = tmp;
                    }

                    // Case 3条件：叔叔是黑色，且当前节点是左孩子。
                    parent.SetBlack();
                    gParent.SetRed();
                    RightRotate(gParent);
                }
                else
                {    //若“z的父节点”是“z的祖父节点的右孩子”
                     // Case 1条件：叔叔节点是红色
                    var uncle = gParent.LeftChild;
                    if ((uncle != null) && uncle.IsRed)
                    {
                        uncle.SetBlack();
                        parent.SetBlack();
                        gParent.SetRed();
                        node = gParent;
                        continue;
                    }

                    // Case 2条件：叔叔是黑色，且当前节点是左孩子
                    if (parent.LeftChild == node)
                    {
                        RightRotate(parent);
                        var tmp = parent;
                        parent = node;
                        node = tmp;
                    }

                    // Case 3条件：叔叔是黑色，且当前节点是右孩子。
                    parent.SetBlack();
                    gParent.SetRed();
                    LeftRotate(gParent);
                }
            }
            Root.SetBlack();
        }

        protected override void RemoveNode(RedBlackTreeNode<TKey, TValue> node)
        {
            // 被删除节点的"左右孩子都不为空"的情况。
            if (node.HasTwoChildren)
            {
                // 被删节点的后继节点。(称为"取代节点")
                // 用它来取代"被删节点"的位置，然后再将"被删节点"去掉。
                var replace = node;

                // 获取后继节点
                replace = replace.RightChild;
                while (replace.LeftChild != null)
                    replace = replace.LeftChild;

                // "node节点"不是根节点(只有根节点不存在父节点)
                if (node != Root)
                {
                    if (node.Parent.LeftChild == node) node.Parent.LeftChild = replace;
                    else node.Parent.RightChild = replace;
                }
                else Root = replace;// "node节点"是根节点，更新根节点。

                // child是"取代节点"的右孩子，也是需要"调整的节点"。
                // "取代节点"肯定不存在左孩子！因为它是一个后继节点。
                var child = replace.RightChild;
                var parent = replace.Parent;
                // 保存"取代节点"的颜色
                var color = replace.Color;

                // "被删除节点"是"它的后继节点的父节点"
                if (parent == node) parent = replace;
                else
                {
                    // child不为空
                    if (child != null) child.Parent = parent;
                    parent.LeftChild = child;

                    replace.RightChild = node.RightChild;
                    if (node.RightChild != null) node.RightChild.Parent = replace;
                }

                replace.Parent = node.Parent;
                replace.Color = node.Color;
                replace.LeftChild = node.LeftChild;
                node.LeftChild.Parent = replace;

                if (color == RedBlackTreeNodeColor.Black)
                    RemoveFixUp(child, parent);
            }
            else
            {
                var child = node.LeftChild ?? node.RightChild;
                var parent = node.Parent;
                // 保存"取代节点"的颜色
                var color = node.Color;
                if (child != null) child.Parent = parent;

                // "node节点"不是根节点
                if (parent != null)
                {
                    if (parent.LeftChild == node) parent.LeftChild = child;
                    else parent.RightChild = child;
                }
                else Root = child;

                if (color == RedBlackTreeNodeColor.Black)
                    RemoveFixUp(child, parent);
            }
            node.Invalidate();
            --_count;
            ++_version;
        }

        /// <summary>
        /// 红黑树删除修正函数
        /// 在从红黑树中删除插入节点之后(红黑树失去平衡)，再调用该函数；
        /// 目的是将它重新塑造成一颗红黑树。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        private void RemoveFixUp(RedBlackTreeNode<TKey, TValue> node, RedBlackTreeNode<TKey, TValue> parent)
        {
            while ((node == null || node.IsBlack) && (node != Root))
            {
                if (parent.LeftChild == node)
                {
                    var other = parent.RightChild;
                    if (other.IsRed)
                    {
                        // Case 1: x的兄弟w是红色的  
                        other.SetBlack();
                        parent.SetRed();
                        LeftRotate(parent);
                        other = parent.RightChild;
                    }

                    if ( (other.LeftChild == null || other.LeftChild.IsBlack) 
                        && (other.RightChild == null || other.RightChild.IsBlack) )
                    {
                        // Case 2: x的兄弟w是黑色，且w的俩个孩子也都是黑色的  
                        other.SetRed();
                        node = parent;
                        parent = node.Parent;
                    }
                    else
                    {
                        if (other.RightChild == null || other.RightChild.IsBlack)
                        {
                            // Case 3: x的兄弟w是黑色的，并且w的左孩子是红色，右孩子为黑色。 
                            other.LeftChild.SetBlack();
                            other.SetRed(); ;
                            RightRotate(other);
                            other = parent.RightChild;
                        }
                        // Case 4: x的兄弟w是黑色的；并且w的右孩子是红色的，左孩子任意颜色。
                        other.Color = parent.Color;
                        parent.SetBlack();
                        other.RightChild.SetBlack();
                        LeftRotate(parent);
                        node = Root;
                        break;
                    }
                }
                else
                {
                    var other = parent.LeftChild;
                    if (other.IsRed)
                    {
                        // Case 1: x的兄弟w是红色的  
                        other.SetBlack();
                        parent.SetRed();
                        RightRotate(parent);
                        other = parent.LeftChild;
                    }

                    if ( (other.LeftChild == null || other.LeftChild.IsBlack) 
                        && (other.RightChild == null || other.RightChild.IsBlack) )
                    {
                        // Case 2: x的兄弟w是黑色，且w的俩个孩子也都是黑色的  
                        other.SetRed();
                        node = parent;
                        parent = node.Parent;
                    }
                    else
                    {

                        if (other.LeftChild == null || other.LeftChild.IsBlack)
                        {
                            // Case 3: x的兄弟w是黑色的，并且w的左孩子是红色，右孩子为黑色。  
                            other.RightChild.SetBlack();
                            other.SetRed();
                            LeftRotate(other);
                            other = parent.LeftChild;
                        }

                        // Case 4: x的兄弟w是黑色的；并且w的右孩子是红色的，左孩子任意颜色。
                        other.Color = parent.Color;
                        parent.SetBlack();
                        other.LeftChild.SetBlack();
                        RightRotate(parent);
                        node = Root;
                        break;
                    }
                }
            }
            node?.SetBlack();
        }
    }
}
