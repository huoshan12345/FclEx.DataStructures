using System;
using System.Collections.Generic;
using System.Linq;
using DataStructuresCSharpTest.Common;
using Xunit;
using FxUtility.Collections;

namespace DataStructuresCSharpTest.Collections.LinkedStack
{
    public abstract class LinkedStackTests<T> : IGenericSharedApiTests<T>
    {
        #region LinkedStack<T> Helper Methods

        #region IGenericSharedAPI<T> Helper Methods

        protected LinkedStack<T> GenericStackFactory()
        {
            return new LinkedStack<T>();
        }

        protected LinkedStack<T> GenericStackFactory(int count)
        {
            var stack = new LinkedStack<T>();
            var seed = count * 34;
            for (var i = 0; i < count; i++)
                stack.Push(CreateT(seed++));
            return stack;
        }

        #endregion

        protected override IEnumerable<T> GenericIEnumerableFactory()
        {
            return GenericStackFactory();
        }

        protected override IEnumerable<T> GenericIEnumerableFactory(int count)
        {
            return GenericStackFactory(count);
        }

        protected override int Count(IEnumerable<T> enumerable) { return ((LinkedStack<T>)enumerable).Count; }
        protected override void Add(IEnumerable<T> enumerable, T value) { ((LinkedStack<T>)enumerable).Push(value); }
        protected override void Clear(IEnumerable<T> enumerable) { ((LinkedStack<T>)enumerable).Clear(); }
        protected override bool Contains(IEnumerable<T> enumerable, T value) { return ((LinkedStack<T>)enumerable).Contains(value); }
        protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) { ((LinkedStack<T>)enumerable).CopyTo(array, index); }
        protected override bool Remove(IEnumerable<T> enumerable) { ((LinkedStack<T>)enumerable).Pop(); return true; }
        protected override bool EnumeratorCurrentUndefinedOperationThrows { get { return true; } }

        #endregion

        #region Constructor

        #endregion

        #region Constructor_IEnumerable

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void Stack_Generic_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
            var stack = new LinkedStack<T>(enumerable);
            Assert.Equal(enumerable.ToArray().Reverse(), stack.ToArray());
        }

        [Fact]
        public void Stack_Generic_Constructor_IEnumerable_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("collection", () => new LinkedStack<T>(null));
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
            Assert.Throws<InvalidOperationException>(() => new LinkedStack<T>().Pop());
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
            Assert.Throws<InvalidOperationException>(() => new LinkedStack<T>().Peek());
        }

        #endregion
    }
}
