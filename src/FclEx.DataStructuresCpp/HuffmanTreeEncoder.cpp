#pragma once


#include <vector>
#include "Define.h"
#include "HuffmanTreeEncoder.h"
#include "HuffmanTreeNode.hpp"
#include "HuffmanTreeHeader.hpp"
#include <memory>
#include <queue>
#include "Comparer.hpp"

namespace FclEx
{
	namespace Algorithms
	{
		namespace HuffmanTree
		{
			using namespace std;
			using PNode = HuffmanTreeNode*;

			static byte Decode(const vector<bool> &bits, uint &bitIndex, const PNode root)
			{
				auto p = root;
				while (!p->IsLeafNode())
				{
					auto bit = bits[bitIndex++];
					p = p->NeighborNodes[bit ? 1 : 0];
				}
				return static_cast<byte>(p->Item);
			}

			static vector<bool> GetCode(const PNode leafNode)
			{
#ifdef _DEBUG
				assert(leafNode->IsLeafNode());
#endif
				auto p = leafNode;
				vector<bool> bits;
				while (p->HasParent())
				{
					bits.push_back(p->Parent->LeftChild != p);
					p = p->Parent;
				}
				reverse(bits.begin(), bits.end());
				return bits;
			}

			static array<vector<bool>, HuffmanTreeHeader::MaxLength> BuildEncodingTable(array<PNode, HuffmanTreeHeader::MaxLength> leafNodeList)
			{
				array<vector<bool>, HuffmanTreeHeader::MaxLength> result;

				for (const auto &leafNode : leafNodeList)
				{
					auto &&code = GetCode(leafNode);
					result[leafNode->Item] = code;
				}
				return result;
			}

			static tuple<PNode, array<PNode, HuffmanTreeHeader::MaxLength>> BuildTree(array<uint, HuffmanTreeHeader::MaxLength> freqArray)
			{
				array<PNode, HuffmanTreeHeader::MaxLength> nodeArr;
				for (uint i = 0; i < freqArray.size(); ++i)
				{
					nodeArr[i] = new HuffmanTreeNode(i, freqArray[i], i);
				}
				priority_queue<PNode, vector<PNode>, PtrGreaterComparer> priQueue(nodeArr.begin(), nodeArr.end());

				array<PNode, HuffmanTreeHeader::MaxLength> leafNodeArr;

				auto leafIndex = 0;
				auto id = static_cast<uint>(freqArray.size());
				while (priQueue.size() >= 2)
				{
					auto node1 = priQueue.top();
					priQueue.pop();
					auto node2 = priQueue.top();
					priQueue.pop();
					if (node1->IsLeafNode()) leafNodeArr[leafIndex++] = node1;
					if (node2->IsLeafNode()) leafNodeArr[leafIndex++] = node2;
					priQueue.push(new HuffmanTreeNode(node1, node2, id++));
				}
#if _DEBUG
				var root = priQueue.top();
				for(auto node : leafNodeArr)
				{
					var nodeRoot = node->GetRoot();
					assert(nodeRoot == root);
				}

				auto list = root->Traverse();
				assert(list.size() == 2 * HuffmanTreeHeader::MaxLength - 1);
#endif
				return tuple<PNode, array<PNode, HuffmanTreeHeader::MaxLength>>(priQueue.top(), leafNodeArr);
			}

			vector<byte> HuffmanTreeEncoder::Encode(vector<byte> datas)
			{
				HuffmanTreeHeader header(datas, true);
				var tree = BuildTree(header.FreqArray());
				var root = std::get<0>(tree);
				var leafNodeList = std::get<1>(tree);
				var encodingTable = BuildEncodingTable(leafNodeList);
				root->DestroyTree();

				vector<bool> encodedSource;
				encodedSource.reserve(datas.size() * 8 + header.HeaderLength() * 8);
				for (auto &data : datas)
				{
					VectorHelper::Append(encodedSource, encodingTable[data]);
				}	
				auto headerBytes = header.ToBytes();
				auto encodedSourceBytes = BitConverter::GetBytes(encodedSource);
				VectorHelper::Append(headerBytes, encodedSourceBytes);
				return headerBytes;
			}

			vector<byte> HuffmanTreeEncoder::Decode(vector<byte> datas)
			{
				HuffmanTreeHeader header(datas, false);
				var root = std::get<0>(BuildTree(header.FreqArray()));
				vector<byte> resultBytes(header.DataLength());
				var bits = BitConverter::BytesToBits(datas);
				var bitIndex = header.HeaderLength() * 8;

				for (size_t i = 0; i < header.DataLength(); ++i)
				{
					resultBytes[i] = HuffmanTree::Decode(bits, bitIndex, root);
				}
				root->DestroyTree();
				return resultBytes;
			}
		}
	}
}
