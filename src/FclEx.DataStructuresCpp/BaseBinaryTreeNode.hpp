#pragma once

#include "BaseNode.hpp"
#include <queue>

namespace FclEx
{
	namespace Node
	{
		template<typename T, class TNode, typename Allocator>
		class BaseBinaryTreeNode : public BaseNode<T, TNode, Allocator>
		{
		public:

			using BaseNode = BaseNode<T, TNode, Allocator>;
			using PNode = typename BaseNode::PNode;

			PNode& LeftChild = BaseNode::_neighborNodes[0];

			PNode& RightChild = BaseNode::_neighborNodes[1];

			PNode& Parent = BaseNode::_neighborNodes[2];

			bool OnlyHasLeftChild() const
			{
				return LeftChild != nullptr && RightChild == nullptr;
			}

			bool OnlyHasRightChild() const
			{
				return LeftChild == nullptr && RightChild != nullptr;
			}

			bool OnlyHasOneChild() const
			{
				return OnlyHasLeftChild || OnlyHasRightChild;
			}

			bool HasLeftChild() const
			{
				return LeftChild != nullptr;
			}

			bool HasParent() const
			{
				return Parent != nullptr;
			}

			bool HasRightChild() const
			{
				return RightChild != nullptr;
			}

			bool IsLeafNode() const
			{
				return LeftChild == nullptr && RightChild == nullptr;
			}

			bool HasTwoChildren() const
			{
				return LeftChild != nullptr && RightChild != nullptr;
			}

			virtual vector<PNode> Traverse()
			{
				vector<PNode> result;
				queue<PNode> queue;
				queue.push(dynamic_cast<PNode>(this));
				while (!queue.empty())
				{
					auto node = queue.front();
					queue.pop();
					result.push_back(node);
					if (node->HasLeftChild()) queue.push(node->LeftChild);
					if (node->HasRightChild()) queue.push(node->RightChild);
				}
				return result;
			}

			virtual void DestroyTree()
			{
				queue<PNode> queue;
				queue.push(dynamic_cast<PNode>(this));
				while (!queue.empty())
				{
					auto node = queue.front();
					queue.pop();
					if (node->HasLeftChild()) queue.push(node->LeftChild);
					if (node->HasRightChild()) queue.push(node->RightChild);
					delete node;
				}
			}

			PNode GetRoot()
			{
				var p = dynamic_cast<PNode>(this);
				while (p->HasParent())
				{
					p = p->Parent;
				}
				return p;
			}

		protected:
			explicit BaseBinaryTreeNode(T item, PNode left = nullptr, PNode right = nullptr, PNode parent = nullptr) :BaseNode(item, 3)
			{
				BaseNode::_neighborNodes[0] = left;
				BaseNode::_neighborNodes[1] = right;
				BaseNode::_neighborNodes[2] = parent;
			}
		};


	}
}
