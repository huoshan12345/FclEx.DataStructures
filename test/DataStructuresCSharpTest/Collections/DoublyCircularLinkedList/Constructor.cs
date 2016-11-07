using System;
using System.Linq;
using FxUtility.Collections;
using DataStructuresCSharpTest.Common;
using Xunit;

namespace DataStructuresCSharpTest.Collections.DoublyCircularLinkedList
{
    public abstract partial class DoublyCircularLinkedListTests<T> : IListGenericTests<T>
    {
        [Fact]
        public void Constructor_Default()
        {
            var list = new DoublyCircularLinkedList<T>();
            Assert.Equal(0, list.Count); //"Do not expect anything to be in the list."
        }

        [Theory]
        [MemberData(nameof(EnumerableTestData))]
        public void Constructor_IEnumerable(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
        {
            var enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
            var list = new DoublyCircularLinkedList<T>(enumerable);
            var expected = enumerable.ToList();

            Assert.Equal(enumerableLength, list.Count); //"Number of items in list do not match the number of items given."

            for (var i = 0; i < enumerableLength; i++)
                Assert.Equal(expected[i], list[i]); //"Expected object in item array to be the same as in the list"
        }

        [Fact]
        public void Constructo_NullIEnumerable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { new DoublyCircularLinkedList<T>(null); }); //"Expected ArgumentnUllException for null items"
        }
    }
}
