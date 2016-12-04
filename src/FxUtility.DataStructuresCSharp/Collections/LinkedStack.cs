using System;
using System.Collections;
using System.Collections.Generic;
using FxUtility.Node;

namespace FxUtility.Collections
{
    public class LinkedStack<T> : IStack<T>, ILinearCollection<T>
    {
        private readonly SingleLinkedListNode<T> _top;
        private int _count;
        private int _version;

        public LinkedStack()
        {
            _top = new SingleLinkedListNode<T>();
            _count = 0;
            _version = 0;
        }

        public LinkedStack(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            _top = new SingleLinkedListNode<T>();
            _count = 0;
            foreach (var item in collection)
            {
                _top.Next = new SingleLinkedListNode<T>(item, _top.Next);
                ++_count;
            }
            _version = 0;
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
            Push(item);
            ++_version;
        }

        public void Clear()
        {
            var p = _top.Next;
            while (p != null)
            {
                var q = p;
                p = p.Next;
                q.Invalidate();
            }
            _count = 0;
            _top.Next = null;
            ++_version;
        }

        public bool Contains(T item)
        {
            return Find(item) != null;
        }

        private SingleLinkedListNode<T> Find(T item)
        {
            var p = _top.Next;
            var c = EqualityComparer<T>.Default;
            while (p != null)
            {
                if (c.Equals(p.Item, item)) return p;
                p = p.Next;
            }
            return null;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _count + arrayIndex) throw new ArgumentException(nameof(array));

            var p = _top.Next;
            while (p != null)
            {
                array[arrayIndex++] = p.Item;
                p = p.Next;
            }
        }

        public int Count => _count;
        
        public void Push(T item)
        {
            _top.Next = new SingleLinkedListNode<T>(item, _top.Next);
            ++_count;
            ++_version;
        }

        public T Pop()
        {
            if (_count == 0) throw new InvalidOperationException("Stack is empty.");
            var node = _top.Next;
            _top.Next = node.Next;
            --_count;
            ++_version;
            return node.Item;
        }

        public T Peek()
        {
            if (_count == 0) throw new InvalidOperationException("Stack is empty.");
            return _top.Next.Item;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly LinkedStack<T> _stack;
            private SingleLinkedListNode<T> _node;
            private readonly int _version;
            private T _current;
            private int _index;

            internal Enumerator(LinkedStack<T> stack)
            {
                _stack = stack;
                _version = stack._version;
                _node = stack._top.Next;
                _current = default(T);
                _index = 0;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_version != _stack._version) throw new InvalidOperationException();
                if (_node == null)
                {
                    _index = _stack.Count + 1;
                    _current = default(T);
                    return false;
                }
                ++_index;
                _current = _node.Item;
                _node = _node.Next;
                return true;
            }

            public void Reset()
            {
                if (_version != _stack._version) throw new InvalidOperationException();
                _current = default(T);
                _node = _stack._top.Next;
                _index = 0;
            }

            public T Current
            {
                get
                {
                    if (_index == 0 || (_index == _stack.Count + 1)) throw new InvalidOperationException();
                    return _current;
                }
            }

            object IEnumerator.Current => Current;
        }
    }
}
