#pragma once

#include <vector>
#include <map>
#include <functional>

namespace FclEx
{
	using namespace std;

	class MapHelper
	{
	public:

		template<typename TKey, typename TValue, typename T>
		static map<TKey, TValue> ConvertFrom(const vector<T> &source, function<TKey(T)> keySelector, function<TValue(T)> valueSelector)
		{
			map<TKey, TValue> result;

			for (auto &item : source)
			{
				result[keySelector(item)] = valueSelector(item);
			}
			return result;
		}
	};
}
