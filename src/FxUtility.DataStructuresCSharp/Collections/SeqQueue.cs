using System;
using System.Collections.Generic;

namespace FxUtility.Collections
{
    public class SeqQueue<T> : BaseSeqCollection<T>, IQueue<T>
    {
        public SeqQueue() : base()
        {
        }

        public SeqQueue(int capacity) : base(capacity)
        {
        }

        public SeqQueue(IEnumerable<T> collection) : base(collection)
        {
        }

        public void Enqueue(T item)
        {
            Add(item);
            ++_version;
        }

        public T Dequeue()
        {
            if (_size == 0) throw new InvalidOperationException("Queue is empty.");
            var item = _items[_head++];
            --_size;
            ++_version;
            return item;
        }

        public T Peek()
        {
            if (_size == 0) throw new InvalidOperationException("Queue is empty.");
            return _items[_head];
        }

    }
}
