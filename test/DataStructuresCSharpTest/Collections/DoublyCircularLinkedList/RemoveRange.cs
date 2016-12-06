using System;
using DataStructuresCSharpTest.Common;
using Xunit;
using System.Linq;

namespace DataStructuresCSharpTest.Collections.DoublyCircularLinkedList
{
    public abstract partial class DoublyCircularLinkedListTests<T> : IListGenericTests<T>
    {
        [Theory]
        [InlineData(10, 3, 3)]
        [InlineData(10, 0, 10)]
        [InlineData(10, 10, 0)]
        [InlineData(10, 5, 5)]
        [InlineData(10, 0, 5)]
        [InlineData(10, 1, 9)]
        [InlineData(10, 9, 1)]
        [InlineData(10, 2, 8)]
        [InlineData(10, 8, 2)]
        public void Remove_Range(int listLength, int index, int count)
        {
            var list = GenericListFactory(listLength);
            var beforeList = list.ToList();

            list.RemoveRange(index, count);
            Assert.Equal(list.Count, listLength - count); //"Expected them to be the same."
            for (var i = 0; i < index; i++)
            {
                Assert.Equal(list[i], beforeList[i]); //"Expected them to be the same."
            }

            for (var i = index; i < count - (index + count); i++)
            {
                Assert.Equal(list[i], beforeList[i + count]); //"Expected them to be the same."
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void RemoveRange_InvalidParameters(int listLength)
        {
            if (listLength % 2 != 0)
                listLength++;
            var list = GenericListFactory(listLength);
            var invalidParameters = new[]
            {
                Tuple.Create(listLength     ,1             ),
                Tuple.Create(listLength+1   ,0             ),
                Tuple.Create(listLength+1   ,1             ),
                Tuple.Create(listLength     ,2             ),
                Tuple.Create(listLength/2   ,listLength/2+1),
                Tuple.Create(listLength-1   ,2             ),
                Tuple.Create(listLength-2   ,3             ),
                Tuple.Create(1              ,listLength    ),
                Tuple.Create(0              ,listLength+1  ),
                Tuple.Create(1              ,listLength+1  ),
                Tuple.Create(2              ,listLength    ),
                Tuple.Create(listLength/2+1 ,listLength/2  ),
                Tuple.Create(2              ,listLength-1  ),
                Tuple.Create(3              ,listLength-2  ),
            };

            Assert.All(invalidParameters, invalidSet =>
            {
                if (invalidSet.Item1 >= 0 && invalidSet.Item2 >= 0)
                    Assert.Throws<ArgumentException>(() => list.RemoveRange(invalidSet.Item1, invalidSet.Item2));
            });
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void RemoveRange_NegativeParameters(int listLength)
        {
            if (listLength % 2 != 0)
                listLength++;
            var list = GenericListFactory(listLength);
            var invalidParameters = new[]
            {
                Tuple.Create(-1,-1),
                Tuple.Create(-1, 0),
                Tuple.Create(-1, 1),
                Tuple.Create(-1, 2),
                Tuple.Create(0 ,-1),
                Tuple.Create(1 ,-1),
                Tuple.Create(2 ,-1),
            };

            Assert.All(invalidParameters, invalidSet =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(invalidSet.Item1, invalidSet.Item2));
            });
        }
    }
}
