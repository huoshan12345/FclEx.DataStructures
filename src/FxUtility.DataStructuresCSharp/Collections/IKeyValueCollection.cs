using System.Collections.Generic;

namespace FxUtility.Collections
{
    public interface IKeyValueCollection<TKey, TValue> : IDictionary<TKey, TValue>
    {
        bool ContainsValue(TValue value);
    }
}
