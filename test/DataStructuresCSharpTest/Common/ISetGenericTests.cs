﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataStructuresCSharpTest.Common
{
    /// <summary>
    /// Contains tests that ensure the correctness of any class that implements the generic
    /// ISet interface.
    /// 
    /// Tests for an ISet follow a rather different structure because of the consistency in
    /// function signatures. Instead of having a test for every data scenario within a class for
    /// every set function, there is instead a test for every configuration of enumerable parameter.
    /// Each of those tests calls a Validation function that calculates the expected result and then
    /// compares it to the actual result of the set operation.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public abstract class ISetGenericTests<T> : ICollectionGenericTests<T>
    {
        #region ISet<T> Helper methods

        /// <summary>
        /// Creates an instance of an ISet{T} that can be used for testing.
        /// </summary>
        /// <returns>An instance of an ISet{T} that can be used for testing.</returns>
        protected abstract ISet<T> GenericISetFactory();

        /// <summary>
        /// Creates an instance of an ISet{T} that can be used for testing.
        /// </summary>
        /// <param name="count">The number of unique items that the returned ISet{T} contains.</param>
        /// <returns>An instance of an ISet{T} that can be used for testing.</returns>
        protected virtual ISet<T> GenericISetFactory(int count)
        {
            var collection = GenericISetFactory();
            AddToCollection(collection, count);
            return collection;
        }

        protected override void AddToCollection(ICollection<T> collection, int numberOfItemsToAdd)
        {
            var seed = 9600;
            var set = (ISet<T>)collection;
            while (set.Count < numberOfItemsToAdd)
            {
                var toAdd = CreateT(seed++);
                while (set.Contains(toAdd) || (InvalidValues != Array.Empty<T>() && InvalidValues.Contains(toAdd, GetIEqualityComparer())))
                    toAdd = CreateT(seed++);
                set.Add(toAdd);
            }
        }

        protected virtual int ISet_Large_Capacity => 4000;

        #endregion

        #region ICollection<T> Helper Methods

        protected override ICollection<T> GenericICollectionFactory() => GenericISetFactory();

        protected override ICollection<T> GenericICollectionFactory(int count) => GenericISetFactory(count);

        protected override bool DuplicateValuesAllowed => false;
        protected override bool DefaultValueWhenNotAllowedThrows => false;

        #endregion

        #region ICollection_Generic

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_ReturnValue(int count)
        {
            if (!IsReadOnly)
            {
                var set = GenericISetFactory(count);
                var seed = 92834;
                var newValue = CreateT(seed++);
                while (set.Contains(newValue))
                    newValue = CreateT(seed++);
                Assert.True(set.Add(newValue));
                if (!DuplicateValuesAllowed)
                    Assert.False(set.Add(newValue));
                Assert.Equal(count + 1, set.Count);
                Assert.True(set.Contains(newValue));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_DuplicateValue_DoesNothing(int count)
        {
            if (!IsReadOnly)
            {
                if (!DuplicateValuesAllowed)
                {
                    var collection = GenericICollectionFactory(count);
                    var seed = 800;
                    var duplicateValue = CreateT(seed++);
                    while (collection.Contains(duplicateValue))
                        duplicateValue = CreateT(seed++);
                    collection.Add(duplicateValue);
                    collection.Add(duplicateValue);
                    Assert.Equal(count + 1, collection.Count);
                }
            }
        }

        #endregion

        #region Set Function Validation

        private void Validate_ExceptWith(ISet<T> set, IEnumerable<T> enumerable)
        {
            if (set.Count == 0 || enumerable == set)
            {
                set.ExceptWith(enumerable);
                Assert.Equal(0, set.Count);
            }
            else
            {
                var expected = new HashSet<T>(set, GetIEqualityComparer());
                foreach (var element in enumerable)
                    expected.Remove(element);
                set.ExceptWith(enumerable);
                Assert.Equal(expected.Count, set.Count);
                Assert.True(expected.SetEquals(set));
            }
        }

        private void Validate_IntersectWith(ISet<T> set, IEnumerable<T> enumerable)
        {
            if (set.Count == 0 || Enumerable.Count(enumerable) == 0)
            {
                set.IntersectWith(enumerable);
                Assert.Equal(0, set.Count);
            }
            else if (set == enumerable)
            {
                var beforeOperation = new HashSet<T>(set, GetIEqualityComparer());
                set.IntersectWith(enumerable);
                Assert.True(beforeOperation.SetEquals(set));
            }
            else
            {
                var comparer = GetIEqualityComparer();
                var expected = new HashSet<T>(comparer);
                foreach (var value in set)
                    if (enumerable.Contains(value, comparer))
                        expected.Add(value);
                set.IntersectWith(enumerable);
                Assert.Equal(expected.Count, set.Count);
                Assert.True(expected.SetEquals(set));
            }
        }

        private void Validate_IsProperSubsetOf(ISet<T> set, IEnumerable<T> enumerable)
        {
            var setContainsValueNotInEnumerable = false;
            var enumerableContainsValueNotInSet = false;
            var comparer = GetIEqualityComparer();
            foreach (var value in set) // Every value in Set must be in Enumerable
            {
                if (!enumerable.Contains(value, comparer))
                {
                    setContainsValueNotInEnumerable = true;
                    break;
                }
            }
            foreach (var value in enumerable) // Enumerable must contain at least one value not in Set
            {
                if (!set.Contains(value, comparer))
                {
                    enumerableContainsValueNotInSet = true;
                    break;
                }
            }
            Assert.Equal(!setContainsValueNotInEnumerable && enumerableContainsValueNotInSet, set.IsProperSubsetOf(enumerable));
        }

        private void Validate_IsProperSupersetOf(ISet<T> set, IEnumerable<T> enumerable)
        {
            var isProperSuperset = true;
            var setContainsElementsNotInEnumerable = false;
            var comparer = GetIEqualityComparer();
            foreach (var value in enumerable)
            {
                if (!set.Contains(value, comparer))
                {
                    isProperSuperset = false;
                    break;
                }
            }
            foreach (var value in set)
            {
                if (!enumerable.Contains(value, comparer))
                {
                    setContainsElementsNotInEnumerable = true;
                    break;
                }
            }
            isProperSuperset = isProperSuperset && setContainsElementsNotInEnumerable;
            Assert.Equal(isProperSuperset, set.IsProperSupersetOf(enumerable));
        }

        private void Validate_IsSubsetOf(ISet<T> set, IEnumerable<T> enumerable)
        {
            var comparer = GetIEqualityComparer();
            foreach (var value in set)
                if (!enumerable.Contains(value, comparer))
                {
                    Assert.False(set.IsSubsetOf(enumerable));
                    return;
                }
            Assert.True(set.IsSubsetOf(enumerable));
        }

        private void Validate_IsSupersetOf(ISet<T> set, IEnumerable<T> enumerable)
        {
            var comparer = GetIEqualityComparer();
            foreach (var value in enumerable)
                if (!set.Contains(value, comparer))
                {
                    Assert.False(set.IsSupersetOf(enumerable));
                    return;
                }
            Assert.True(set.IsSupersetOf(enumerable));
        }

        private void Validate_Overlaps(ISet<T> set, IEnumerable<T> enumerable)
        {
            var comparer = GetIEqualityComparer();
            foreach (var value in enumerable)
            {
                if (set.Contains(value, comparer))
                {
                    Assert.True(set.Overlaps(enumerable));
                    return;
                }
            }
            Assert.False(set.Overlaps(enumerable));
        }

        private void Validate_SetEquals(ISet<T> set, IEnumerable<T> enumerable)
        {
            var comparer = GetIEqualityComparer();
            foreach (var value in set)
            {
                if (!enumerable.Contains(value, comparer))
                {
                    Assert.False(set.SetEquals(enumerable));
                    return;
                }
            }
            foreach (var value in enumerable)
            {
                if (!set.Contains(value, comparer))
                {
                    Assert.False(set.SetEquals(enumerable));
                    return;
                }
            }
            Assert.True(set.SetEquals(enumerable));
        }

        private void Validate_SymmetricExceptWith(ISet<T> set, IEnumerable<T> enumerable)
        {
            var comparer = GetIEqualityComparer();
            var expected = new HashSet<T>(comparer);
            foreach (var element in enumerable)
                if (!set.Contains(element, comparer))
                    expected.Add(element);
            foreach (var element in set)
                if (!enumerable.Contains(element, comparer))
                    expected.Add(element);
            set.SymmetricExceptWith(enumerable);
            Assert.Equal(expected.Count, set.Count);
            Assert.True(expected.SetEquals(set));
        }

        private void Validate_UnionWith(ISet<T> set, IEnumerable<T> enumerable)
        {
            var comparer = GetIEqualityComparer();
            var expected = new HashSet<T>(set, comparer);
            foreach (var element in enumerable)
                if (!set.Contains(element, comparer))
                    expected.Add(element);
            set.UnionWith(enumerable);
            Assert.Equal(expected.Count, set.Count);
            Assert.True(expected.SetEquals(set));
        }

        #endregion

        #region Set Function tests

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_NullEnumerableArgument(int count)
        {
            var set = GenericISetFactory(count);
            Assert.Throws<ArgumentNullException>(() => set.ExceptWith(null));
            Assert.Throws<ArgumentNullException>(() => set.IntersectWith(null));
            Assert.Throws<ArgumentNullException>(() => set.IsProperSubsetOf(null));
            Assert.Throws<ArgumentNullException>(() => set.IsProperSupersetOf(null));
            Assert.Throws<ArgumentNullException>(() => set.IsSubsetOf(null));
            Assert.Throws<ArgumentNullException>(() => set.IsSupersetOf(null));
            Assert.Throws<ArgumentNullException>(() => set.Overlaps(null));
            Assert.Throws<ArgumentNullException>(() => set.SetEquals(null));
            Assert.Throws<ArgumentNullException>(() => set.SymmetricExceptWith(null));
            Assert.Throws<ArgumentNullException>(() => set.UnionWith(null));
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_ExceptWith(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_ExceptWith(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_IntersectWith(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_IntersectWith(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_IsProperSubsetOf(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_IsProperSubsetOf(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_IsProperSupersetOf(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_IsProperSupersetOf(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_IsSubsetOf(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_IsSubsetOf(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_IsSupersetOf(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_IsSupersetOf(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_Overlaps(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_Overlaps(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_SetEquals(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_SetEquals(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_SymmetricExceptWith(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_SymmetricExceptWith(set, enumerable);
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_UnionWith(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Validate_UnionWith(set, enumerable);
        }

        #endregion

        #region Set Function tests on itself

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_ExceptWith_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_ExceptWith(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_IntersectWith_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_IntersectWith(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_IsProperSubsetOf_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_IsProperSubsetOf(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_IsProperSupersetOf_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_IsProperSupersetOf(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_IsSubsetOf_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_IsSubsetOf(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_IsSupersetOf_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_IsSupersetOf(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_Overlaps_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_Overlaps(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_SetEquals_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Assert.True(set.SetEquals(set));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_SymmetricExceptWith_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_SymmetricExceptWith(set, set);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ISet_Generic_UnionWith_Itself(int setLength)
        {
            var set = GenericISetFactory(setLength);
            Validate_UnionWith(set, set);
        }

        #endregion

        #region Set Function tests on a very large Set

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_ExceptWith_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_ExceptWith(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_IntersectWith_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_IntersectWith(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_IsProperSubsetOf_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_IsProperSubsetOf(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_IsProperSupersetOf_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_IsProperSupersetOf(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_IsSubsetOf_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_IsSubsetOf(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_IsSupersetOf_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_IsSupersetOf(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_Overlaps_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_Overlaps(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_SetEquals_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_SetEquals(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_SymmetricExceptWith_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_SymmetricExceptWith(set, enumerable);
        }

        [Fact]
        // [OuterLoop]
        public void ISet_Generic_UnionWith_LargeSet()
        {
            var set = GenericISetFactory(ISet_Large_Capacity);
            var enumerable = CreateEnumerable(EnumerableType.List, set, 150, 0, 0);
            Validate_UnionWith(set, enumerable);
        }

        #endregion

        #region Other misc ISet test Scenarios

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void ISet_Generic_SymmetricExceptWith_AfterRemovingElements(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var set = GenericISetFactory(setLength);
            var value = CreateT(532);
            if (!set.Contains(value))
                set.Add(value);
            set.Remove(value);
            var enumerable = CreateEnumerable(enumerableType, set, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            Debug.Assert(enumerable != null);

            var comparer = GetIEqualityComparer();
            var expected = new HashSet<T>(comparer);
            foreach (var element in enumerable)
                if (!set.Contains(element, comparer))
                    expected.Add(element);
            foreach (var element in set)
                if (!enumerable.Contains(element, comparer))
                    expected.Add(element);
            set.SymmetricExceptWith(enumerable);
            Assert.Equal(expected.Count, set.Count);
            Assert.True(expected.SetEquals(set));
        }

        #endregion
    }
}
