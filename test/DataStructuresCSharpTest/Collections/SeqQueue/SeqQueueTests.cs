using System;
using System.Collections.Generic;
using System.Linq;
using DataStructuresCSharpTest.Common;
using Xunit;
using FxUtility.Collections;

namespace DataStructuresCSharpTest.Collections.SeqQueue
{
    public abstract class SeqQueueTests<T> : IGenericSharedApiTests<T>
    {
        #region SeqQueue<T> Helper Methods

        protected SeqQueue<T> GenericQueueFactory()
        {
            return new SeqQueue<T>();
        }

        protected SeqQueue<T> GenericQueueFactory(int count)
        {
            var queue = new SeqQueue<T>(count);
            var seed = count * 34;
            for (var i = 0; i < count; i++)
                queue.Enqueue(CreateT(seed++));
            return queue;
        }

        #endregion

        #region IGenericSharedAPI<T> Helper Methods

        protected override IEnumerable<T> GenericIEnumerableFactory()
        {
            return GenericQueueFactory();
        }

        protected override IEnumerable<T> GenericIEnumerableFactory(int count)
        {
            return GenericQueueFactory(count);
        }

        protected override int Count(IEnumerable<T> enumerable) { return ((SeqQueue<T>)enumerable).Count; }
        protected override void Add(IEnumerable<T> enumerable, T value) { ((SeqQueue<T>)enumerable).Enqueue(value); }
        protected override void Clear(IEnumerable<T> enumerable) { ((SeqQueue<T>)enumerable).Clear(); }
        protected override bool Contains(IEnumerable<T> enumerable, T value) { return ((SeqQueue<T>)enumerable).Contains(value); }
        protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) { ((SeqQueue<T>)enumerable).CopyTo(array, index); }
        protected override bool Remove(IEnumerable<T> enumerable) { ((SeqQueue<T>)enumerable).Dequeue(); return true; }
        protected override bool EnumeratorCurrentUndefinedOperationThrows { get { return true; } }

        #endregion

        #region Constructor_IEnumerable

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void Queue_Generic_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
            var queue = new SeqQueue<T>(enumerable);
            Assert.Equal(enumerable, queue);
        }

        [Fact]
        public void Queue_Generic_Constructor_IEnumerable_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("collection", () => new SeqQueue<T>(null));
        }

        #endregion

        #region Constructor_Capacity

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_Constructor_int(int count)
        {
            var queue = new SeqQueue<T>(count);
            Assert.Equal(new T[0], queue.ToArray());
            queue.Clear();
            Assert.Equal(new T[0], queue.ToArray());
        }

        [Fact]
        public void Queue_Generic_Constructor_int_Negative_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new SeqQueue<T>(-1));
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new SeqQueue<T>(int.MinValue));
        }

        #endregion

        #region Dequeue

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_Dequeue_AllElements(int count)
        {
            var queue = GenericQueueFactory(count);
            var elements = queue.ToList();
            foreach (var element in elements)
                Assert.Equal(element, queue.Dequeue());
        }

        [Fact]
        public void Queue_Generic_Dequeue_OnEmptyQueue_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new SeqQueue<T>().Dequeue());
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(1, 1)]
        [InlineData(3, 100)]
        public void Queue_Generic_EnqueueAndDequeue(int capacity, int items)
        {
            var seed = 53134;
            var q = new SeqQueue<T>(capacity);
            Assert.Equal(0, q.Count);

            // Enqueue some values and make sure the count is correct
            var source = (List<T>)CreateEnumerable(EnumerableType.List, null, items, 0, 0);
            foreach (var val in source)
            {
                q.Enqueue(val);
            }
            Assert.Equal(source, q);

            // Dequeue to make sure the values are removed in the right order and the count is updated
            for (var i = 0; i < items; i++)
            {
                var itemToRemove = source[0];
                source.RemoveAt(0);
                Assert.Equal(itemToRemove, q.Dequeue());
                Assert.Equal(items - i - 1, q.Count);
            }

            // Can't dequeue when empty
            Assert.Throws<InvalidOperationException>(() => q.Dequeue());

            // But can still be used after a failure and after bouncing at empty
            var itemToAdd = CreateT(seed++);
            q.Enqueue(itemToAdd);
            Assert.Equal(itemToAdd, q.Dequeue());
        }

        #endregion

        #region ToArray

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_ToArray(int count)
        {
            var queue = GenericQueueFactory(count);
            Assert.True(queue.ToArray().SequenceEqual(queue.ToArray<T>()));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_ToArray_NonWrappedQueue(int count)
        {
            var collection = new SeqQueue<T>(count + 1);
            AddToCollection(collection, count);
            var elements = collection.ToArray();
            elements.Reverse();
            Assert.True(Enumerable.SequenceEqual(elements, collection.ToArray<T>()));
        }

        #endregion

        #region Peek

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_Peek_AllElements(int count)
        {
            var queue = GenericQueueFactory(count);
            var elements = queue.ToList();
            foreach (var element in elements)
            {
                Assert.Equal(element, queue.Peek());
                queue.Dequeue();
            }
        }

        [Fact]
        public void Queue_Generic_Peek_OnEmptyQueue_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new SeqQueue<T>().Peek());
        }

        #endregion

        #region TrimExcess

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_TrimExcess_OnValidQueueThatHasntBeenRemovedFrom(int count)
        {
            var queue = GenericQueueFactory(count);
            queue.TrimExcess();
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_TrimExcess_Repeatedly(int count)
        {
            var queue = GenericQueueFactory(count); ;
            var expected = queue.ToList();
            queue.TrimExcess();
            queue.TrimExcess();
            queue.TrimExcess();
            Assert.True(queue.SequenceEqual(expected));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_TrimExcess_AfterRemovingOneElement(int count)
        {
            if (count > 0)
            {
                var queue = GenericQueueFactory(count); ;
                var expected = queue.ToList();
                queue.TrimExcess();
                var removed = queue.Dequeue();
                expected.Remove(removed);
                queue.TrimExcess();

                Assert.True(queue.SequenceEqual(expected));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_TrimExcess_AfterClearingAndAddingSomeElementsBack(int count)
        {
            if (count > 0)
            {
                var queue = GenericQueueFactory(count); ;
                queue.TrimExcess();
                queue.Clear();
                queue.TrimExcess();
                Assert.Equal(0, queue.Count);

                AddToCollection(queue, count / 10);
                queue.TrimExcess();
                Assert.Equal(count / 10, queue.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Queue_Generic_TrimExcess_AfterClearingAndAddingAllElementsBack(int count)
        {
            if (count > 0)
            {
                var queue = GenericQueueFactory(count); ;
                queue.TrimExcess();
                queue.Clear();
                queue.TrimExcess();
                Assert.Equal(0, queue.Count);

                AddToCollection(queue, count);
                queue.TrimExcess();
                Assert.Equal(count, queue.Count);
            }
        }

        #endregion
    }
}
