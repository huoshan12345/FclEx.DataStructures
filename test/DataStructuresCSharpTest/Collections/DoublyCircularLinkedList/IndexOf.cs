using System;
using System.Collections.Generic;
using System.Linq;
using DataStructuresCSharpTest.Common;
using Xunit;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.DoublyCircularLinkedList
{
    public abstract partial class DoublyCircularLinkedListTests<T> : IListGenericTests<T>
    {
        #region Helpers

        public delegate int IndexOfDelegate(DoublyCircularLinkedList<T> list, T value);
        public enum IndexOfMethod
        {
            IndexOfT,
            IndexOfTInt,
            IndexOfTIntInt,
            LastIndexOfT,
            LastIndexOfTInt,
            LastIndexOfTIntInt,
        };

        private IndexOfDelegate IndexOfDelegateFromType(IndexOfMethod methodType)
        {
            switch (methodType)
            {
                case (IndexOfMethod.IndexOfT):
                return ((list, value) => list.IndexOf(value));
                case (IndexOfMethod.IndexOfTInt):
                return ((list, value) => list.IndexOf(value, 0));
                case (IndexOfMethod.IndexOfTIntInt):
                return ((list, value) => list.IndexOf(value, 0, list.Count));
                case (IndexOfMethod.LastIndexOfT):
                return ((list, value) => list.LastIndexOf(value));
                case (IndexOfMethod.LastIndexOfTInt):
                return ((list, value) => list.LastIndexOf(value, list.Count - 1));
                case (IndexOfMethod.LastIndexOfTIntInt):
                return ((list, value) => list.LastIndexOf(value, list.Count - 1, list.Count));
                default:
                throw new Exception("Invalid IndexOfMethod");
            }
        }

        /// <summary>
        /// MemberData for a Theory to test the IndexOf methods for List. To avoid high code reuse of tests for the 6 IndexOf
        /// methods in List, delegates are used to cover the basic behavioral cases shared by all IndexOf methods. A bool
        /// is used to specify the ordering (front-to-back or back-to-front (e.g. LastIndexOf)) that the IndexOf method
        /// searches in.
        /// </summary>
        public static IEnumerable<object[]> IndexOfTestData()
        {
            foreach (var sizes in ValidCollectionSizes())
            {
                var count = (int)sizes[0];
                yield return new object[] { IndexOfMethod.IndexOfT, count, true };
                yield return new object[] { IndexOfMethod.LastIndexOfT, count, false };

                if (count > 0) // 0 is an invalid index for IndexOf when the count is 0.
                {
                    yield return new object[] { IndexOfMethod.IndexOfTInt, count, true };
                    yield return new object[] { IndexOfMethod.LastIndexOfTInt, count, false };
                    yield return new object[] { IndexOfMethod.IndexOfTIntInt, count, true };
                    yield return new object[] { IndexOfMethod.LastIndexOfTIntInt, count, false };
                }
            }
        }

        #endregion

        #region IndexOf

        [Theory]
        [MemberData(nameof(IndexOfTestData))]
        public void IndexOf_NoDuplicates(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
        {
            var list = GenericListFactory(count);
            var expectedList = list.ToList();
            var indexOf = IndexOfDelegateFromType(indexOfMethod);

            Assert.All(Enumerable.Range(0, count), i =>
            {
                Assert.Equal(i, indexOf(list, expectedList[i]));
            });
        }

        [Theory]
        [MemberData(nameof(IndexOfTestData))]
        public void IndexOf_NonExistingValues(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
        {
            var list = GenericListFactory(count);
            var nonexistentValues = CreateEnumerable(TestBase.EnumerableType.List, list, count, 0, 0);
            var indexOf = IndexOfDelegateFromType(indexOfMethod);

            Assert.All(nonexistentValues, nonexistentValue =>
            {
                Assert.Equal(-1, indexOf(list, nonexistentValue));
            });
        }

        [Theory]
        [MemberData(nameof(IndexOfTestData))]
        public void IndexOf_DefaultValue(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
        {
            var defaultValue = default(T);
            var list = GenericListFactory(count);
            var indexOf = IndexOfDelegateFromType(indexOfMethod);
            while (list.Remove(defaultValue))
                count--;
            list.Add(defaultValue);
            Assert.Equal(count, indexOf(list, defaultValue));
        }

        [Theory]
        [MemberData(nameof(IndexOfTestData))]
        public void IndexOf_OrderIsCorrect(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
        {
            var list = GenericListFactory(count);
            var withoutDuplicates = list.ToList();
            list.AddRange(list);
            var indexOf = IndexOfDelegateFromType(indexOfMethod);

            Assert.All(Enumerable.Range(0, count), i =>
            {
                if (frontToBackOrder)
                    Assert.Equal(i, indexOf(list, withoutDuplicates[i]));
                else
                    Assert.Equal(count + i, indexOf(list, withoutDuplicates[i]));
            });
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IndexOf_int_OrderIsCorrectWithManyDuplicates(int count)
        {
            var list = GenericListFactory(count);
            var withoutDuplicates = list.ToList();
            list.AddRange(list);
            list.AddRange(list);
            list.AddRange(list);

            Assert.All(Enumerable.Range(0, count), i =>
            {
                Assert.All(Enumerable.Range(0, 4), j =>
                {
                    var expectedIndex = (j * count) + i;
                    Assert.Equal(expectedIndex, list.IndexOf(withoutDuplicates[i], (count * j)));
                    Assert.Equal(expectedIndex, list.IndexOf(withoutDuplicates[i], (count * j), count));
                });
            });
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void LastIndexOf_int_OrderIsCorrectWithManyDuplicates(int count)
        {
            var list = GenericListFactory(count);
            var withoutDuplicates = list.ToList();
            list.AddRange(list);
            list.AddRange(list);
            list.AddRange(list);

            Assert.All(Enumerable.Range(0, count), i =>
            {
                Assert.All(Enumerable.Range(0, 4), j =>
                {
                    var expectedIndex = (j * count) + i;
                    Assert.Equal(expectedIndex, list.LastIndexOf(withoutDuplicates[i], (count * (j + 1)) - 1));
                    Assert.Equal(expectedIndex, list.LastIndexOf(withoutDuplicates[i], (count * (j + 1)) - 1, count));
                });
            });
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IndexOf_int_OutOfRangeExceptions(int count)
        {
            var list = GenericListFactory(count);
            var element = CreateT(234);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 1)); //"Expect ArgumentOutOfRangeException for index greater than length of list.."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 10)); //"Expect ArgumentOutOfRangeException for index greater than length of list.."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, -1)); //"Expect ArgumentOutOfRangeException for negative index."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, int.MinValue)); //"Expect ArgumentOutOfRangeException for negative index."
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IndexOf_int_int_OutOfRangeExceptions(int count)
        {
            var list = GenericListFactory(count);
            var element = CreateT(234);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count, 1)); //"ArgumentOutOfRangeException expected on index larger than array."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 1, 1)); //"ArgumentOutOfRangeException expected  on index larger than array."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, count + 1)); //"ArgumentOutOfRangeException expected  on count larger than array."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count / 2, count / 2 + 2)); //"ArgumentOutOfRangeException expected.."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, count + 1)); //"ArgumentOutOfRangeException expected  on count larger than array."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, -1)); //"ArgumentOutOfRangeException expected on negative count."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, -1, 1)); //"ArgumentOutOfRangeException expected on negative index."
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void LastIndexOf_int_OutOfRangeExceptions(int count)
        {
            var list = GenericListFactory(count);
            var element = CreateT(234);
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count)); //"ArgumentOutOfRangeException expected."
            if (count == 0)  // IndexOf with a 0 count List is special cased to return -1.
                Assert.Equal(-1, list.LastIndexOf(element, -1));
            else
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void LastIndexOf_int_int_OutOfRangeExceptions(int count)
        {
            var list = GenericListFactory(count);
            var element = CreateT(234);

            if (count > 0)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, count + 1)); //"Expected ArgumentOutOfRangeException."
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count / 2, count / 2 + 2)); //"Expected ArgumentOutOfRangeException."
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, count + 1)); //"Expected ArgumentOutOfRangeException."
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, -1)); //"Expected ArgumentOutOfRangeException."
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1, count)); //"Expected ArgumentOutOfRangeException."
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1, 1)); //"Expected ArgumentOutOfRangeException."                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count, 0)); //"Expected ArgumentOutOfRangeException."
                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count, 1)); //"Expected ArgumentOutOfRangeException."
            }
            else // IndexOf with a 0 count List is special cased to return -1.
            {
                Assert.Equal(-1, list.LastIndexOf(element, 0, count + 1));
                Assert.Equal(-1, list.LastIndexOf(element, count / 2, count / 2 + 2));
                Assert.Equal(-1, list.LastIndexOf(element, 0, count + 1));
                Assert.Equal(-1, list.LastIndexOf(element, 0, -1));
                Assert.Equal(-1, list.LastIndexOf(element, -1, count));
                Assert.Equal(-1, list.LastIndexOf(element, -1, 1));
                Assert.Equal(-1, list.LastIndexOf(element, count, 0));
                Assert.Equal(-1, list.LastIndexOf(element, count, 1));
            }
        }

        #endregion
    }
}
