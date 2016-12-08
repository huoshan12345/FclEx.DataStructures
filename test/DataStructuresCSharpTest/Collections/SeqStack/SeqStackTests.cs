using System;
using System.Collections.Generic;
using System.Linq;
using DataStructuresCSharpTest.Common;
using Xunit;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.SeqStack
{
    public abstract class SeqStackTests<T> : IGenericSharedApiTests<T>
    {
        #region SeqStack<T> Helper Methods

        #region IGenericSharedAPI<T> Helper Methods

        protected SeqStack<T> GenericStackFactory()
        {
            return new SeqStack<T>();
        }

        protected SeqStack<T> GenericStackFactory(int count)
        {
            var stack = new SeqStack<T>(count);
            var seed = count * 34;
            for (var i = 0; i < count; i++)
                stack.Push(CreateT(seed++));
            return stack;
        }

        #endregion

        protected override IEnumerable<T> GenericIEnumerableFactory() => GenericStackFactory();

        protected override IEnumerable<T> GenericIEnumerableFactory(int count) => GenericStackFactory(count);

        protected override int Count(IEnumerable<T> enumerable) { return ((SeqStack<T>)enumerable).Count; }
        protected override void Add(IEnumerable<T> enumerable, T value) { ((SeqStack<T>)enumerable).Push(value); }
        protected override void Clear(IEnumerable<T> enumerable) { ((SeqStack<T>)enumerable).Clear(); }
        protected override bool Contains(IEnumerable<T> enumerable, T value) { return ((SeqStack<T>)enumerable).Contains(value); }
        protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) { ((SeqStack<T>)enumerable).CopyTo(array, index); }
        protected override bool Remove(IEnumerable<T> enumerable) { ((SeqStack<T>)enumerable).Pop(); return true; }
        protected override bool EnumeratorCurrentUndefinedOperationThrows { get { return true; } }

        #endregion

        #region Constructor

        #endregion

        #region Constructor_IEnumerable

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void Generic_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
            var stack = new SeqStack<T>(enumerable);
            Assert.Equal(enumerable.ToArray().Reverse(), stack.ToArray());
        }

        [Fact]
        public void Generic_Constructor_IEnumerable_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("collection", () => new SeqStack<T>(null));
        }

        #endregion

        #region Constructor_Capacity

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_Constructor_int(int count)
        {
            var stack = new SeqStack<T>(count);
            Assert.Equal(new T[0], stack.ToArray());
        }

        [Fact]
        public void Generic_Constructor_int_Negative_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new SeqStack<T>(-1));
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new SeqStack<T>(int.MinValue));
        }

        #endregion

        #region Pop

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_Pop_AllElements(int count)
        {
            var stack = GenericStackFactory(count);
            var elements = stack.ToList();
            foreach (var element in elements)
                Assert.Equal(element, stack.Pop());
        }

        [Fact]
        public void Stack_Generic_Pop_OnEmptyStack_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new SeqStack<T>().Pop());
        }

        #endregion

        #region ToArray

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_ToArray(int count)
        {
            var stack = GenericStackFactory(count);
            Assert.True(stack.ToArray().SequenceEqual(stack.ToArray<T>()));
        }

        #endregion

        #region Peek

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_Peek_AllElements(int count)
        {
            var stack = GenericStackFactory(count);
            var elements = stack.ToList();
            foreach (var element in elements)
            {
                Assert.Equal(element, stack.Peek());
                stack.Pop();
            }
        }

        [Fact]
        public void Stack_Generic_Peek_OnEmptyStack_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new SeqStack<T>().Peek());
        }

        #endregion

        #region TrimExcess

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_TrimExcess_OnValidStackThatHasntBeenRemovedFrom(int count)
        {
            var stack = GenericStackFactory(count);
            stack.TrimExcess();
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_TrimExcess_Repeatedly(int count)
        {
            var stack = GenericStackFactory(count); ;
            var expected = stack.ToList();
            stack.TrimExcess();
            stack.TrimExcess();
            stack.TrimExcess();
            Assert.True(stack.SequenceEqual(expected));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_TrimExcess_AfterRemovingOneElement(int count)
        {
            if (count > 0)
            {
                var stack = GenericStackFactory(count); ;
                var expected = stack.ToList();
                var elementToRemove = stack.ElementAt(0);

                stack.TrimExcess();
                stack.Pop();
                expected.RemoveAt(0);
                stack.TrimExcess();

                Assert.True(stack.SequenceEqual(expected));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_TrimExcess_AfterClearingAndAddingSomeElementsBack(int count)
        {
            if (count > 0)
            {
                var stack = GenericStackFactory(count); ;
                stack.TrimExcess();
                stack.Clear();
                stack.TrimExcess();
                Assert.Equal(0, stack.Count);

                AddToCollection(stack, count / 10);
                stack.TrimExcess();
                Assert.Equal(count / 10, stack.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Stack_Generic_TrimExcess_AfterClearingAndAddingAllElementsBack(int count)
        {
            if (count > 0)
            {
                var stack = GenericStackFactory(count); ;
                stack.TrimExcess();
                stack.Clear();
                stack.TrimExcess();
                Assert.Equal(0, stack.Count);

                AddToCollection(stack, count);
                stack.TrimExcess();
                Assert.Equal(count, stack.Count);
            }
        }

        #endregion
    }
}
