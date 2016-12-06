using System.Collections.Generic;

namespace FclEx.Collections
{
    public interface IKeyValueCollection<TKey, TValue> : IDictionary<TKey, TValue>
    {
        bool ContainsValue(TValue value);
    }
}
