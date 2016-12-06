using FclEx.Extensions;

namespace FclEx.Node
{
    public abstract class MultiNode<T, TNode> where TNode : MultiNode<T, TNode>
    {
        public T[] Items { get; set; }
        protected TNode[] Neighbors { get; set; }

        protected MultiNode(int neighborNum, int itemNum)
        {
            Items = new T[itemNum];
            Neighbors = new TNode[neighborNum];
        }

        public virtual void Invalidate()
        {
            Items.Clear();
            Neighbors.Clear();
            Neighbors = null;
            Items = null;
        }
    }
}
