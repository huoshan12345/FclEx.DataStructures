using System;
using System.Collections;
using System.Collections.Generic;

namespace FxUtility.Collections
{
    public class BaseKeyCollection<TKey, TValue> : BaseKeyOrValueCollection<TKey>
    {
        private readonly IKeyValueCollection<TKey, TValue> _col;

        public BaseKeyCollection(IKeyValueCollection<TKey, TValue> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            _col = collection;
        }

        public override IEnumerator<TKey> GetEnumerator() => new Enumerator(_col);

        public override bool Contains(TKey item) => _col.ContainsKey(item);

        public override void CopyTo(TKey[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _col.Count + arrayIndex) throw new ArgumentException(nameof(array));

            foreach (var item in _col)
            {
                array[arrayIndex++] = item.Key;
            }
        }

        public override int Count => _col.Count;

        public struct Enumerator : IEnumerator<TKey>
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            internal Enumerator(IDictionary<TKey, TValue> dic)
            {
                _enumerator = dic.GetEnumerator();
            }

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset()=> _enumerator.Reset();

            public TKey Current => _enumerator.Current.Key;

            object IEnumerator.Current => Current;

            public void Dispose()=> _enumerator.Dispose();
        }
    }
}
