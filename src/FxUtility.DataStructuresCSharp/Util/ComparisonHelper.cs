using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataStructuresCSharp.Util
{
    public static class ComparisonHelper<T>
    {
        public static IComparer<T> CreateComparer<TV>(Func<T, TV> keySelector)
        {
            return new CommonComparer<TV>(keySelector);
        }
        public static IComparer<T> CreateComparer<TV>(Func<T, TV> keySelector, IComparer<TV> comparer)
        {
            return new CommonComparer<TV>(keySelector, comparer);
        }

        class CommonComparer<V> : IComparer<T>
        {
            private readonly Func<T, V> _keySelector;
            private readonly IComparer<V> _comparer;

            public CommonComparer(Func<T, V> keySelector, IComparer<V> comparer)
            {
                _keySelector = keySelector;
                _comparer = comparer;
            }
            public CommonComparer(Func<T, V> keySelector)
                : this(keySelector, Comparer<V>.Default)
            { }

            public int Compare(T x, T y)
            {
                return _comparer.Compare(_keySelector(x), _keySelector(y));
            }
        }
    }
}
