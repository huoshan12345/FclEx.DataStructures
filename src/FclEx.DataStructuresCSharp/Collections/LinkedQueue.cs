using System;
using System.Collections;
using System.Collections.Generic;
using FclEx.Node;

namespace FclEx.Collections
{
    public class LinkedQueue<T> : IQueue<T>, ILinearCollection<T>
    {
        private readonly SingleLinkedListNode<T> _head;
        private SingleLinkedListNode<T> _tail;
        private int _count;
        private int _version;

        public LinkedQueue()
        {
            _head = new SingleLinkedListNode<T>();
            _tail = _head;
            _count = 0;
            _version = 0;
        }

        public LinkedQueue(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            _head = new SingleLinkedListNode<T>();
            var p = _head;
            _count = 0;
            foreach (var item in collection)
            {
                var node = new SingleLinkedListNode<T>(item);
                p.Next = node;
                _tail = node;
                p = p.Next;
                ++_count;
            }
            _version = 0;
        }

        public void Enqueue(T item)
        {
            var node = new SingleLinkedListNode<T>(item);
            _tail.Next = node;
            _tail++;
            ++_count;
            ++_version;
        }

        public T Dequeue()
        {
            if (_count == 0) throw new InvalidOperationException();
            if (_count == 1) _tail = _head;
            var node = _head.Next;
            _head.Next = node.Next;
            --_count;
            ++_version;
            return node.Item;
        }

        public T Peek()
        {
            if (_count == 0) throw new InvalidOperationException();
            return _head.Next.Item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            var p = _head.Next;
            while (p != null)
            {
                var q = p;
                p++;
                q.Invalidate();
            }
            _count = 0;
            _head.Next = null;
            _tail = _head;
            ++_version;
        }

        private SingleLinkedListNode<T> Find(T item)
        {
            var p = _head.Next;
            var c = EqualityComparer<T>.Default;
            while (p != null)
            {
                if (c.Equals(p.Item, item)) return p;
                p = p.Next;
            }
            return null;
        }

        public bool Contains(T item)
        {
            return Find(item) != null;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _count + arrayIndex) throw new ArgumentException(nameof(array));

            var p = _head.Next;
            while (p != null)
            {
                array[arrayIndex++] = p.Item;
                p = p.Next;
            }
        }

        public int Count => _count;

        public struct Enumerator : IEnumerator<T>
        {
            private readonly LinkedQueue<T> _queue;
            private SingleLinkedListNode<T> _node;
            private readonly int _version;
            private T _current;
            private int _index;

            internal Enumerator(LinkedQueue<T> queue)
            {
                _queue = queue;
                _version = queue._version;
                _node = queue._head.Next;
                _current = default(T);
                _index = 0;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_version != _queue._version) throw new InvalidOperationException();
                if (_node == null)
                {
                    _index = _queue.Count + 1;
                    _current = default(T);
                    return false;
                }
                ++_index;
                _current = _node.Item;
                _node++;
                return true;
            }

            public void Reset()
            {
                if (_version != _queue._version) throw new InvalidOperationException();
                _current = default(T);
                _node = _queue._head.Next;
                _index = 0;
            }

            public T Current
            {
                get
                {
                    if (_index == 0 || (_index == _queue.Count + 1)) throw new InvalidOperationException();
                    return _current;
                }
            }

            object IEnumerator.Current => Current;
        }
    }
}
