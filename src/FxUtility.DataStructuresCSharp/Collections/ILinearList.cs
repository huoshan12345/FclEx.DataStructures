using System.Collections.Generic;

namespace FclEx.Collections
{
    public interface ILinearList<T> : IList<T>
    {
        void AddRange(IEnumerable<T> collection);
        int IndexOf(T item, int index);
        int IndexOf(T item, int index, int count);
        int LastIndexOf(T item);
        int LastIndexOf(T item, int index);
        int LastIndexOf(T item, int index, int count);
        void InsertRange(int index, IEnumerable<T> collection);
        void Reverse();
        void Reverse(int index, int count);
        void RemoveRange(int index, int count);
    }
}
