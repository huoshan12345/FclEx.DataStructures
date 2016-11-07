using System;
using System.Collections.Generic;

namespace FxUtility.Collections
{
    public class SeqStack<T> : BaseSeqCollection<T>, IStack<T>
    {
        public SeqStack() : base()
        {
        }

        public SeqStack(int capacity) : base(capacity)
        {
        }

        public SeqStack(IEnumerable<T> collection) : base(collection)
        {
        }

        protected override bool IsEnumerateOrderInverted => true;

        public void Push(T item)
        {
            Add(item);
            ++_version;
        }

        public T Pop()
        {
            if (_size == 0) throw new InvalidOperationException("Stack is empty.");
            var item = _items[_tail];
            _items[_tail] = default(T);     // Free memory quicker.
            --_tail;
            --_size;
            ++_version;
            return item;
        }

        public T Peek()
        {
            if (_size == 0) throw new InvalidOperationException("Stack is empty.");
            return _items[_tail];
        }
    }
}
