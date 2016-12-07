#pragma once

#include <utility>

#include "BaseBinaryTreeNode.hpp"
#include "Define.h"

namespace FclEx
{
	namespace Node
	{
		using namespace std;
		using namespace Node;

		template<class TKey, class TValue, typename Allocator>
		class BinarySearchTreeNode :public BaseBinaryTreeNode<pair<TKey, TValue>, BaseBinaryTreeNode<TKey, TValue, Allocator>, Allocator>
		{
			using BaseBinaryTreeNode = BaseBinaryTreeNode<TKey, TValue, Allocator>;
			using BaseNode = typename BaseBinaryTreeNode::BaseNode;
			using PNode = typename BaseNode::PNode;

		public:
			BinarySearchTreeNode(TKey key, TValue value, PNode left = null, PNode right = null, PNode parent = null)
				:BinarySearchTreeNode(pair<TKey, TValue>(key, value), left, right, parent)
			{				
			}

				BinarySearchTreeNode(pair<TKey, TValue> item, PNode left = null, PNode right = null, PNode parent = null)
				: BaseBinaryTreeNode(item, left, right, parent)
			{				
			}

		private:

		};
	}
}
