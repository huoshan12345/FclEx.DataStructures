#pragma once

#include <vector>
#include <fstream>

#include "Define.h"

namespace FclEx
{
	using namespace std;

	class FileHelper
	{
	public:
		static vector<byte> ReadFile(const char* filename)
		{
			// open the file:
			ifstream file(filename, ios::binary);

			// read the data:
			return vector<byte>((istreambuf_iterator<char>(file)), istreambuf_iterator<char>());
		}
	};
}
