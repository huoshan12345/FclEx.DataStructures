namespace FclEx.Node
{
    public class SingleLinkedListNode<T> : Node<T, SingleLinkedListNode<T>>
    {
        public SingleLinkedListNode<T> Next
        {
            get { return Neighbors[0]; }
            set { Neighbors[0] = value; }
        }

        public SingleLinkedListNode() : this(default(T), null) { }

        public SingleLinkedListNode(T item):this(item, null) { }

        public SingleLinkedListNode(T item, SingleLinkedListNode<T> next) : base(1, item)
        {
            Next = next;
        }

        public static SingleLinkedListNode<T> operator ++(SingleLinkedListNode<T> node)
        {
            node = node.Next;
            return node;
        }
    }
}
