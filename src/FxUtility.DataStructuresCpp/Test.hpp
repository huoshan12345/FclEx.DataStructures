#pragma once

#include <ctime>
#include <functional>
#include <vector>
#include <chrono>
#include <map>

#include "Define.h"
#include "IKeyValueCollection.h"

#ifndef SYSOUT_F
#define SYSOUT_F(f, ...) _RPT1( 0, f, __VA_ARGS__ ) // For Visual studio
#endif

#ifndef SpeedTest__             
#define SpeedTest__(data)   for (long blockTime = 0; (blockTime == 0 ? (blockTime = clock()) != 0 : false); SYSOUT_F(data "%.3fs", (double) (clock() - blockTime) / CLOCKS_PER_SEC))
#endif


namespace FxUtility
{
	using namespace std;
	using namespace Collections;

	template<typename TimeT = chrono::milliseconds>
	struct Measure
	{
		template<typename Func, typename ...Args>
		static typename TimeT::rep Execution(Func func, Args&&... args)
		{
			auto start = chrono::high_resolution_clock::now();
			func(forward<Args>(args)...);
			auto end = chrono::high_resolution_clock::now();
			auto duration = chrono::duration_cast<TimeT>(end - start);
			return duration.count();
		}
	};

	template<typename T, class TDic>
	class TestDic
	{
	public:
		static void Add(TDic &dic, const vector<T> &items)
		{
			for (auto &item : items)
			{
				dic.Add(item, item);
			}
		}

		static void ContainsKey(const TDic &dic, const vector<T> &items)
		{
			for (auto &item : items)
			{
				dic.ContainsKey(item);
			}
		}

		static void ContainsValue(const TDic &dic, const vector<T> &items)
		{
			for (auto &item : items)
			{
				dic.ContainsValue(item);
			}
		}

		static void IndexGet(const TDic &dic, const vector<T> &items)
		{
			for (auto &item : items)
			{
				var value = dic[item];
			}
		}

		static void IndexSet(TDic &dic, const vector<T> &items)
		{
			for (auto &item : items)
			{
				++dic[item];
			}
		}

		static void Remove(TDic &dic, const vector<T> &items)
		{
			for (auto &item : items)
			{
				dic.Remove(item);
			}
		}

		static void AddAndClear(TDic &dic, const vector<T> &items)
		{
			for (auto &item : items)
			{
				dic.Add(item, item);
				dic.Clear();
			}
		}

		static void Enumerate(TDic &dic, const vector<T> &items)
		{
			for (auto &item : dic)
			{
				auto temp = item;
			}
		}

		static void ConstIterate(const TDic &dic, const vector<T> &items)
		{
			for (auto it = dic.begin(); it != dic.end(); ++it)
			{
				auto temp = *it;
			}
		}

		static void IterateAndModify(TDic &dic, const vector<T> &items)
		{
			for (auto it = dic.begin(); it != dic.end(); ++it)
			{
				(*it).first = (*it).first * 2;
				it->second = it->second * 2;
			}
		}



	};

	class Test
	{
	public:

		template<typename T, class TDic>
		static map<string, Int64> TestKeyValueCollection(TDic &dic, const vector<T> &items)
		{
			map<string, Int64> result;

			result[nameof(Add)] = Measure<>::Execution(TestDic<T, TDic>::Add, dic, items);
			result[nameof(Enumerate)] = Measure<>::Execution(TestDic<T, TDic>::Enumerate, dic, items);
			result[nameof(Iterate)] = Measure<>::Execution(TestDic<T, TDic>::ConstIterate, dic, items);
			result[nameof(ContainsKey)] = Measure<>::Execution(TestDic<T, TDic>::ContainsKey, dic, items);
			// result[nameof(ContainsValue)] = Measure<>::Execution(TestDic<T, TDic>::ContainsValue, dic, items);			
			result[nameof(IndexGet)] = Measure<>::Execution(TestDic<T, TDic>::IndexGet, dic, items);
			result[nameof(IndexSet)] = Measure<>::Execution(TestDic<T, TDic>::IndexSet, dic, items);
			result[nameof(IterateAndModify)] = Measure<>::Execution(TestDic<T, TDic>::IterateAndModify, dic, items);
			result[nameof(Remove)] = Measure<>::Execution(TestDic<T, TDic>::Remove, dic, items);
			result[nameof(AddAndClear)] = Measure<>::Execution(TestDic<T, TDic>::AddAndClear, dic, items);
			return result;
		}

		static void PrintTestResult(const map<string, Int64> &result)
		{
			for (var &item : result)
			{
				printf("%-20s", item.first.c_str());
			}

			printf("\n");

			for (var &item : result)
			{
				printf("%-20lld", item.second);
			}
		}
	};
}
