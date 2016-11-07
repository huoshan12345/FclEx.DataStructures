using System;
using System.Collections.Generic;

namespace FxUtility.Collections
{
    public class SeqList<T> : BaseSeqCollection<T>, ILinearList<T>
    {
        public SeqList() : base()
        {
        }

        public SeqList(int capacity) : base(capacity)
        {
        }

        public SeqList(IEnumerable<T> collection) : base(collection)
        {
        }

        public int IndexOf(T item)
        {
            return IndexOfInternal(item, 0, _size);
        }

        public void Insert(int index, T item)
        {
            // index can be equal to the Size
            if (index < 0 || index > _size) throw new ArgumentOutOfRangeException(nameof(index));
            if (_size == _items.Length)
            {
                Array.Resize(ref _items, _items.Length == 0 ? DefaultCapacity : 2 * _items.Length);
            }
            if (index < _size) Array.Copy(_items, index, _items, index + 1, _size - index);
            _items[index] = item;
            ++_size;
            ++_tail;
            ++_version;
        }

        public void RemoveAt(int index)
        {
            RemoveAtInternal(index);
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException(nameof(index));
                _items[index] = value;
                ++_version;
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            AddRangeInternal(collection);
        }

        public int IndexOf(T item, int index)
        {
            return IndexOfInternal(item, index, _size - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            return IndexOfInternal(item, index, count);
        }

        public int LastIndexOf(T item)
        {
            return LastIndexOfInternal(item, _size - 1, _size);
        }

        public int LastIndexOf(T item, int index)
        {
            if (index >= _size) throw new ArgumentOutOfRangeException(nameof(index));
            return LastIndexOfInternal(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            return LastIndexOfInternal(item, index, count);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            InsertRangeInternal(index, collection);
        }

        public void Reverse()
        {
            ReverseInternal(0, _size);
        }

        public void Reverse(int index, int count)
        {
            ReverseInternal(index, count);
        }

        public void RemoveRange(int index, int count)
        {
            RemoveRangeInternal(index, count);
        }
        
        public bool Remove(T item)
        {
            var index = IndexOfInternal(item, 0, _size);
            if (index < 0) return false;
            RemoveAtInternal(index);
            ++_version;
            return true;
        }

        public bool IsReadOnly => false;
    }
}
