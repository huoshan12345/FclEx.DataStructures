using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FxUtility.Node;

namespace FxUtility.Collections
{
    public class DoublyLinkedListNode<T> : BaseNode<T, DoublyLinkedListNode<T>>
    {
        public DoublyLinkedListNode<T> Next
        {
            get { return NeighborNodes[0]; }
            set { NeighborNodes[0] = value; }
        }
        public DoublyLinkedListNode<T> Prev
        {
            get { return NeighborNodes[1]; }
            set { NeighborNodes[1] = value; }
        }

        public DoublyLinkedListNode() : this(default(T), null, null) { }

        public DoublyLinkedListNode(T item) : this(item, null, null) { }

        public DoublyLinkedListNode(T item, DoublyLinkedListNode<T> prev, DoublyLinkedListNode<T> next)
            : base(2, item)
        {
            Next = next;
            Prev = prev;
        }

        public static DoublyLinkedListNode<T> operator ++(DoublyLinkedListNode<T> node)
        {
            node = node.Next;
            return node;
        }

        public static DoublyLinkedListNode<T> operator --(DoublyLinkedListNode<T> node)
        {
            node = node.Prev;
            return node;
        }
    }

    public class DoublyCircularLinkedList<T> : ILinearList<T>
    {
        private DoublyLinkedListNode<T> _head;
        private int _count;
        private int _version;

        public DoublyCircularLinkedList()
        {
            _head = null;
            _count = 0;
            _version = 0;
        }

        private static DoublyLinkedListNode<T> BuildLinkedList(IEnumerable<T> collection)
        {
            var head = new DoublyLinkedListNode<T>();
            var p = head; // head.next points to the first node
            foreach (var item in collection)
            {
                var node = new DoublyLinkedListNode<T>(item);
                p.Next = node;
                node.Prev = p;
                p = p.Next;
            }
            // now p points to the last node
            p.Next = head.Next;
            if (p.Next != null) p.Next.Prev = p;
            return head.Next;
        }

        public DoublyCircularLinkedList(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            var arr = collection.ToArray();
            _head = BuildLinkedList(arr);
            _count += arr.Length;
            _version = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            Insert(_count, item);
            ++_version;
        }

        public void Clear()
        {
            if (_head == null) return;
            var current = _head;
            do
            {
                var temp = current;
                current = current.Next;
                temp.Invalidate();
            } while (current != _head);
            _head = null;
            _count = 0;
            ++_version;
        }

        public bool Contains(T item)
        {
            return Find(_head, item, _count, true) != null;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < _count + arrayIndex) throw new ArgumentException(nameof(array));

            var node = _head;
            if (node == null) return;
            do
            {
                array[arrayIndex++] = node.Item;
                node = node.Next;
            } while (node != _head);
        }

        public bool Remove(T item)
        {
            var node = Find(_head, item, _count, true);
            if (node == null) return false;
            RemoveNode(node);
            ++_version;
            return true;
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
            var newNode = new DoublyLinkedListNode<T>(item);
            if (_head == null)
            {
                _head = newNode;
                _head.Next = _head;
                _head.Prev = _head;
            }
            else
            {
                var nextNode = (index == _count) ? _head : GetAt(index);
                var prevNode = nextNode.Prev;
                prevNode.Next = newNode;
                newNode.Prev = prevNode;
                newNode.Next = nextNode;
                nextNode.Prev = newNode;
                if (index == 0) _head = newNode;
            }
            ++_count;
            ++_version;
        }

        private void RemoveNode(DoublyLinkedListNode<T> node)
        {
            if (node == _head && _count == 1)
            {
                _head.Invalidate();
                _head = null;
            }
            else
            {
                if (ReferenceEquals(node, _head)) _head = node.Next;
                node.Prev.Next = node.Next;
                node.Next.Prev = node.Prev;
                node.Invalidate();
            }
            --_count;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
            RemoveNode(GetAt(index));
            ++_version;
        }

        private DoublyLinkedListNode<T> GetAt(int index)
        {
            if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
            var node = _head;

            if (index < _count / 2)
            {
                var i = 0;
                while (i++ < index) node++;
            }
            else
            {
                var i = _count;
                while (i-- > index) node--;
            }
            return node;
        }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
                return GetAt(index).Item;
            }
            set
            {
                if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
                GetAt(index).Item = value;
                ++_version;
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(_count, collection);
            ++_version;
        }

        private static DoublyLinkedListNode<T> Find(DoublyLinkedListNode<T> startNode, T item, int maxCount, bool frontToBackOrder)
        {
            if (startNode == null) return null;
            var node = startNode;
            var c = EqualityComparer<T>.Default;
            var i = 0;
            do
            {
                if (i++ >= maxCount) break;
                if (c.Equals(node.Item, item)) return node;
                node = frontToBackOrder ? node.Next : node.Prev;
            } while (node != startNode);
            return null;
        }

        private static int RelativeIndexOf(DoublyLinkedListNode<T> startNode, T item, int maxCount, bool frontToBackOrder)
        {
            if (startNode == null) return -1;
            var node = startNode;
            var c = EqualityComparer<T>.Default;
            var i = 0;
            do
            {
                if (c.Equals(node.Item, item)) return i;
                if (i++ >= maxCount) break;
                node = frontToBackOrder ? node.Next : node.Prev;
            } while (node != startNode);
            return -1;
        }

        public int IndexOf(T item)
        {
            return IndexOf(item, 0, _count);
        }

        public int IndexOf(T item, int index)
        {
            return IndexOf(item, index, _count - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            if ((uint)index > (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > _count) throw new ArgumentOutOfRangeException(nameof(count));
            if (_count == 0) return -1; // IndexOf with a 0 count List is special cased to return -1. 

            var node = GetAt(index);
            if (node == null) return -1;
            var result = RelativeIndexOf(node, item, _count, true);
            if (result == -1) return -1;
            return result + index;
        }

        public int LastIndexOf(T item)
        {
            return LastIndexOf(item, _count - 1, _count);
        }

        public int LastIndexOf(T item, int index)
        {
            if (index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
            return LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if (index < 0 && _count != 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 && _count != 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_count == 0) return -1; // LastIndexOf with a 0 count List is special cased to return -1. 
            if (index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
            if (count > index + 1) throw new ArgumentOutOfRangeException(nameof(count));

            var node = GetAt(index);
            if (node == null) return -1;
            var result = RelativeIndexOf(node, item, _count, false);
            if (result == -1) return -1;
            return index - result;
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (index < 0 || index > _count) throw new ArgumentOutOfRangeException(nameof(index));
            var arr = collection.ToArray();
            var count = arr.Length;
            if (count == 0) return;

            var newNode = BuildLinkedList(arr);
            if (_head == null) _head = newNode;
            else
            {
                var node = (index == _count) ? _head : GetAt(index);
                var nodePrev = node.Prev;
                var newNodePrev = newNode.Prev;
                nodePrev.Next = newNode;
                newNode.Prev = nodePrev;
                newNodePrev.Next = node;
                node.Prev = newNodePrev;
            }
            if (index == 0) _head = newNode;
            _count += count;
            ++_version;
        }

        public void Reverse()
        {
            Reverse(_head, _count);
            ++_version;
        }

        private void Reverse(DoublyLinkedListNode<T> startNode, int maxCount)
        {
            if (startNode == null || maxCount <= 0) return;
            var p = startNode;
            var prevNode = startNode.Prev;
            var lastNode = startNode;
            for (var i = 0; i < maxCount; i++)
            {
                var temp = p.Next;
                p.Next = p.Prev;
                p.Prev = temp;
                lastNode = p;
                p = temp; // move forward p
                if (p == _head) break;
            }
            if (!(ReferenceEquals(_head, startNode) && maxCount >= _count))
            {   // it means not reverse all the list
                var lastNodeNext = p;
                prevNode.Next = lastNode;
                lastNode.Prev = prevNode;
                startNode.Next = lastNodeNext;
                lastNodeNext.Prev = startNode;
            }
            if (ReferenceEquals(_head, startNode)) _head = lastNode;
            ++_version;
        }

        public void Reverse(int index, int count)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_count < count + index) throw new ArgumentException();

            var startNode = GetAt(index % _count);
            Reverse(startNode, count);
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_count < count + index) throw new ArgumentException();
            if (count == 0) return;

            var startNode = GetAt(index);
            var prevNode = startNode.Prev;
            var lastNode = startNode;
            for (var i = 0; i < count; i++)
            {
                var p = lastNode;
                lastNode = lastNode.Next;
                p.Invalidate();
            }
            if (_count == count) _head = null;
            else
            {
                prevNode.Next = lastNode;
                lastNode.Prev = prevNode;
            }
            _count -= count;
            ++_version;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly DoublyCircularLinkedList<T> _list;
            private DoublyLinkedListNode<T> _node;
            private readonly int _version;
            private T _current;
            private int _index;

            internal Enumerator(DoublyCircularLinkedList<T> list)
            {
                _list = list;
                _version = list._version;
                _node = list._head;
                _current = default(T);
                _index = 0;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_version != _list._version) throw new InvalidOperationException();

                if (_node == null)
                {
                    _index = _list.Count + 1;
                    _current = default(T);
                    return false;
                }

                ++_index;
                _current = _node.Item;
                _node = _node.Next;
                if (_node == _list._head) _node = null;
                return true;
            }

            public void Reset()
            {
                if (_version != _list._version) throw new InvalidOperationException();

                _current = default(T);
                _node = _list._head;
                _index = 0;
            }

            public T Current
            {
                get
                {
                    if (_index == 0 || (_index == _list.Count + 1)) throw new InvalidOperationException();
                    return _current;
                }
            }

            object IEnumerator.Current => Current;
        }
    }
}
