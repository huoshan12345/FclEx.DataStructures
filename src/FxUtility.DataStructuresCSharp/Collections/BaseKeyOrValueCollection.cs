using System;
using System.Collections;
using System.Collections.Generic;

namespace FxUtility.Collections
{
    public abstract class BaseKeyOrValueCollection<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public abstract bool Contains(T item);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public bool IsReadOnly => true;

        public abstract int Count { get; }
    }
}
