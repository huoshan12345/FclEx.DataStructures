namespace DataStructuresCSharp.Node
{
    public abstract class BaseBinaryTreeNode<T, TNode> : BaseNode<T, TNode> where TNode : BaseNode<T, TNode>
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
            get { return NeighborNodes[0]; }
            set { NeighborNodes[0] = value; }
        }
        public TNode RightChild
        {
            get { return NeighborNodes[1]; }
            set { NeighborNodes[1] = value; }
        }

        public TNode Parent
        {
            get { return NeighborNodes[2]; }
            set { NeighborNodes[2] = value; }
        }

        public TNode[] Children => NeighborNodes;

        protected BaseBinaryTreeNode(T item, TNode left = null, TNode right = null, TNode parent = null)
            : base(3, item)
        {
            LeftChild = left;
            RightChild = right;
            Parent = parent;
        }
    }
}
