using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStructuresCSharpTest.Common;
using Xunit;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.MaxHeap
{
    public abstract class MaxHeapTests<T> : IGenericSharedApiTests<T>
    {
        #region MaxHeap<T> Helper Methods

        #region IGenericSharedAPI<T> Helper Methods

        protected MaxHeap<T> GenericheapFactory()
        {
            return new MaxHeap<T>();
        }

        protected MaxHeap<T> GenericheapFactory(int count)
        {
            var heap = new MaxHeap<T>();
            var seed = count * 34;
            for (var i = 0; i < count; i++)
                heap.Push(CreateT(seed++));
            return heap;
        }

        #endregion

        protected override IEnumerable<T> GenericIEnumerableFactory()
        {
            return GenericheapFactory();
        }

        protected override IEnumerable<T> GenericIEnumerableFactory(int count)
        {
            return GenericheapFactory(count);
        }

        protected override int Count(IEnumerable<T> enumerable) { return ((MaxHeap<T>)enumerable).Count; }
        protected override void Add(IEnumerable<T> enumerable, T value) { ((MaxHeap<T>)enumerable).Push(value); }
        protected override void Clear(IEnumerable<T> enumerable) { ((MaxHeap<T>)enumerable).Clear(); }
        protected override bool Contains(IEnumerable<T> enumerable, T value) { return ((MaxHeap<T>)enumerable).Contains(value); }
        protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) { ((MaxHeap<T>)enumerable).CopyTo(array, index); }
        protected override bool Remove(IEnumerable<T> enumerable) { ((MaxHeap<T>)enumerable).Pop(); return true; }
        protected override bool EnumeratorCurrentUndefinedOperationThrows => true;

        #endregion

        #region Constructor

        #endregion

        #region Constructor_IEnumerable

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void Generic_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var arr = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements).ToArray();
            var heap = new MaxHeap<T>(arr, Comparer<T>.Default);
            Assert.Equal(arr.Length, heap.Count);
            Array.Sort(arr, Comparer<T>.Default);
            Array.Reverse(arr);
            foreach (var item in arr)
            {
                Assert.Equal(item, heap.Pop());
            }
        }

        [Fact]
        public void Generic_Constructor_IEnumerable_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("collection", () => new MaxHeap<T>(null));
        }

        #endregion

        #region Pop

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void heap_Generic_Pop_AllElements(int count)
        {
            var heap = GenericheapFactory(count);
            var elements = heap.ToList();
            elements.Sort();
            elements.Reverse();
            foreach (var element in elements)
                Assert.Equal(element, heap.Pop());
        }

        [Fact]
        public void heap_Generic_Pop_OnEmptyheap_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new MaxHeap<T>().Pop());
        }

        #endregion

        #region ToArray

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void heap_Generic_ToArray(int count)
        {
            var heap = GenericheapFactory(count);
            Assert.True(heap.ToArray().SequenceEqual(heap.ToArray<T>()));
        }

        #endregion

        #region Peek

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void heap_Generic_Peek_AllElements(int count)
        {
            var heap = GenericheapFactory(count);
            var elements = heap.ToList();
            elements.Sort();
            elements.Reverse();
            foreach (var element in elements)
            {
                Assert.Equal(element, heap.Peek());
                heap.Pop();
            }
        }

        [Fact]
        public void heap_Generic_Peek_OnEmptyheap_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new MaxHeap<T>().Peek());
        }

        #endregion
    }
}
