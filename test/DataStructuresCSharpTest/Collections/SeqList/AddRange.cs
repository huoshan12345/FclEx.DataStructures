using System;
using System.Linq;
using DataStructuresCSharpTest.Common;
using Xunit;

namespace DataStructuresCSharpTest.Collections.SeqList
{
    public abstract partial class SeqListTests<T> : IListGenericTests<T>
    {
        // Has tests that pass a variably sized TestCollection and MyEnumerable to the AddRange function
        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void AddRange(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var list = GenericListFactory(listLength);
            var listBeforeAdd = list.ToList();
            var enumerable = CreateEnumerable(enumerableType, list, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
            list.AddRange(enumerable);

            // Check that the first section of the List is unchanged
            Assert.All(Enumerable.Range(0, listLength), index =>
            {
                Assert.Equal(listBeforeAdd[index], list[index]);
            });

            // Check that the added elements are correct
            Assert.All(Enumerable.Range(0, enumerableLength), index =>
            {
                Assert.Equal(enumerable.ElementAt(index), list[index + listLength]);
            });
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void AddRange_NullEnumerable_ThrowsArgumentNullException(int count)
        {
            var list = GenericListFactory(count);
            var listBeforeAdd = list.ToList();
            Assert.Throws<ArgumentNullException>(() => list.AddRange(null));
            Assert.Equal(listBeforeAdd, list);
        }
    }
}
