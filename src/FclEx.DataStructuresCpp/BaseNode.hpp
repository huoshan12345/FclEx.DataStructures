#pragma once

#include <vector>
#include "Define.h"

namespace FclEx
{
	namespace Node
	{
		using namespace std;

		template<typename T, class TNode, typename Allocator>
		class BaseNode
		{
		public:

			typedef Allocator									allocator_type;
			typedef typename allocator_type::difference_type    difference_type;
			typedef typename allocator_type::reference          reference;
			typedef typename allocator_type::const_reference    const_reference;
			typedef typename allocator_type::pointer            pointer;
			typedef typename allocator_type::const_pointer      const_pointer;
			typedef T											value_type;

			using PNode = TNode*;
			using ItemType = T;

			ItemType Item;

			vector<PNode>& NeighborNodes = this->_neighborNodes;

			SizeType NeighborNodesNum() const
			{
				return _neighborNodes.size();
			}

		protected:

			vector<PNode> _neighborNodes;

			explicit BaseNode(T item, SizeType neighborNodesNum) : Item(item)
			{
				if (neighborNodesNum <= 0) throw invalid_argument("neighborNodesNum");
				_neighborNodes.resize(neighborNodesNum, nullptr);
			}

			// destructor
			virtual ~BaseNode() noexcept
			{
#ifdef _DEBUG
				// printf("destructor:BaseNode\n");
#endif
			}
		};
	}
}
