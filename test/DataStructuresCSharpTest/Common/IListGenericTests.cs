﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataStructuresCSharpTest.Common
{
    /// <summary>
    /// Contains tests that ensure the correctness of any class that implements the generic
    /// IList interface
    /// </summary>
    // ReSharper disable InconsistentNaming
    public abstract class IListGenericTests<T> : ICollectionGenericTests<T>
    {
        #region IList<T> Helper Methods

        /// <summary>
        /// Creates an instance of an IList{T} that can be used for testing.
        /// </summary>
        /// <returns>An instance of an IList{T} that can be used for testing.</returns>
        protected abstract IList<T> GenericIListFactory();

        /// <summary>
        /// Creates an instance of an IList{T} that can be used for testing.
        /// </summary>
        /// <param name="count">The number of unique items that the returned IList{T} contains.</param>
        /// <returns>An instance of an IList{T} that can be used for testing.</returns>
        protected virtual IList<T> GenericIListFactory(int count)
        {
            var collection = GenericIListFactory();
            AddToCollection(collection, count);
            return collection;
        }

        /// <summary>
        /// Returns a set of ModifyEnumerable delegates that modify the enumerable passed to them.
        /// </summary>
        protected override IEnumerable<ModifyEnumerable> ModifyEnumerables
        {
            get
            {
                yield return (IEnumerable<T> enumerable) => {
                    var casted = ((IList<T>)enumerable);
                    casted.Add(CreateT(2344));
                    return true;
                };
                yield return (IEnumerable<T> enumerable) => {
                    var casted = ((IList<T>)enumerable);
                    if (casted.Count > 0)
                    {
                        casted.Insert(0, CreateT(12));
                        return true;
                    }
                    return false;
                };
                yield return (IEnumerable<T> enumerable) => {
                    var casted = ((IList<T>)enumerable);
                    if (casted.Count > 0)
                    {
                        casted[0] = CreateT(12);
                        return true;
                    }
                    return false;
                };

                yield return (IEnumerable<T> enumerable) => {
                    var casted = ((IList<T>)enumerable);
                    if (casted.Count > 0)
                    {
                        return casted.Remove(casted[0]);
                    }
                    return false;
                };
                yield return (IEnumerable<T> enumerable) => {
                    var casted = ((IList<T>)enumerable);
                    if (casted.Count > 0)
                    {
                        casted.RemoveAt(0);
                        return true;
                    }
                    return false;
                };
                yield return (IEnumerable<T> enumerable) => {
                    var casted = ((IList<T>)enumerable);
                    if (casted.Count > 0)
                    {
                        casted.Clear();
                        return true;
                    }
                    return false;
                };
            }
        }

        #endregion

        #region ICollection<T> Helper Methods

        protected override bool DefaultValueWhenNotAllowedThrows { get { return false; } }

        protected override ICollection<T> GenericICollectionFactory()
        {
            return GenericIListFactory();
        }

        protected override ICollection<T> GenericICollectionFactory(int count)
        {
            return GenericIListFactory(count);
        }

        #endregion

        #region Item Getter

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemGet_NegativeIndex_ThrowsArgumentOutOfRangeException(int count)
        {
            var list = GenericIListFactory(count);
            Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => list[int.MinValue]);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemGet_IndexGreaterThanListCount_ThrowsArgumentOutOfRangeException(int count)
        {
            var list = GenericIListFactory(count);
            Assert.Throws<ArgumentOutOfRangeException>(() => list[count]);
            Assert.Throws<ArgumentOutOfRangeException>(() => list[count + 1]);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemGet_ValidGetWithinListBounds(int count)
        {
            var list = GenericIListFactory(count);
            T result;
            Assert.All(Enumerable.Range(0, count), index => result = list[index]);
        }

        #endregion

        #region Item Setter

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_NegativeIndex_ThrowsArgumentOutOfRangeException(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var validAdd = CreateT(0);
                Assert.Throws<ArgumentOutOfRangeException>(() => list[-1] = validAdd);
                Assert.Throws<ArgumentOutOfRangeException>(() => list[int.MinValue] = validAdd);
                Assert.Equal(count, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_IndexGreaterThanListCount_ThrowsArgumentOutOfRangeException(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var validAdd = CreateT(0);
                Assert.Throws<ArgumentOutOfRangeException>(() => list[count] = validAdd);
                Assert.Throws<ArgumentOutOfRangeException>(() => list[count + 1] = validAdd);
                Assert.Equal(count, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_OnReadOnlyList(int count)
        {
            if (IsReadOnly && count > 0)
            {
                var list = GenericIListFactory(count);
                var before = list[count / 2];
                Assert.Throws<NotSupportedException>(() => list[count / 2] = CreateT(321432));
                Assert.Equal(before, list[count / 2]);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_FirstItemToNonDefaultValue(int count)
        {
            if (count > 0 && !IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var value = CreateT(123452);
                list[0] = value;
                Assert.Equal(value, list[0]);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_FirstItemToDefaultValue(int count)
        {
            if (count > 0 && !IsReadOnly)
            {
                var list = GenericIListFactory(count);
                if (DefaultValueAllowed)
                {
                    list[0] = default(T);
                    Assert.Equal(default(T), list[0]);
                }
                else
                {
                    Assert.Throws<ArgumentNullException>(() => list[0] = default(T));
                    Assert.NotEqual(default(T), list[0]);
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_LastItemToNonDefaultValue(int count)
        {
            if (count > 0 && !IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var value = CreateT(123452);
                var lastIndex = count > 0 ? count - 1 : 0;
                list[lastIndex] = value;
                Assert.Equal(value, list[lastIndex]);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_LastItemToDefaultValue(int count)
        {
            if (count > 0 && !IsReadOnly && DefaultValueAllowed)
            {
                var list = GenericIListFactory(count);
                var lastIndex = count > 0 ? count - 1 : 0;
                if (DefaultValueAllowed)
                {
                    list[lastIndex] = default(T);
                    Assert.Equal(default(T), list[lastIndex]);
                }
                else
                {
                    Assert.Throws<ArgumentNullException>(() => list[lastIndex] = default(T));
                    Assert.NotEqual(default(T), list[lastIndex]);
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_DuplicateValues(int count)
        {
            if (count >= 2 && !IsReadOnly && DuplicateValuesAllowed)
            {
                var list = GenericIListFactory(count);
                var value = CreateT(123452);
                list[0] = value;
                list[1] = value;
                Assert.Equal(value, list[0]);
                Assert.Equal(value, list[1]);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_ItemSet_InvalidValue(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, value =>
                {
                    var list = GenericIListFactory(count);
                    Assert.Throws<ArgumentException>(() => list[count / 2] = value);
                });
            }
        }

        #endregion

        #region IndexOf

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_IndexOf_DefaultValueNotContainedInList(int count)
        {
            if (DefaultValueAllowed)
            {
                var list = GenericIListFactory(count);
                var value = default(T);
                if (list.Contains(value))
                {
                    if (IsReadOnly)
                        return;
                    list.Remove(value);
                }
                Assert.Equal(-1, list.IndexOf(value));
            }
            else
            {
                var list = GenericIListFactory(count);
                Assert.Throws<ArgumentNullException>(() => list.IndexOf(default(T)));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_IndexOf_DefaultValueContainedInList(int count)
        {
            if (count > 0 && DefaultValueAllowed)
            {
                var list = GenericIListFactory(count);
                var value = default(T);
                if (!list.Contains(value))
                {
                    if (IsReadOnly)
                        return;
                    list[0] = value;
                }
                Assert.Equal(0, list.IndexOf(value));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_IndexOf_ValidValueNotContainedInList(int count)
        {
            var list = GenericIListFactory(count);
            var seed = 54321;
            var value = CreateT(seed++);
            while (list.Contains(value))
                value = CreateT(seed++);
            Assert.Equal(-1, list.IndexOf(value));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_IndexOf_ValueInCollectionMultipleTimes(int count)
        {
            if (count > 0 && !IsReadOnly && DuplicateValuesAllowed)
            {
                // IndexOf should always return the lowest index for which a matching element is found
                var list = GenericIListFactory(count);
                var value = CreateT(12345);
                list[0] = value;
                list[count / 2] = value;
                Assert.Equal(0, list.IndexOf(value));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_IndexOf_EachValueNoDuplicates(int count)
        {
            // Assumes no duplicate elements contained in the list returned by GenericIListFactory
            var list = GenericIListFactory(count);
            Assert.All(Enumerable.Range(0, count), index =>
            {
                Assert.Equal(index, list.IndexOf(list[index]));
            });
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_IndexOf_InvalidValue(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, value =>
                {
                    var list = GenericIListFactory(count);
                    Assert.Throws<ArgumentException>(() => list.IndexOf(value));
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_IndexOf_ReturnsFirstMatchingValue(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                foreach (var duplicate in list.ToList()) // hard copies list to circumvent enumeration error
                    list.Add(duplicate);
                var expectedList = list.ToList();

                Assert.All(Enumerable.Range(0, count), (index =>
                    Assert.Equal(index, list.IndexOf(expectedList[index]))
                ));
            }
        }

        #endregion

        #region Insert

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_NegativeIndex_ThrowsArgumentOutOfRangeException(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var validAdd = CreateT(0);
                Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, validAdd));
                Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(int.MinValue, validAdd));
                Assert.Equal(count, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_IndexGreaterThanListCount_Appends(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var validAdd = CreateT(12350);
                list.Insert(count, validAdd);
                Assert.Equal(count + 1, list.Count);
                Assert.Equal(validAdd, list[count]);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_ToReadOnlyList(int count)
        {
            if (IsReadOnly)
            {
                var list = GenericIListFactory(count);
                Assert.Throws<NotSupportedException>(() => list.Insert(count / 2, CreateT(321432)));
                Assert.Equal(count, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_FirstItemToNonDefaultValue(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var value = CreateT(123452);
                list.Insert(0, value);
                Assert.Equal(value, list[0]);
                Assert.Equal(count + 1, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_FirstItemToDefaultValue(int count)
        {
            if (!IsReadOnly && DefaultValueAllowed)
            {
                var list = GenericIListFactory(count);
                var value = default(T);
                list.Insert(0, value);
                Assert.Equal(value, list[0]);
                Assert.Equal(count + 1, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_LastItemToNonDefaultValue(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var value = CreateT(123452);
                var lastIndex = count > 0 ? count - 1 : 0;
                list.Insert(lastIndex, value);
                Assert.Equal(value, list[lastIndex]);
                Assert.Equal(count + 1, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_LastItemToDefaultValue(int count)
        {
            if (!IsReadOnly && DefaultValueAllowed)
            {
                var list = GenericIListFactory(count);
                var value = default(T);
                var lastIndex = count > 0 ? count - 1 : 0;
                list.Insert(lastIndex, value);
                Assert.Equal(value, list[lastIndex]);
                Assert.Equal(count + 1, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_DuplicateValues(int count)
        {
            if (!IsReadOnly && DuplicateValuesAllowed)
            {
                var list = GenericIListFactory(count);
                var value = CreateT(123452);
                list.Insert(0, value);
                list.Insert(1, value);
                Assert.Equal(value, list[0]);
                Assert.Equal(value, list[1]);
                Assert.Equal(count + 2, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_Insert_InvalidValue(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, value =>
                {
                    var list = GenericIListFactory(count);
                    Assert.Throws<ArgumentException>(() => list.Insert(count / 2, value));
                });
            }
        }

        #endregion

        #region RemoveAt

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var validAdd = CreateT(0);
                Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
                Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(int.MinValue));
                Assert.Equal(count, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_RemoveAt_IndexGreaterThanListCount_ThrowsArgumentOutOfRangeException(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                var validAdd = CreateT(0);
                Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(count));
                Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(count + 1));
                Assert.Equal(count, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_RemoveAt_OnReadOnlyList(int count)
        {
            if (IsReadOnly)
            {
                var list = GenericIListFactory(count);
                Assert.Throws<NotSupportedException>(() => list.RemoveAt(count / 2));
                Assert.Equal(count, list.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_RemoveAt_AllValidIndices(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                Assert.Equal(count, list.Count);
                Assert.All(Enumerable.Range(0, count).Reverse(), index =>
                {
                    list.RemoveAt(index);
                    Assert.Equal(index, list.Count);
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IList_Generic_RemoveAt_ZeroMultipleTimes(int count)
        {
            if (!IsReadOnly)
            {
                var list = GenericIListFactory(count);
                Assert.All(Enumerable.Range(0, count), index =>
                {
                    list.RemoveAt(0);
                    Assert.Equal(count - index - 1, list.Count);
                });
            }
        }

        #endregion
    }
}
