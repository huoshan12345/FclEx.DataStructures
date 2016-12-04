using System;

namespace FxUtility.Node
{
    public abstract class BaseNode<T, TNode> where TNode : BaseNode<T, TNode>
    {
        public T Item { get; set; }
        protected int NeighborNodesNum { get; }
        protected TNode[] NeighborNodes { get; set; }

        protected BaseNode(int neighborNodesNum, T item)
        {
            Item = item;
            NeighborNodesNum = neighborNodesNum;
            NeighborNodes = new TNode[NeighborNodesNum];
        }

        public virtual void Invalidate()
        {
            Item = default(T);
            Array.Clear(NeighborNodes, 0, NeighborNodes.Length);
            NeighborNodes = null;
        }
    }
}
