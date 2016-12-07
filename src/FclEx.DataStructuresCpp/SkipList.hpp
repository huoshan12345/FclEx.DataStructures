#pragma once

#include "Define.h"
#include "IKeyValueCollection.h"
#include "Random.hpp"
#include "BaseNode.hpp"
#include "Comparer.hpp"
#include "Iterator.hpp"
#include <memory>
#include "NonCopyable.hpp"

namespace FclEx
{
	namespace Collections
	{
		using namespace std;
		using namespace Node;

		template<typename TKey, typename TValue, typename Allocator>
		class SkipListNode : public BaseNode<pair<TKey, TValue>, SkipListNode<TKey, TValue, Allocator>, Allocator>
		{
		public:

			using BaseNode = BaseNode<pair<TKey, TValue>, SkipListNode, Allocator>;
			using PNode = typename BaseNode::PNode;

			explicit SkipListNode(SizeType level) : SkipListNode(level, ItemType(default(TKey), default(TValue))) { }

			explicit SkipListNode(SizeType level, pair<TKey, TValue> item) : BaseNode(item, level) { }

			explicit SkipListNode(SizeType level, TKey key, TValue value) : SkipListNode(level, pair<TKey, TValue>(key, value)) { }
			
			PNode& Next = BaseNode::_neighborNodes[0];

			SizeType Height() const
			{
				return BaseNode::NeighborNodesNum();
			}
		};

		template<typename TKey, typename TValue, typename Allocator>
		class SkipListIterator : public Iterator<SkipListNode<TKey, TValue, Allocator>, SkipListIterator<TKey, TValue, Allocator>>
		{
		public:
			using Iterator = Iterator<SkipListNode<TKey, TValue, Allocator>, SkipListIterator>;
			using IteratorType = typename Iterator::self_type;
			using NodeType = typename Iterator::node_type;

			SkipListIterator(NodeType *node) :Iterator(node) { }

			IteratorType &operator++() override
			{
				Iterator::_pNode = Iterator::_pNode->Next;
				return *this;
			}

			IteratorType operator++(int) override
			{
				IteratorType old(*this);
				Iterator::_pNode = Iterator::_pNode->Next;
				return old;
			}
		};

		template<typename TKey,
			typename TValue,
			typename TLess = less<TKey>,
			typename Allocator = allocator<pair<TKey, TValue>>>
			class SkipList : IKeyValueCollection<TKey, TValue>, NonCopyable
		{
		public:

			using Node = SkipListNode<TKey, TValue, Allocator>;
			using PNode = typename Node::PNode;
			using ItemType = typename Node::ItemType;
			using Iterator = SkipListIterator<TKey, TValue, Allocator>;

			Iterator begin()
			{
				return Iterator(_head->Next);
			}

			Iterator end()
			{
				return Iterator(_nil);
			}

			Iterator begin() const
			{
				return Iterator(_head->Next);
			}

			Iterator end() const
			{
				return Iterator(_nil);
			}

			SkipList() :
				_head(new Node(MaxLevel)),
				_nil(null)
			{
				Initialize();
			}

			~SkipList() noexcept
			{
				var p = _head;
				while (p != _nil)
				{
					var q = p;
					p = p->Next;
					delete q;
				}
				if(_nil != null) delete _nil;
			}

			SizeType Count() const override
			{
				return _count;
			}

			void Add(const ItemType &item) override
			{
				pair<vector<PNode>, bool> prevNodes = FindPrevNodes(item.first);
				if (!prevNodes.second)
				{
					Insert(item, prevNodes.first);
				}
			}

			void Clear() override
			{
				var p = _head->Next;
				while (p != _nil)
				{
					var q = p;
					p = p->Next;
					delete q;
				}
				Initialize();
			}

			bool Contains(const ItemType &item) const override
			{
				return Find(item.first) == null;
			}

			bool Remove(const ItemType &item) override
			{
				return Remove(item.first, true, item.second);
			}

			//// index-get
			const TValue& operator[](const TKey& key) const override
			{
				auto node = Find(key);
				return node == null ? _defaultValue : node->Item.second;
			}

			//// index-set
			TValue& operator[](const TKey& key) override
			{
				pair<vector<PNode>, bool> prevNodes = FindPrevNodes(key);
				if (prevNodes.second)
				{
					return prevNodes.first[0]->Next->Item.second;
				}
				else
				{
					var node = Insert(ItemType(key, default(TValue)), prevNodes.first);
					return node->Item.second;
				}
			}

			void Add(const TKey& key, const TValue& value) override
			{
				Add(ItemType(key, value));
			}

			bool ContainsKey(const TKey& key) const override 
			{ 
				return Find(key) == null; 
			}

			bool ContainsValue(const TValue& value) const override 
			{ 
				for(const auto &item : *this)
				{
					if(_valueComparer.Equals(item.second, value))
					{
						return false;
					}
				}
				return true;
			}

			bool Remove(const TKey& key) override
			{ 
				return Remove(key, false);
			}

		private:

			static constexpr UInt32 MaxLevel = 32;			// Maximum level any node in a skip list can have
			static constexpr double Probability = 0.5;		// Probability factor used to determine the node level
			const PNode _head;								// The skip list header.
			const PNode _nil;								//  NIL node.

			Int32 _listLevel;								// Current maximum list level.
			UInt32 _count;									// Current number of elements in the skip list.
			const TValue _defaultValue = default(TValue);
			const Comparer<TKey, TLess> _comparer;
			const Comparer<TValue> _valueComparer;
			const Random _random;

			Int32 GetNewLevel() const
			{
				var level = 1;
				// Determines the next node level.
				while (_random.NextDouble() < Probability
					&& level < MaxLevel
					&& level <= _listLevel)
				{
					level++;
				}
				return level;
			}

			PNode Find(TKey key) const
			{
				var p = _head;
				for (var i = _listLevel - 1; i >= 0; --i)
				{
					while (p->NeighborNodes[i] != _nil && _comparer.Less(p->NeighborNodes[i]->Item.first, key))
					{
						p = p->NeighborNodes[i]; // Move forward in the skip list.
					}
					if (p->NeighborNodes[i] != _nil && _comparer.Equals(p->NeighborNodes[i]->Item.first, key)) return p->NeighborNodes[i];
				}
				return null;
			}

			PNode Insert(const pair<TKey, TValue> &item, vector<PNode> &prevNodes)
			{
				var newLevel = GetNewLevel(); // Get the level for the new node.
				var newNode = new Node(newLevel, item);

				for (var i = 0; i < _listLevel && i < newLevel; ++i)
				{
					// The new node next references are initialized to point to our update next references which point to nodes further along in the skip list.
					newNode->NeighborNodes[i] = prevNodes[i]->NeighborNodes[i];
					// Take our update next references and point them towards the new node. 
					prevNodes[i]->NeighborNodes[i] = newNode;
				}
				if (newLevel > _listLevel)
				{
					// Make sure our update references above the current skip list level point to the header. 
					for (var i = _listLevel; i < newLevel; ++i)
					{
						newNode->NeighborNodes[i] = _head->NeighborNodes[i];
						_head->NeighborNodes[i] = newNode;
					}
					_listLevel = newLevel; // The current skip list level is now the new node level.
				}
				++_count;
				return newNode;
			}

			pair<vector<PNode>, bool> FindPrevNodes(TKey key) const
			{
				vector<PNode> prevNodes(_listLevel, null);

				var p = _head;
				var exist = false;
				for (var i = _listLevel - 1; i >= 0; i--)
				{
					while (p->NeighborNodes[i] != _nil && _comparer.Less(p->NeighborNodes[i]->Item.first, key))
					{
						p = p->NeighborNodes[i]; // Move forward in the skip list.
					}
					prevNodes[i] = p;
					if (p->NeighborNodes[i] != _nil && _comparer.Equals(p->NeighborNodes[i]->Item.first, key)) exist = true;
				}
				return{ prevNodes,exist };
			}

			void Initialize()
			{
				for (decltype(_head->Height()) i = 0; i < _head->Height(); ++i)
				{
					_head->NeighborNodes[i] = _nil;
				}
				_listLevel = 1;
				_count = 0;
			}

			bool Remove(TKey key, bool checkValue, TValue value = default(TValue))
			{
				var prevNodesResult = FindPrevNodes(key);
				if (!prevNodesResult.second) return false;

				auto &prevNodes = prevNodesResult.first;
				auto node = prevNodes[0]->Next;
				if (checkValue && !_valueComparer.Equals(node->Item.second, value)) return false;

				for (SizeType i = 0; i < prevNodes.size(); i++)
				{
					if (prevNodes[i]->NeighborNodes[i] != node) break;
					prevNodes[i]->NeighborNodes[i] = node->NeighborNodes[i];
				}
				delete node;
				// After removing the node, we may need to lower the current skip list level if the node had the highest level of all of the nodes.
				while (_listLevel > 1 && _head->NeighborNodes[_listLevel - 1] == _head)
				{
					--_listLevel;
				}
				--_count;
				return true;
			}

		};

	}
}
