namespace DataStructuresCSharp.Node
{
    public class DoublyLinkedListNode<T>: BaseNode<T, DoublyLinkedListNode<T>>
    {
        public DoublyLinkedListNode<T> Next
        {
            get { return NeighborNodes[0]; }
            set { NeighborNodes[0] = value; }
        }
        public DoublyLinkedListNode<T> Prev
        {
            get { return NeighborNodes[1]; }
            set { NeighborNodes[1] = value; }
        }

        public DoublyLinkedListNode() : this(default(T), null, null) { }

        public DoublyLinkedListNode(T item):this(item, null, null) { }

        public DoublyLinkedListNode(T item, DoublyLinkedListNode<T> prev, DoublyLinkedListNode<T> next)
            : base(2, item)
        {
            Next = next;
            Prev = prev;
        }

        public static DoublyLinkedListNode<T> operator ++(DoublyLinkedListNode<T> node)
        {
            node = node.Next;
            return node;
        }

        public static DoublyLinkedListNode<T> operator --(DoublyLinkedListNode<T> node)
        {
            node = node.Prev;
            return node;
        }
    }
}
