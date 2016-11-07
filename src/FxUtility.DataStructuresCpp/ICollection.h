#pragma once

namespace FxUtility
{
	namespace Collections
	{
		template<typename T>
		class ICollection
		{
		public:
			virtual ~ICollection() = default;
			virtual SizeType Count() const = 0;
			virtual void Add(const T &item) = 0;
			virtual void Clear() = 0;
			virtual bool Contains(const T &item) const = 0;
			virtual bool Remove(const T &item) = 0;
		};
	}
}