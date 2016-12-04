using System;
using System.Collections;
using System.Collections.Generic;
using FxUtility.Node;

namespace FxUtility.Collections
{
    /// <summary>
    /// A SkipList is a data structure that allows fast search within an [ordered sequence] of elements
    /// <para>The SkipList gives an average of Log(n) on all dictionary operations.</para> 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SkipList<TKey, TValue> : IKeyValueCollection<TKey, TValue>
    {
        private const int MaxLevel = 32;        // Maximum level any node in a skip list can have
        private const double Probability = 0.5; // Probability factor used to determine the node level
        private readonly SkipListNode _head;    // The skip list header. It also serves as the NIL node.
        private int _listLevel;                 // Current maximum list level.
        private int _count;                     // Current number of elements in the skip list.
        private int _version;
        private readonly IComparer<TKey> _comparer;
        private readonly EqualityComparer<TValue> _valueComparer;
        private readonly Random _random;

        public SkipList() : this(Comparer<TKey>.Default)
        {
        }

        public SkipList(IComparer<TKey> comparer)
        {
            _head = new SkipListNode(MaxLevel);
            Initialize();
            _version = 0;
            _comparer = comparer ?? Comparer<TKey>.Default;
            _valueComparer = EqualityComparer<TValue>.Default;
            _random = new Random();
        }

        public SkipList(IDictionary<TKey, TValue> dictionary) : this(dictionary, null)
        {
        }

        public SkipList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer) : this(comparer)
        {
            foreach (var item in dictionary)
            {
                Add(item);
            }
        }

        private void Initialize()
        {
            for (var i = 0; i < _head.Height; i++)
            {
                _head.NextNodes[i] = _head;
            }
            _listLevel = 1;
            _count = 0;
        }

        /// <summary>
        /// Returns a level value for a new SkipList node which to be added
        /// </summary>
        /// <returns>
        /// The level value for a new SkipList node.
        /// </returns>
        private int GetNewLevel()
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

        public TValue this[TKey key]
        {
            get
            {
                var node = Find(key);
                if (node == null) throw new KeyNotFoundException();
                return node.Item.Value;
            }

            set
            {
                var node = Find(key);
                if (node == null) Add(key, value);
                else node.Item = new KeyValuePair<TKey, TValue>(key, value);
                _version++;
            }
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => new BaseKeyCollection<TKey, TValue>(this);

        public ICollection<TValue> Values => new BaseValueCollection<TKey, TValue>(this);

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));

            var prevNodes = FindPrevNodes(item.Key);
            if (prevNodes[0].Next != _head && _comparer.Compare(prevNodes[0].Next.Item.Key, item.Key) == 0)
                throw new ArgumentException($"An item with the same key has already been added. Key: {item.Key}");

            var newLevel = GetNewLevel(); // Get the level for the new node.
            var newNode = new SkipListNode(newLevel, item);
            // Insert the new node into the skip list.
            for (var i = 0; i < _listLevel && i < newLevel; i++)
            {
                // The new node next references are initialized to point to our update next references which point to nodes further along in the skip list.
                newNode.NextNodes[i] = prevNodes[i].NextNodes[i];
                // Take our update next references and point them towards the new node. 
                prevNodes[i].NextNodes[i] = newNode;
            }
            if (newLevel > _listLevel)
            {
                // Make sure our update references above the current skip list level point to the header. 
                for (var i = _listLevel; i < newLevel; i++)
                {
                    newNode.NextNodes[i] = _head.NextNodes[i];
                    _head.NextNodes[i] = newNode;
                }
                _listLevel = newLevel; // The current skip list level is now the new node level.
            }
            _count++;
            _version++;
        }

        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        private SkipListNode Find(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var p = _head;
            for (var i = _listLevel - 1; i >= 0; i--)
            {
                while (p.NextNodes[i] != _head && _comparer.Compare(p.NextNodes[i].Item.Key, key) < 0)
                {
                    p = p.NextNodes[i]; // Move forward in the skip list.
                }
                if (p.NextNodes[i] != _head && _comparer.Compare(p.NextNodes[i].Item.Key, key) == 0) return p.NextNodes[i];
            }
            return null;
        }

        /// <summary>
        /// BinarySearch prev nodes 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="minCapacity"></param>
        /// <returns></returns>
        private SkipListNode[] FindPrevNodes(TKey key)
        {
            var prevNodes = new SkipListNode[_listLevel];
            var p = _head;
            for (var i = _listLevel - 1; i >= 0; i--)
            {
                while (p.NextNodes[i] != _head && _comparer.Compare(p.NextNodes[i].Item.Key, key) < 0)
                {
                    p = p.NextNodes[i]; // Move forward in the skip list.
                }
                prevNodes[i] = p;
            }
            return prevNodes;
        }

        public void Clear()
        {
            var p = _head.NextNodes[0];
            while (p != _head)
            {
                var q = p;
                p = p.NextNodes[0];
                q.Invalidate();
            }
            Initialize();
            _version++;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var node = Find(item.Key);
            if (node == null) return false;
            return _valueComparer.Equals(node.Item.Value, item.Value);
        }

        public bool ContainsKey(TKey key) => Find(key) != null;

        public bool ContainsValue(TValue value)
        {
            var p = _head.Next;
            while (p != _head)
            {
                if (_valueComparer.Equals(p.Item.Value, value)) return true;
                p = p.Next;
            }
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _count + arrayIndex) throw new ArgumentException(nameof(array));

            var i = arrayIndex;
            var p = _head.Next;
            while (p != _head)
            {
                array[i] = p.Item;
                p++;
                i++;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this);

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key, true, item.Value);

        private bool Remove(TKey key, bool checkValue, TValue value = default(TValue))
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var prevNodes = FindPrevNodes(key);
            var node = prevNodes[0].Next;
            if (node == _head || _comparer.Compare(node.Item.Key, key) != 0) return false;
            if (checkValue && !_valueComparer.Equals(node.Item.Value, value)) return false;

            for (var i = 0; i < prevNodes.Length; i++)
            {
                if (prevNodes[i].NextNodes[i] != node) break;
                prevNodes[i].NextNodes[i] = node.NextNodes[i];
            }
            node.Invalidate();
            // After removing the node, we may need to lower the current skip list level if the node had the highest level of all of the nodes.
            while (_listLevel > 1 && _head.NextNodes[_listLevel - 1] == _head)
            {
                _listLevel--;
            }
            _count--;
            _version++;
            return true;
        }

        public bool Remove(TKey key)
        {
            return Remove(key, false);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var node = Find(key);
            if (node == null)
            {
                value = default(TValue);
                return false;
            }
            value = node.Item.Value;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class SkipListNode : BaseNode<KeyValuePair<TKey, TValue>, SkipListNode>
        {
            public SkipListNode(int level) : this(level, new KeyValuePair<TKey, TValue>(default(TKey), default(TValue)))
            {

            }

            public SkipListNode(int level, KeyValuePair<TKey, TValue> item) : base(level, item)
            {
            }

            public SkipListNode(int level, TKey key, TValue value)
                : this(level, new KeyValuePair<TKey, TValue>(key, value))
            {
            }

            public SkipListNode[] NextNodes => NeighborNodes;

            public SkipListNode Next => NeighborNodes[0];

            public int Height => NeighborNodes.Length;

            public static SkipListNode operator ++(SkipListNode node)
            {
                node = node.Next;
                return node;
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly SkipList<TKey, TValue> _list;
            private KeyValuePair<TKey, TValue> _current;
            private SkipListNode _node;
            private readonly int _version;
            private int _index;
            private static readonly KeyValuePair<TKey, TValue> EmptyKeyValuePair = default(KeyValuePair<TKey, TValue>);

            internal Enumerator(SkipList<TKey, TValue> list)
            {
                _list = list;
                _version = list._version;
                _node = list._head.Next;
                _current = EmptyKeyValuePair;
                _index = -1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_version != _list._version) throw new InvalidOperationException();
                if (_node == _list._head)
                {
                    _index = _list.Count;
                    _current = EmptyKeyValuePair;
                    return false;
                }
                ++_index;
                _current = _node.Item;
                _node++;
                return true;
            }

            public void Reset()
            {
                if (_version != _list._version) throw new InvalidOperationException();
                _current = EmptyKeyValuePair;
                _node = _list._head.Next;
                _index = -1;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (_index < 0 || (_index >= _list.Count)) throw new InvalidOperationException();
                    return _current;
                }
            }

            object IEnumerator.Current => Current;
        }
    }
}