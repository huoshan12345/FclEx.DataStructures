#pragma once
#include <vector>
#include <numeric>

namespace FxUtility
{
	using namespace std;

	class VectorHelper
	{
	public:

		template<typename T, size_t TSize>
		static typename vector<T>::iterator Append(vector<T> &source, const array<T, TSize> &arr)
		{
			return source.insert(source.begin() + source.size(), arr.begin(), arr.end());
		}

		template<typename T>
		static typename vector<T>::iterator Append(vector<T> &source, const vector<T> &arr)
		{
			return source.insert(source.begin() + source.size(), arr.begin(), arr.end());
		}

		template<typename T>
		static bool Equal(const vector<T> &left, const vector<T> &right)
		{
			return left.size() == right.size() &&
				equal(left.begin(), left.end(), right.begin());
		}

		static vector<int> Range(int start, int count)
		{
			vector<int> ivec(count);
			iota(ivec.begin(), ivec.end(), start);			

			//for (decltype(ivec.size()) i = 0; i < ivec.size(); ++i)
			//{
			//	ivec[i] = i;
			//}
			return ivec;
		}
	};	
}
