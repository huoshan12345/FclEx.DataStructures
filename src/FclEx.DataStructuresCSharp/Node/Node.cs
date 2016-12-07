using System;

namespace FclEx.Node
{
    public abstract class Node<T, TNode> where TNode : Node<T, TNode>
    {
        public T Item { get; set; }
        protected TNode[] Neighbors { get; set; }

        protected Node(int neighborNum, T item)
        {
            Item = item;
            Neighbors = new TNode[neighborNum];
        }

        public virtual void Invalidate()
        {
            Item = default(T);
            Array.Clear(Neighbors, 0, Neighbors.Length);
            Neighbors = null;
        }
    }
}
