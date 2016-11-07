#pragma once

#include <vector>
#include <array>
#include "Define.h"

namespace FxUtility
{
	using namespace std;

	class BitConverter
	{
	public:

		template<typename T>
		static array<byte, sizeof(T)> GetBytes(T value)
		{
			array<byte, sizeof(T)> bytes;
			auto p = static_cast<const byte*>(static_cast<const void*>(&value));
			std::copy(p, p + sizeof(T), bytes.begin());
			return bytes;
		}

		template<typename T>
		static T BytesTo(const byte *start)
		{
			// auto p = static_cast<const T*>(static_cast<const void*>(start));
			// return *p;

			T temp{};
			// std::copy(start, start + sizeof(T), &temp); // dont use std::copy here
			memcpy(&temp, start, sizeof(T));
			return temp;
		}

		static vector<byte> GetBytes(const vector<bool> &bits)
		{
			auto numBytes = bits.size() / 8;
			if (bits.size() % 8 != 0) ++numBytes;

			vector<byte> bytes(numBytes);
			auto byteIndex = 0, bitIndex = 0;
			for (auto bit : bits)
			{
				if (bit) bytes[byteIndex] |= static_cast<byte>(1 << bitIndex);
				++bitIndex;
				if (bitIndex == 8)
				{
					bitIndex = 0;
					++byteIndex;
				}
			}
			return bytes;
		}

		static vector<bool> BytesToBits(vector<byte> bytes)
		{
			vector<bool> bits(bytes.size() * 8);
			for (size_t  i = 0; i < bytes.size(); ++i)
			{
				var singleByte = bytes[i];
				for (var j = 0; j < 8; ++j)
				{
					bits[i * 8 + j] = static_cast<bool>((singleByte >> j) & 1);
				}
			}
			return bits;
		}
	};
}

