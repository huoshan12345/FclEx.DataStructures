using System;
using System.Collections;
using System.Collections.Generic;
using DataStructuresCSharp.Util;

namespace FxUtility.Collections
{
    public class MaxHeap<T> : ILinearCollection<T>
    {
        private const int DefaultCapacity = 4;

        private T[] _items;
        private int _size;
        private int _version;
        protected readonly IComparer<T> _comparer;

        public MaxHeap(IEnumerable<T> collection, IComparer<T> comparer = null) : this(DefaultCapacity, comparer)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            foreach (var item in collection)
            {
                Push(item);
            }
        }

        public MaxHeap() : this(DefaultCapacity, null)
        {
        }

        public MaxHeap(int capacity, IComparer<T> comparer)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (comparer == null) comparer = Comparer<T>.Default;
            _items = new T[capacity];
            _size = 0;
            _version = 0;
            _comparer = comparer;
        }

        private void TrimExcess()
        {
            if (_size < _items.Length / 2)
            {
                Array.Resize(ref _items, _size);
                ++_version;
            }
        }

        public int Count => _size;

        public void Clear()
        {
            if (_size != 0)
            {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }
            ++_version;
        }

        public T Peek()
        {
            if (_size == 0) throw new InvalidOperationException();
            return _items[0];
        }

        public T Pop()
        {
            if (_size == 0) throw new InvalidOperationException();
            var item = _items[0];
            _items[0] = _items[_size - 1]; //使用最后一个节点来代替当前结点，然后再向下调整当前结点。
            ShiftDown(0, _size--);
            ++_version;
            return item;
        }

        public void Add(T item) => Push(item);

        public void Push(T item)
        {
            if (_size == _items.Length) Array.Resize(ref _items, _items.Length == 0 ? DefaultCapacity : 2 * _items.Length);
            _items[_size] = item;
            ShiftUp(_size);
            ++_size;
            ++_version;
        }

        private void ShiftUp(int index)
        {
            var item = _items[index];
            while (index > 0) // 如果还未到达根节点，继续调整
            {
                var parentIndex = (index - 1) >> 1;  // 求其双亲节点
                if (_comparer.Compare(item, _items[parentIndex]) < 0) break;
                _items[index] = _items[parentIndex];
                index = parentIndex;
            }
            _items[index] = item;    // 插入最后的位置
        }

        private void ShiftDown(int current, int length)
        {
            //  var value = _items[current];
            while ((current << 1) + 1 < length)
            {
                var child = (current << 1) + 1;
                if (child + 1 < length && _comparer.Compare(_items[child], _items[child + 1]) < 0) ++child;
                if (_comparer.Compare(_items[current], _items[child]) > 0) break;
                // _items[current] = _items[child];
                Helper.Swap(ref _items[current], ref _items[child]);
                current = child;
            }
            // _items[current] = value;
        }

        public virtual IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(T item)
        {
            var c = EqualityComparer<T>.Default;
            for (var i = 0; i < _size; i++)
            {
                if (_comparer.Compare(_items[i], item) == 0) return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _size + arrayIndex) throw new ArgumentException();
            if (_size == 0) return;
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly MaxHeap<T> _collection;
            private int _index;
            private readonly int _version;

            internal Enumerator(MaxHeap<T> collection)
            {
                _collection = collection;
                _index = -1;
                _version = collection._version;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (_version != _collection._version) throw new InvalidOperationException();
                if (_index + 1 > _collection._size) return false;
                ++_index;
                return _index != _collection._size;
            }

            public T Current
            {
                get
                {
                    if (_index == -1 || _index >= _collection._size) throw new InvalidOperationException();
                    return _collection._items[_index];
                }
            }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                if (_version != _collection._version) throw new InvalidOperationException();
                _index = -1;
            }
        }
    }
}
