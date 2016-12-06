using System;
using System.Linq;
using DataStructuresCSharpTest.Common;
using Xunit;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.SeqList
{
    public abstract partial class SeqListTests<T> : IListGenericTests<T>
    {
        [Fact]
        public void Constructor_Default()
        {
            var list = new SeqList<T>();
            Assert.Equal(0, list.Count); //"Do not expect anything to be in the list."
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(100)]
        public void Constructor_Capacity(int capacity)
        {
            var list = new SeqList<T>(capacity);
            Assert.Equal(0, list.Count); //"Do not expect anything to be in the list."
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SeqList<T>(capacity));
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void Constructor_IEnumerable(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
            var list = new SeqList<T>(enumerable);
            var expected = enumerable.ToList();

            Assert.Equal(enumerableLength, list.Count); //"Number of items in list do not match the number of items given."

            for (var i = 0; i < enumerableLength; i++)
                Assert.Equal(expected[i], list[i]); //"Expected object in item array to be the same as in the list"
        }

        [Fact]
        public void Constructo_NullIEnumerable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { new SeqList<T>(null); }); //"Expected ArgumentnUllException for null items"
        }
    }
}
