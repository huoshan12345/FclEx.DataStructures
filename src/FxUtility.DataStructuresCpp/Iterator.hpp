#pragma once

#include <iterator>

namespace FxUtility
{
	namespace Node
	{
		using namespace std;

		template <class Node, class IteratorType>
		class Iterator : public iterator<forward_iterator_tag,
			typename Node::value_type,
			typename Node::difference_type,
			typename Node::pointer,
			typename Node::reference>
		{

		public:
			typedef Node										node_type;
			typedef typename node_type::BaseNode				BaseNode;
			typedef typename BaseNode::allocator_type			allocator_type;
			typedef typename BaseNode::difference_type			difference_type;
			typedef typename BaseNode::reference				reference;
			typedef typename BaseNode::const_reference			const_reference;
			typedef typename BaseNode::pointer					pointer;
			typedef typename BaseNode::const_pointer			const_pointer;
			typedef IteratorType								self_type;
			
			explicit Iterator(node_type *node) :_pNode(node) { }

			virtual ~Iterator() { }

			virtual self_type &operator++() = 0;

			virtual self_type operator++(int) = 0;

			reference operator*()
			{
				return _pNode->Item;
			}

			pointer operator->()
			{
				return &_pNode->Item;
			}

			bool operator==(const self_type &other) const
			{
				return _pNode == other._pNode;
			}

			bool operator!=(const self_type &other) const
			{
				return !operator==(other);
			}

		protected:
			node_type *_pNode;
		};

		template <class Node, class IteratorType>
		class ConstIterator : public iterator<forward_iterator_tag,
			typename Node::value_type,
			typename Node::difference_type,
			typename Node::const_pointer,
			typename Node::const_reference>
		{

		public:
			typedef Node										node_type;
			typedef typename node_type::BaseNode				BaseNode;
			typedef typename BaseNode::allocator_type			allocator_type;
			typedef typename BaseNode::difference_type			difference_type;
			typedef typename BaseNode::reference				reference;
			typedef typename BaseNode::const_reference			const_reference;
			typedef typename BaseNode::pointer					pointer;
			typedef typename BaseNode::const_pointer			const_pointer;
			typedef IteratorType								self_type;

			explicit ConstIterator(const node_type *node) :_pNode(node) { }

			virtual ~ConstIterator() { }

			virtual self_type &operator++() const = 0;

			virtual self_type operator++(int) const = 0;

			const_reference operator*() const
			{
				return _pNode->Item;
			}

			const_pointer operator->() const
			{
				return &_pNode->Item;
			}

			bool operator==(const self_type &other) const
			{
				return _pNode == other._pNode;
			}

			bool operator!=(const self_type &other) const
			{
				return !operator==(other);
			}

		protected:
			node_type *_pNode;
		};
	}
}
