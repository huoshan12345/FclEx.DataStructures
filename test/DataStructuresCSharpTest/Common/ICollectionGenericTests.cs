using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataStructuresCSharpTest.Common
{
    /// <summary>
    /// Contains tests that ensure the correctness of any class that implements the generic
    /// ICollection interface
    /// </summary>
    public abstract class ICollectionGenericTests<T> : IEnumerableGenericTests<T>
    {
        #region ICollection<T> Helper Methods

        /// <summary>
        /// Creates an instance of an ICollection{T} that can be used for testing.
        /// </summary>
        /// <returns>An instance of an ICollection{T} that can be used for testing.</returns>
        protected abstract ICollection<T> GenericICollectionFactory();

        /// <summary>
        /// Creates an instance of an ICollection{T} that can be used for testing.
        /// </summary>
        /// <param name="count">The number of unique items that the returned ICollection{T} contains.</param>
        /// <returns>An instance of an ICollection{T} that can be used for testing.</returns>
        protected virtual ICollection<T> GenericICollectionFactory(int count)
        {
            var collection = GenericICollectionFactory();
            AddToCollection(collection, count);
            return collection;
        }

        protected virtual bool DuplicateValuesAllowed => true;
        protected virtual bool DefaultValueWhenNotAllowedThrows => true;
        protected virtual bool IsReadOnly => false;
        protected virtual bool DefaultValueAllowed => true;
        protected virtual IEnumerable<T> InvalidValues => new T[0];

        protected virtual void AddToCollection(ICollection<T> collection, int numberOfItemsToAdd)
        {
            var seed = 9600;
            var comparer = GetIEqualityComparer();
            while (collection.Count < numberOfItemsToAdd)
            {
                var toAdd = CreateT(seed++);
                while (collection.Contains(toAdd, comparer) || InvalidValues.Contains(toAdd, comparer))
                    toAdd = CreateT(seed++);
                collection.Add(toAdd);
            }
        }

        #endregion

        #region IEnumerable<T> Helper Methods

        protected override IEnumerable<T> GenericIEnumerableFactory(int count)
        {
            return GenericICollectionFactory(count);
        }

        /// <summary>
        /// Returns a set of ModifyEnumerable delegates that modify the enumerable passed to them.
        /// </summary>
        protected override IEnumerable<ModifyEnumerable> ModifyEnumerables
        {
            get
            {
                yield return (IEnumerable<T> enumerable) => {
                    var casted = (ICollection<T>)enumerable;
                    casted.Add(CreateT(2344));
                    return true;
                };
                yield return (IEnumerable<T> enumerable) => {
                    var casted = (ICollection<T>)enumerable;
                    if (casted.Count() > 0)
                    {
                        casted.Remove(casted.ElementAt(0));
                        return true;
                    }
                    return false;
                };
                yield return (IEnumerable<T> enumerable) => {
                    var casted = (ICollection<T>)enumerable;
                    if (casted.Count() > 0)
                    {
                        casted.Clear();
                        return true;
                    }
                    return false;
                };
            }
        }

        #endregion

        #region IsReadOnly

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_IsReadOnly_Validity(int count)
        {
            var collection = GenericICollectionFactory(count);
            Assert.Equal(IsReadOnly, collection.IsReadOnly);
        }

        #endregion

        #region Count

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Count_Validity(int count)
        {
            var collection = GenericICollectionFactory(count);
            Assert.Equal(count, collection.Count);
        }

        #endregion

        #region Add

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_DefaultValue(int count)
        {
            if (DefaultValueAllowed && !IsReadOnly)
            {
                var collection = GenericICollectionFactory(count);
                collection.Add(default(T));
                Assert.Equal(count + 1, collection.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_InvalidValueToMiddleOfCollection(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, invalidValue =>
                {
                    var collection = GenericICollectionFactory(count);
                    collection.Add(invalidValue);
                    for (var i = 0; i < count; i++)
                        collection.Add(CreateT(i));
                    Assert.Equal(count * 2, collection.Count);
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_InvalidValueToBeginningOfCollection(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, invalidValue =>
                {
                    var collection = GenericICollectionFactory(0);
                    collection.Add(invalidValue);
                    for (var i = 0; i < count; i++)
                        collection.Add(CreateT(i));
                    Assert.Equal(count, collection.Count);
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_InvalidValueToEndOfCollection(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, invalidValue =>
                {
                    var collection = GenericICollectionFactory(count);
                    collection.Add(invalidValue);
                    Assert.Equal(count, collection.Count);
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_DuplicateValue(int count)
        {
            if (!IsReadOnly && DuplicateValuesAllowed)
            {
                var collection = GenericICollectionFactory(count);
                var duplicateValue = CreateT(700);
                collection.Add(duplicateValue);
                collection.Add(duplicateValue);
                Assert.Equal(count + 2, collection.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_AfterCallingClear(int count)
        {
            if (IsReadOnly) return;
            var collection = GenericICollectionFactory(count);
            collection.Clear();
            AddToCollection(collection, 5);
            Assert.Equal(5, collection.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_AfterRemovingAnyValue(int count)
        {
            if (IsReadOnly) return;
            var seed = 840;
            var collection = GenericICollectionFactory(count);
            var items = collection.ToList();
            var toAdd = CreateT(seed++);
            while (collection.Contains(toAdd))
                toAdd = CreateT(seed++);
            collection.Add(toAdd);
            collection.Remove(toAdd);

            toAdd = CreateT(seed++);
            while (collection.Contains(toAdd))
                toAdd = CreateT(seed++);

            collection.Add(toAdd);
            items.Add(toAdd);
            CollectionAsserts.EqualUnordered(items, collection);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_AfterRemovingAllItems(int count)
        {
            if (IsReadOnly) return;
            var collection = GenericICollectionFactory(count);
            for (var i = 0; i < count; i++)
                Assert.True(collection.Remove(collection.ElementAt(0)));
            collection.Add(CreateT(254));
            Assert.Equal(1, collection.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_ToReadOnlyCollection(int count)
        {
            if (!IsReadOnly) return;
            var collection = GenericICollectionFactory(count);
            Assert.Throws<NotSupportedException>(() => collection.Add(CreateT(0)));
            Assert.Equal(count, collection.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Add_AfterRemoving(int count)
        {
            if (IsReadOnly) return;
            var seed = 840;
            var collection = GenericICollectionFactory(count);
            var toAdd = CreateT(seed++);
            while (collection.Contains(toAdd))
                toAdd = CreateT(seed++);
            collection.Add(toAdd);
            collection.Remove(toAdd);
            collection.Add(toAdd);
        }

        #endregion

        #region Clear

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Clear(int count)
        {
            var collection = GenericICollectionFactory(count);
            if (IsReadOnly)
            {
                Assert.Throws<NotSupportedException>(() => collection.Clear());
                Assert.Equal(count, collection.Count);
            }
            else
            {
                collection.Clear();
                Assert.Equal(0, collection.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Clear_Repeatedly(int count)
        {
            var collection = GenericICollectionFactory(count);
            if (IsReadOnly)
            {
                Assert.Throws<NotSupportedException>(() => collection.Clear());
                Assert.Throws<NotSupportedException>(() => collection.Clear());
                Assert.Throws<NotSupportedException>(() => collection.Clear());
                Assert.Equal(count, collection.Count);
            }
            else
            {
                collection.Clear();
                collection.Clear();
                collection.Clear();
                Assert.Equal(0, collection.Count);
            }
        }

        #endregion

        #region Contains

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Contains_ValidValueOnCollectionNotContainingThatValue(int count)
        {
            var collection = GenericICollectionFactory(count);
            var seed = 4315;
            var item = CreateT(seed++);
            while (collection.Contains(item))
                item = CreateT(seed++);
            Assert.False(collection.Contains(item));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Contains_ValidValueOnCollectionContainingThatValue(int count)
        {
            var collection = GenericICollectionFactory(count);
            foreach (var item in collection)
                Assert.True(collection.Contains(item));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Contains_DefaultValueOnCollectionNotContainingDefaultValue(int count)
        {
            var collection = GenericICollectionFactory(count);
            if (DefaultValueAllowed)
                Assert.False(collection.Contains(default(T)));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Contains_DefaultValueOnCollectionContainingDefaultValue(int count)
        {
            var collection = GenericICollectionFactory(count);
            if (DefaultValueAllowed && !IsReadOnly)
            {
                collection.Add(default(T));
                Assert.True(collection.Contains(default(T)));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Contains_ValidValueThatExistsTwiceInTheCollection(int count)
        {
            if (DuplicateValuesAllowed && !IsReadOnly)
            {
                var collection = GenericICollectionFactory(count);
                var item = CreateT(12);
                collection.Add(item);
                collection.Add(item);
                Assert.Equal(count + 2, collection.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Contains_InvalidValue_ThrowsArgumentException(int count)
        {
            var collection = GenericICollectionFactory(count);
            Assert.All(InvalidValues, invalidValue =>
                Assert.Throws<ArgumentException>(() => collection.Contains(invalidValue))
            );
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public virtual void ICollection_Generic_Contains_DefaultValueWhenNotAllowed(int count)
        {
            var collection = GenericICollectionFactory(count);
            if (!DefaultValueAllowed && !IsReadOnly)
            {
                if (DefaultValueWhenNotAllowedThrows)
                    Assert.ThrowsAny<ArgumentNullException>(() => collection.Contains(default(T)));
                else
                    Assert.False(collection.Contains(default(T)));
            }
        }

        #endregion

        #region CopyTo

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_CopyTo_NullArray_ThrowsArgumentNullException(int count)
        {
            var collection = GenericICollectionFactory(count);
            Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException(int count)
        {
            var collection = GenericICollectionFactory(count);
            var array = new T[count];
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, int.MinValue));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_CopyTo_IndexEqualToArrayCount_ThrowsArgumentException(int count)
        {
            var collection = GenericICollectionFactory(count);
            var array = new T[count];
            if (count > 0)
                Assert.Throws<ArgumentException>(() => collection.CopyTo(array, count));
            else
                collection.CopyTo(array, count); // does nothing since the array is empty
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_CopyTo_IndexLargerThanArrayCount_ThrowsAnyArgumentException(int count)
        {
            var collection = GenericICollectionFactory(count);
            var array = new T[count];
            Assert.ThrowsAny<ArgumentException>(() => collection.CopyTo(array, count + 1)); // some implementations throw ArgumentOutOfRangeException for this scenario
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_CopyTo_NotEnoughSpaceInOffsettedArray_ThrowsArgumentException(int count)
        {
            if (count > 0) // Want the T array to have at least 1 element
            {
                var collection = GenericICollectionFactory(count);
                var array = new T[count];
                Assert.Throws<ArgumentException>(() => collection.CopyTo(array, 1));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_CopyTo_ExactlyEnoughSpaceInArray(int count)
        {
            var collection = GenericICollectionFactory(count);
            var array = new T[count];
            collection.CopyTo(array, 0);
            Assert.True(collection.SequenceEqual(array));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_CopyTo_ArrayIsLargerThanCollection(int count)
        {
            var collection = GenericICollectionFactory(count);
            var array = new T[count * 3 / 2];
            collection.CopyTo(array, 0);
            Assert.True(collection.SequenceEqual(array.Take(count)));
        }

        #endregion

        #region Remove

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_OnReadOnlyCollection_ThrowsNotSupportedException(int count)
        {
            if (!IsReadOnly) return;
            var collection = GenericICollectionFactory(count);
            Assert.Throws<NotSupportedException>(() => collection.Remove(CreateT(34543)));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_DefaultValueNotContainedInCollection(int count)
        {
            if (!IsReadOnly && DefaultValueAllowed && !InvalidValues.Contains(default(T)))
            {
                var seed = count * 21;
                var collection = GenericICollectionFactory(count);
                var value = default(T);
                while (collection.Contains(value))
                {
                    collection.Remove(value);
                    count--;
                }
                Assert.False(collection.Remove(value));
                Assert.Equal(count, collection.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_NonDefaultValueNotContainedInCollection(int count)
        {
            if (IsReadOnly) return;
            var seed = count * 251;
            var collection = GenericICollectionFactory(count);
            var value = CreateT(seed++);
            while (collection.Contains(value) || InvalidValues.Contains(value))
                value = CreateT(seed++);
            Assert.False(collection.Remove(value));
            Assert.Equal(count, collection.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_DefaultValueContainedInCollection(int count)
        {
            if (!IsReadOnly && DefaultValueAllowed && !InvalidValues.Contains(default(T)))
            {
                var seed = count * 21;
                var collection = GenericICollectionFactory(count);
                var value = default(T);
                if (!collection.Contains(value))
                {
                    collection.Add(value);
                    count++;
                }
                Assert.True(collection.Remove(value));
                Assert.Equal(count - 1, collection.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_NonDefaultValueContainedInCollection(int count)
        {
            if (IsReadOnly) return;
            var seed = count * 251;
            var collection = GenericICollectionFactory(count);
            var value = CreateT(seed++);
            if (!collection.Contains(value))
            {
                collection.Add(value);
                count++;
            }
            Assert.True(collection.Remove(value));
            Assert.Equal(count - 1, collection.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_ValueThatExistsTwiceInCollection(int count)
        {
            if (!IsReadOnly && DuplicateValuesAllowed)
            {
                var seed = count * 90;
                var collection = GenericICollectionFactory(count);
                var value = CreateT(seed++);
                collection.Add(value);
                collection.Add(value);
                count += 2;
                Assert.True(collection.Remove(value));
                Assert.True(collection.Contains(value));
                Assert.Equal(count - 1, collection.Count);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_EveryValue(int count)
        {
            if (IsReadOnly) return;
            var collection = GenericICollectionFactory(count);
            var list = collection.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                Assert.True(collection.Remove(list[i]));
            }
            Assert.Empty(collection);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_EveryValue_Unordered(int count)
        {
            if (IsReadOnly) return;
            var collection = GenericICollectionFactory(count);
            var list = collection.OrderBy(item=>item.GetHashCode()).ToList();
            foreach (var t in list)
            {
                Assert.True(collection.Remove(t));
            }
            Assert.Empty(collection);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_EveryValue_Backward(int count)
        {
            if (!IsReadOnly)
            {
                var collection = GenericICollectionFactory(count);
                var arr = collection.ToArray();
                Array.Reverse(arr);
                foreach (var t in arr)
                {
                    Assert.True(collection.Remove(t));
                }
                Assert.Empty(collection);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_InvalidValue_ThrowsArgumentException(int count)
        {
            var collection = GenericICollectionFactory(count);
            foreach (var value in InvalidValues)
            {
                Assert.ThrowsAny<ArgumentException>(() => collection.Remove(value));
            }
            Assert.Equal(count, collection.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_Generic_Remove_DefaultValueWhenNotAllowed(int count)
        {
            var collection = GenericICollectionFactory(count);
            if (!DefaultValueAllowed && !IsReadOnly)
            {
                if (DefaultValueWhenNotAllowedThrows)
                    Assert.ThrowsAny<ArgumentNullException>(() => collection.Remove(default(T)));
                else
                    Assert.False(collection.Remove(default(T)));
            }
        }

        #endregion
    }
}
