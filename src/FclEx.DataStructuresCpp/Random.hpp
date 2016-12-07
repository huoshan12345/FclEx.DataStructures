#pragma once

#include <random>
#include <ctime>

#include "Define.h"

namespace FclEx
{
	using namespace std;

	class Random
	{
	public:

		explicit Random()
			: _randomNumberGenerator(static_cast<uint>(time(nullptr)))
			, _byteDistribution(numeric_limits<byte>::min(), numeric_limits<byte>::max())
		{ }

		int Next(int minValue = 0, int maxValue = numeric_limits<int>::max()) const
		{
			if (maxValue < minValue)
			{
				throw invalid_argument("maxValue must be greater than minvalue");
			}
			uniform_int_distribution<int> distribution(minValue, maxValue);
			return distribution(_randomNumberGenerator);
		}

		double NextDouble(double minValue = 0, double maxValue = 1) const
		{
			if (maxValue < minValue)
			{
				throw invalid_argument("maxValue must be greater than minvalue");
			}
			uniform_real_distribution<double> distribution(minValue, maxValue);
			return distribution(_randomNumberGenerator);
		}

		void NextBytes(vector<byte>& buffer) const
		{
			for (auto &i : buffer)
			{
				i = static_cast<byte>(_byteDistribution(_randomNumberGenerator));
			}
		}

	private:
		mutable default_random_engine _randomNumberGenerator;
		uniform_int_distribution<int> _byteDistribution;
	};
}
