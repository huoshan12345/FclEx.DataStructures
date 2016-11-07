#pragma once

#include <utility>
#include "ICollection.h"

namespace FxUtility
{
	namespace Collections
	{
		template<typename TKey, typename TValue>
		class IKeyValueCollection : public ICollection<pair<TKey, TValue>>
		{
		public:
			virtual const TValue& operator[](const TKey& key) const = 0;
			// Effects: If there is no key equivalent to x in the map, inserts value_type(x, T()) into the map.
			virtual TValue& operator[](const TKey& key) = 0;
			virtual void Add(const TKey& key, const TValue& value) = 0;
			virtual bool ContainsKey(const TKey& key) const = 0;
			virtual bool ContainsValue(const TValue& value) const = 0;
			virtual bool Remove(const TKey& key) = 0;
		};
	}
}
