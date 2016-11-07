#include <vector>
#include <string>
#include <iostream>

#include "Define.h"
#include "HuffmanTreeEncoder.h"
#include "VectorHelper.hpp"
#include "FileHelper.hpp"
#include "IKeyValueCollection.h"
#include <functional>
#include "Test.hpp"
#include "SkipList.hpp"
#include "MapHelper.hpp"
#include "StringHelper.hpp"

using namespace std;
using namespace FxUtility;
using namespace Algorithms::HuffmanTree;


static void TestHuffmanEncoding(const vector<byte> &bytes)
{
	cout << bytes.size() << endl;

	var codes = HuffmanTreeEncoder::Encode(bytes);
	cout << codes.size() << endl;

	var plains = HuffmanTreeEncoder::Decode(codes);
	cout << plains.size() << endl;



	auto isEqual = VectorHelper::Equal(bytes, plains);

	if (!isEqual) cout << "数据验证失败" << endl;
	else if (codes.size() != 0)
	{
		printf("压缩率：%.3lf\n", codes.size() / static_cast<double>(plains.size()));
	}
}

static void TestKeyValueCollection()
{
	auto items = VectorHelper::Range(1, 10 * 1000);
	SkipList<int, int> list;

	auto result = Test::TestKeyValueCollection<int, SkipList<int, int>>(list, items);
	Test::PrintTestResult(result);

}


int main(void)
{
	//cout << "压缩字符串" << endl;
	//string str = "static_cast is the first cast you should attempt to use. It does things like implicit conversions between types (such as int to float, or pointer to void*), and it can also call explicit conversion functions (or implicit ones). In many cases, explicitly stating static_cast isn't necessary, but it's important to note that the T(something) syntax is equivalent to (T)something and should be avoided (more on that later). A T(something, something_else) is safe, however, and guaranteed to call the constructor.";	
	//TestHuffmanEncoding(vector<byte>(str.begin(), str.end()));
	//cin.get();

	//cout << "压缩文件" << endl;
	//const auto file = R"(test.txt)";
	//SpeedTest__("压缩文件用时：")
	//{
	//	TestHuffmanEncoding(FileHelper::ReadFile(file));
	//}
	// cin.get();
	TestKeyValueCollection();


	cin.get();
	return 0;
}
