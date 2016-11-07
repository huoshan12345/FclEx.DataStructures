#pragma once

#include "BaseBinaryTreeNode.hpp"

namespace FxUtility
{
	namespace Algorithms
	{
		namespace HuffmanTree
		{
			using namespace Node;

			class HuffmanTreeNode : public BaseBinaryTreeNode<short, HuffmanTreeNode, allocator<short>>
			{

			public:
				Int32 Frequency;
				uint Id;

				// default constructor
				explicit HuffmanTreeNode(short item, int frequency, uint id) : BaseBinaryTreeNode(item),
					Frequency(frequency),
					Id(id)
				{
				}

				explicit HuffmanTreeNode(PNode left, PNode right, uint id) : HuffmanTreeNode(-1, left->Frequency + right->Frequency, id)
				{
					LeftChild = left;
					RightChild = right;
					left->Parent = this;
					right->Parent = this;
				}

				// copy constructor
				explicit HuffmanTreeNode(const HuffmanTreeNode&) = default;

				// move constructor
				explicit HuffmanTreeNode(HuffmanTreeNode&&) = default;

				// copy assignment
				// Make copy assignment non-virtual, take the parameter by const&, and return by non-const&
				HuffmanTreeNode& operator=(const HuffmanTreeNode& x) = default;
				
				// move assignment
				HuffmanTreeNode& operator=(HuffmanTreeNode&&) = default;

				// destructor
				virtual ~HuffmanTreeNode() = default;

				bool operator<(const HuffmanTreeNode& rhs) const
				{
					return Frequency == rhs.Frequency ? Id < rhs.Id
						: Frequency < rhs.Frequency;
				}

				bool operator>(const HuffmanTreeNode& rhs) const
				{
					return Frequency == rhs.Frequency ? Id > rhs.Id
						: Frequency > rhs.Frequency;
				}

				bool operator==(const HuffmanTreeNode& rhs) const
				{
					return this->Item == rhs.Item && this->Frequency == rhs.Frequency && this->Id == rhs.Id;
				}

				bool operator!=(const HuffmanTreeNode& rhs) const
				{
					return !(*this == rhs);
				}


			};
		}
	}
}
