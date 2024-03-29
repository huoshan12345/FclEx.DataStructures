﻿using System;
using DataStructuresCSharpTest.Common;
using Xunit;
using System.Linq;

namespace DataStructuresCSharpTest.Collections.SeqList
{
    public abstract partial class SeqListTests<T> : IListGenericTests<T>
    {
        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Reverse(int listLength)
        {
            var list = GenericListFactory(listLength);
            var listBefore = list.ToList();

            list.Reverse();

            for (var i = 0; i < listBefore.Count; i++)
            {
                Assert.Equal(list[i], listBefore[listBefore.Count - (i + 1)]); //"Expect them to be the same."
            }
        }

        [Theory]
        [InlineData(10, 0, 10)]
        [InlineData(10, 3, 3)]
        [InlineData(10, 10, 0)]
        [InlineData(10, 5, 5)]
        [InlineData(10, 0, 5)]
        [InlineData(10, 1, 9)]
        [InlineData(10, 9, 1)]
        [InlineData(10, 2, 8)]
        [InlineData(10, 8, 2)]
        public void Reverse_int_int(int listLength, int index, int count)
        {
            var list = GenericListFactory(listLength);
            var listBefore = list.ToList();

            list.Reverse(index, count);

            for (var i = 0; i < index; i++)
            {
                Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
            }

            var j = 0;
            for (var i = index; i < index + count; i++)
            {
                Assert.Equal(list[i], listBefore[index + count - (j + 1)]); //"Expect them to be the same."
                j++;
            }

            for (var i = index + count; i < listBefore.Count; i++)
            {
                Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
            }
        }

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
        public void Reverse_RepeatedValues(int listLength, int index, int count)
        {
            var list = GenericListFactory(1);
            for (var i = 1; i < listLength; i++)
                list.Add(list[0]);
            var listBefore = list.ToList();

            list.Reverse(index, count);

            for (var i = 0; i < index; i++)
            {
                Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
            }

            var j = 0;
            for (var i = index; i < index + count; i++)
            {
                Assert.Equal(list[i], listBefore[index + count - (j + 1)]); //"Expect them to be the same."
                j++;
            }

            for (var i = index + count; i < listBefore.Count; i++)
            {
                Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Reverse_InvalidParameters(int listLength)
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
                    Assert.Throws<ArgumentException>(() => list.Reverse(invalidSet.Item1, invalidSet.Item2));
            });
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Reverse_NegativeParameters(int listLength)
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
                Assert.Throws<ArgumentOutOfRangeException>(() => list.Reverse(invalidSet.Item1, invalidSet.Item2));
            });
        }
    }
}
