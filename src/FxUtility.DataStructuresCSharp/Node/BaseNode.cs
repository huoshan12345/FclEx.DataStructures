using System;

namespace FxUtility.Node
{
    public abstract class BaseNode<T, TNode> where TNode : BaseNode<T, TNode>
    {
        public T Item { get; set; }
        protected TNode[] NeighborNodes { get; set; }

        protected BaseNode(int neighborNodesNum, T item)
        {
            Item = item;
            NeighborNodes = new TNode[neighborNodesNum];
        }

        public virtual void Invalidate()
        {
            Item = default(T);
            Array.Clear(NeighborNodes, 0, NeighborNodes.Length);
            NeighborNodes = null;
        }
    }
}
