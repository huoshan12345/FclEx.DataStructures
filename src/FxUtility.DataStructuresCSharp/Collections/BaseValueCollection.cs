using System;
using System.Collections;
using System.Collections.Generic;

namespace FxUtility.Collections
{
    public class BaseValueCollection<TKey, TValue> : BaseKeyOrValueCollection<TValue>
    {
        private readonly IKeyValueCollection<TKey, TValue> _col;

        public BaseValueCollection(IKeyValueCollection<TKey, TValue> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            _col = collection;
        }

        public override IEnumerator<TValue> GetEnumerator() => new Enumerator(_col);

        public override bool Contains(TValue item) => _col.ContainsValue(item);

        public override void CopyTo(TValue[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _col.Count + arrayIndex) throw new ArgumentException(nameof(array));

            foreach (var item in _col)
            {
                array[arrayIndex++] = item.Value;
            }
        }

        public override int Count => _col.Count;

        public struct Enumerator : IEnumerator<TValue>
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            internal Enumerator(IDictionary<TKey, TValue> dic)
            {
                _enumerator = dic.GetEnumerator();
            }

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset()=> _enumerator.Reset();

            public TValue Current => _enumerator.Current.Value;

            object IEnumerator.Current => Current;

            public void Dispose()=> _enumerator.Dispose();
        }
    }
}
