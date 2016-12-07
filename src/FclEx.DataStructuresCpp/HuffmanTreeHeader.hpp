#pragma once

#include <vector>
#include <map>

#include "Define.h"
#include "BitConverter.hpp"
#include "VectorHelper.hpp"

namespace FclEx
{
	namespace Algorithms
	{
		namespace HuffmanTree
		{
			using namespace std;
			class HuffmanTreeHeader
			{
			public:

				static constexpr int MaxLength = numeric_limits<byte>::max() + 1;

				HuffmanTreeHeader(const vector<byte> &datas, bool create)
				{
					auto tuple = create ? CreateHeader(datas) : GetHeader(datas);
					_dataLength = std::get<0>(tuple);
					_freqArray = std::get<1>(tuple);
					_compressedFreqArray = std::get<2>(tuple);
				}
				
				// copy constructor
				explicit HuffmanTreeHeader(const HuffmanTreeHeader&) = default;

				// move constructor
				explicit HuffmanTreeHeader(HuffmanTreeHeader&&) = default;

				// copy assignment
				// Make copy assignment non-virtual, take the parameter by const&, and return by non-const&
				HuffmanTreeHeader& operator=(const HuffmanTreeHeader&) = default;

				// move assignment
				HuffmanTreeHeader& operator=(HuffmanTreeHeader&&) = default;

				
				// destructor
				virtual ~HuffmanTreeHeader() noexcept
				{
#ifdef _DEBUG
					// printf("destructor:HuffmanTreeHeader\n");
#endif
				}

				uint HeaderLength() const
				{
					return static_cast<uint>(_compressedFreqArray.size()) + sizeof(uint) + sizeof(uint);
				}

				uint DataLength() const
				{
					return _dataLength;
				}

				const array<uint, MaxLength>& FreqArray() const
				{
					return _freqArray;
				}

				vector<byte> ToBytes() const
				{
					auto headerLength = HeaderLength();
					vector<byte> bytes;
					bytes.reserve(headerLength);
					VectorHelper::Append(bytes, BitConverter::GetBytes(headerLength));
					VectorHelper::Append(bytes, BitConverter::GetBytes(_dataLength));
					VectorHelper::Append(bytes, _compressedFreqArray);
					return bytes;
				}

			private:
				uint _dataLength;
				array<uint, MaxLength> _freqArray;
				vector<byte> _compressedFreqArray;

				static vector<byte> CompressFreqArray(array<uint, MaxLength> freqArray)
				{
					map<uint, uint> byteDic;
					map<uint, uint> ushortDic;
					map<uint, uint> intDic;

					for (uint i = 0; i < freqArray.size(); ++i)
					{
						if (freqArray[i] == 0) continue;
						else if (freqArray[i] < numeric_limits<byte>::max()) byteDic[i] = freqArray[i];
						else if (freqArray[i] < numeric_limits<ushort>::max()) ushortDic[i] = freqArray[i];
						else intDic[i] = freqArray[i];
					}

					auto length = sizeof(uint) * 3
						+ byteDic.size() * (sizeof(byte) + sizeof(byte))
						+ ushortDic.size() * (sizeof(byte) + sizeof(ushort))
						+ intDic.size() * (sizeof(byte) + sizeof(uint));

					vector<byte> result;
					result.reserve(length);

					VectorHelper::Append(result, BitConverter::GetBytes(static_cast<uint>(byteDic.size())));
					for (auto &item : byteDic)
					{
						result.push_back(static_cast<byte>(item.first));
						result.push_back(static_cast<byte>(item.second));
					}

					VectorHelper::Append(result, BitConverter::GetBytes(static_cast<uint>(ushortDic.size())));
					for (auto &item : ushortDic)
					{
						result.push_back(static_cast<byte>(item.first));
						var num = static_cast<ushort>(item.second);
						VectorHelper::Append(result, BitConverter::GetBytes(num));
					}

					VectorHelper::Append(result, BitConverter::GetBytes(static_cast<uint>(intDic.size())));
					for (auto &item : intDic)
					{
						result.push_back(static_cast<byte>(item.first));
						var num = static_cast<uint>(item.second);
						VectorHelper::Append(result, BitConverter::GetBytes(num));
					}
					return result;
				}

				static array<uint, MaxLength> DecompressFreqArray(const vector<byte> &datas)
				{
					array<uint, MaxLength> freqArray = {0};

					auto it = datas.begin();
					auto byteLen = BitConverter::BytesTo<uint>(it._Ptr);
					it += 4;
					for (size_t i = 0; i < byteLen; ++i)
					{
						auto itemIndex = *it++;
						auto freq = *it++;
						freqArray[itemIndex] = freq;
					}

					auto ushortLen = BitConverter::BytesTo<uint>(it._Ptr);
					it += 4;
					for (size_t i = 0; i < ushortLen; ++i)
					{
						auto itemIndex = *it++;
						auto freq = BitConverter::BytesTo<ushort>(it._Ptr);
						it += 2;
						freqArray[itemIndex] = freq;
					}

					auto intLen = BitConverter::BytesTo<uint>(it._Ptr);
					it += 4;
					for (size_t i = 0; i < intLen; ++i)
					{
						auto itemIndex = *it++;
						auto freq = BitConverter::BytesTo<uint>(it._Ptr);
						it += 4;
						freqArray[itemIndex] = freq;
					}

					return freqArray;
				}

				static tuple<uint, array<uint, MaxLength>, vector<byte>> GetHeader(vector<byte> datas)
				{
					auto it = datas.begin();

					auto headerLength = BitConverter::BytesTo<uint>(it._Ptr);
					it += sizeof(uint);
					auto dataLength = BitConverter::BytesTo<uint>(it._Ptr);
					it += sizeof(uint);

					const auto headerEndIt = datas.begin() + headerLength;
					var len = headerLength - (it - datas.begin());
					vector<byte> compressedFreqArray(len);

					copy(it, headerEndIt, compressedFreqArray.begin());

					return tuple<uint, array<uint, MaxLength>, vector<byte>>(
						dataLength, DecompressFreqArray(compressedFreqArray), compressedFreqArray);
				}

				static array<uint, MaxLength> CreateFreqArray(vector<byte> datas)
				{
					array<uint, MaxLength> result = { 0 };
					for (auto &data : datas)
					{
						result[data]++;
					}
					return result;
				}

				static tuple<uint, array<uint, MaxLength>, vector<byte>> CreateHeader(vector<byte> datas)
				{
					auto dataLength = datas.size();
					auto freqArray = CreateFreqArray(datas);
					auto compressedFreqArray = CompressFreqArray(freqArray);
					return tuple<uint, array<uint, MaxLength>, vector<byte>>(static_cast<uint>(dataLength), freqArray, compressedFreqArray);
				}

			};
		}
	}
}
