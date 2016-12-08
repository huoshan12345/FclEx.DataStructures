using System.Collections.Generic;

namespace FclEx.Node
{
    public abstract class BinaryNode<T, TNode> : Node<T, TNode> where TNode : BinaryNode<T, TNode>
    {
        public bool OnlyHasLeftChild => LeftChild != null && RightChild == null;
        public bool OnlyHasRightChild => LeftChild == null && RightChild != null;
        public bool OnlyHasOneChild => OnlyHasLeftChild || OnlyHasRightChild;
        public bool HasLeftChild => LeftChild != null;
        public bool HasRightChild => RightChild != null;
        public bool IsLeafNode => LeftChild == null && RightChild == null;
        public bool HasTwoChildren => LeftChild != null && RightChild != null;

        public TNode LeftChild
        {
            get { return Neighbors[0]; }
            set { Neighbors[0] = value; }
        }
        public TNode RightChild
        {
            get { return Neighbors[1]; }
            set { Neighbors[1] = value; }
        }
        public TNode Parent
        {
            get { return Neighbors[2]; }
            set { Neighbors[2] = value; }
        }

        public TNode[] Children => Neighbors;

        protected BinaryNode(T item, TNode left = null, TNode right = null, TNode parent = null)
            : base(3, item)
        {
            LeftChild = left;
            RightChild = right;
            Parent = parent;
        }

        public static IEnumerable<TNode> PreOrderTraverse(TNode node)
        {
            if (node == null) yield break;
            yield return node;
            foreach (var n in PreOrderTraverse(node.LeftChild))
            {
                yield return n;
            }
            foreach (var n in PreOrderTraverse(node.RightChild))
            {
                yield return n;
            }
        }

        public static IEnumerable<TNode> InOrderTraverse(TNode node)
        {
            if (node == null) yield break;
            foreach (var n in InOrderTraverse(node.LeftChild))
            {
                yield return n;
            }
            yield return node;
            foreach (var n in InOrderTraverse(node.RightChild))
            {
                yield return n;
            }
        }

        public static IEnumerable<TNode> PostOrderTraverse(TNode node)
        {
            if (node == null) yield break;
            foreach (var n in PostOrderTraverse(node.LeftChild))
            {
                yield return n;
            }
            foreach (var n in PostOrderTraverse(node.RightChild))
            {
                yield return n;
            }
            yield return node;
        }

        public static IEnumerable<TNode> LayerTraverse(TNode node)
        {
            if (node == null) yield break;
            var queue = new Queue<TNode>();
            queue.Enqueue(node);
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                yield return item;
                if (item.LeftChild != null) queue.Enqueue(item.LeftChild);
                if (item.RightChild != null) queue.Enqueue(item.RightChild);
            }
        }
    }
}
