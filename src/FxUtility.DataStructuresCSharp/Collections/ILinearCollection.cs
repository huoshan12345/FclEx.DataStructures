using System.Collections.Generic;

namespace FclEx.Collections
{
    public interface ILinearCollection<T> : IReadOnlyCollection<T>
    {
        void Add(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
    }
}
