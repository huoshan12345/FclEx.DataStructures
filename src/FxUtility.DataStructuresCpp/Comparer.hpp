#pragma once

namespace FxUtility
{
	struct PtrLessComparer
	{
		template<class T>
		bool operator()(const T& left, const T& right) const {
			return *left < *right;
		}
	};

	struct PtrGreaterComparer
	{
		template<class T>
		bool operator()(const T& left, const T& right) const {
			return *left > *right;
		}
	};

	template <typename T, typename TLessComparer = less<T>>
	class Comparer
	{
	public:

		bool Less(const T &lhs, const T &rhs) const
		{
			return _less(lhs, rhs);
		}

		bool Equals(const T &lhs, const T &rhs) const
		{
			return !_less(lhs, rhs) && !_less(rhs, lhs);
		}

		bool Greater(const T &lhs, const T &rhs) const
		{
			return _less(rhs, lhs);
		}

		bool LessOrEqual(const T &lhs, const T &rhs) const
		{
			return !Greater(lhs, rhs);
		}

		bool GreaterOrEqual(const T &lhs, const T &rhs) const
		{
			return !Less(lhs, rhs);
		}

		Int32 Compare(const T &lhs, const T &rhs) const
		{
			return _less(lhs, rhs) ? -1 : 
				(_less(rhs, lhs) ? 1 : 0);
		}

	private:
		const TLessComparer _less;
	};
}