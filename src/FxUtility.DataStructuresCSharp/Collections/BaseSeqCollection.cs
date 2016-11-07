using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FxUtility.Collections
{
    public abstract class BaseSeqCollection<T> : ILinearCollection<T>
    {
        protected virtual int DefaultCapacity { get; } = 4;
        protected virtual bool IsEnumerateOrderInverted { get; } = false;
        protected T[] _items;
        protected int _size;
        protected int _head;
        protected int _tail;
        protected int _version;

        protected BaseSeqCollection() : this(0)
        {
        }

        protected BaseSeqCollection(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _items = new T[capacity];
            _size = 0;
            _head = 0;
            _tail = -1;
            _version = 0;
        }

        protected BaseSeqCollection(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            _items = collection.ToArray();
            _size = _items.Length;
            _head = 0;
            _tail = _size - 1;
            _version = 0;
        }

        public void TrimExcess()
        {
            int threshold = (int)(((double)_items.Length) * 0.9);
            if (_size < threshold)
            {
                Array.Copy(_items, _head, _items, 0, _size);
                _head = 0;
                _tail = _size - 1;
                Array.Resize(ref _items, _size);
                ++_version;
            }
        }

        public virtual int Count => _size;

        public virtual IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Add(T item)
        {
            if (_size == _items.Length)
            {
                Array.Resize(ref _items, _items.Length == 0 ? DefaultCapacity : 2 * _items.Length);
            }
            else if (_tail + 1 == _items.Length)
            {
                Array.Copy(_items, _head, _items, 0, _size);
                _head = 0;
                _tail = _size - 1;
            }
            _items[++_tail] = item;
            ++_size;
            ++_version;
        }

        public virtual void Clear()
        {
            if (_size != 0)
            {
                Array.Clear(_items, _head, _size);
                _size = 0;
            }
            _head = 0;
            _tail = -1;
            ++_version;
        }

        public virtual bool Contains(T item)
        {
            var c = EqualityComparer<T>.Default;
            for (var i = _head; i <= _tail; i++)
            {
                if (c.Equals(_items[i], item)) return true;
            }
            return false;
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _size + arrayIndex) throw new ArgumentException();
            if (_size == 0) return;
            Array.Copy(_items, _head, array, arrayIndex, _size);
            if (IsEnumerateOrderInverted) Array.Reverse(array, arrayIndex, _size);
        }


        internal int IndexOfInternal(T item, int index, int count)
        {
            if ((uint)index > (uint)_size) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > _size) throw new ArgumentOutOfRangeException(nameof(count));
            return Array.IndexOf(_items, item, _head + index, count);
        }

        internal int LastIndexOfInternal(T item, int index, int count)
        {
            if (index < 0 && _size != 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 && _size != 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_size == 0) return -1; // LastIndexOf with a 0 count List is special cased to return -1. 
            if (index >= _size) throw new ArgumentOutOfRangeException(nameof(index));
            if (count > index + 1) throw new ArgumentOutOfRangeException(nameof(count));
            return Array.LastIndexOf(_items, item, _head + index, count);
        }

        internal void RemoveAtInternal(int index)
        {
            if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index));
            --_size;
            --_tail;
            if (index < _size) Array.Copy(_items, index + 1, _items, index, _size - index);
            _items[_size] = default(T);
            ++_version;
        }

        internal void ReverseInternal(int index, int count)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_size < count + index) throw new ArgumentException();

            // The non-generic Array.Reverse is not used because it does not perform
            // well for non-primitive value types.
            // If/when a generic Array.Reverse<T> becomes available, the below code
            // can be deleted and replaced with a call to Array.Reverse<T>.
            var startIndex = index + _head;
            var i = startIndex;
            var j = startIndex + count - 1;
            var array = _items;
            while (i < j)
            {
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
                i++;
                j--;
            }
            ++_version;
        }

        internal void InsertRangeInternal(int index, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if ((uint)index > (uint)_size) throw new ArgumentOutOfRangeException(nameof(index));
            var array = collection.ToArray();
            var count = array.Length;
            if (count == 0) return;

            if (_size + count > _items.Length) Array.Resize(ref _items, _size + count);
            else if (_tail + count >= _items.Length)
            {
                Array.Copy(_items, _head, _items, 0, _size);
                _head = 0;
                _tail = _size - 1;
            }

            var startIndex = index + _head;
            if (index < _size) Array.Copy(_items, startIndex, _items, startIndex + count, _size - index);
            if (ReferenceEquals(this, collection)) // If we're inserting a List into itself, we want to be able to deal with that.
            {
                // Copy first part of _items to insert location
                Array.Copy(_items, 0, _items, startIndex, index);
                // Copy last part of _items back to inserted location
                Array.Copy(_items, startIndex + count, _items, startIndex + index, _size - index);
            }
            else
            {
                var itemsToInsert = new T[count];
                array.CopyTo(itemsToInsert, 0);
                Array.Copy(itemsToInsert, 0, _items, startIndex, count);
            }
            _size += count;
            _tail += count;
            ++_version;
        }

        internal void RemoveRangeInternal(int index, int count)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_size < count + index) throw new ArgumentException();
            if (count == 0) return;

            _size -= count;
            if (index < _size) Array.Copy(_items, index + count, _items, index, _size - index);
            Array.Clear(_items, _size, count);
            ++_version;
        }

        internal void AddRangeInternal(IEnumerable<T> collection)
        {
            InsertRangeInternal(_size, collection);
        }

        internal void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_size < count + index) throw new ArgumentException();
            Array.Sort<T>(_items, _head + index, count, comparer);
            ++_version;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly BaseSeqCollection<T> _collection;
            private int _index;
            private readonly int _version;
            private T _current;

            internal Enumerator(BaseSeqCollection<T> collection)
            {
                _collection = collection;
                _index = 0;
                _version = collection._version;
                _current = default(T);
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                var collection = _collection;
                if (_version == collection._version && ((uint)_index < (uint)collection._size))
                {
                    var index = _collection.IsEnumerateOrderInverted
                        ? (_collection._tail - _index) : (_index + collection._head);
                    _current = collection._items[index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _collection._version) throw new InvalidOperationException();
                _index = _collection._size + 1;
                _current = default(T);
                return false;
            }

            public T Current
            {
                // 此处发现了一个System.Collections.Generic.List的一个bug
                get
                {
                    if (_index == 0 || _index == _collection._size + 1)
                    {
                        throw new InvalidOperationException();
                    }
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                if (_version != _collection._version) throw new InvalidOperationException();
                _index = 0;
                _current = default(T);
            }
        }
    }
}
