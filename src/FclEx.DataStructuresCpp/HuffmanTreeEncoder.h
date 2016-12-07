#pragma once

#include "Define.h"
#include <vector>

namespace FclEx
{
	namespace Algorithms
	{
		namespace HuffmanTree
		{
			class HuffmanTreeNode;
			using namespace std;

			class HuffmanTreeEncoder
			{
			public:
				static vector<byte> Encode(vector<byte> datas);
				static vector<byte> Decode(vector<byte> datas);
			};
		}
	}
}
