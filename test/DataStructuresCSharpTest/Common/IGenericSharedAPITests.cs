using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataStructuresCSharpTest.Common
{
    // ReSharper disable InconsistentNaming
    public abstract class IGenericSharedApiTests<T> : IEnumerableGenericTests<T>
    {
        #region IGenericSharedAPI<T> Helper methods

        protected virtual bool DuplicateValuesAllowed { get { return true; } }
        protected virtual bool DefaultValueWhenNotAllowed_Throws { get { return true; } }
        protected virtual bool IsReadOnly { get { return false; } }
        protected virtual bool DefaultValueAllowed { get { return true; } }
        protected virtual IEnumerable<T> InvalidValues { get { return new T[0]; } }

        protected virtual void AddToCollection(IEnumerable<T> collection, int numberOfItemsToAdd)
        {
            var seed = 9600;
            var comparer = GetIEqualityComparer();
            while (Count(collection) < numberOfItemsToAdd)
            {
                var toAdd = CreateT(seed++);
                while (collection.Contains(toAdd, comparer) || InvalidValues.Contains(toAdd, comparer))
                    toAdd = CreateT(seed++);
                Add(collection, toAdd);
            }
        }

        // There are a number of methods shared between Queue, and Stack for which there is no
        // common interface. To enable high code reuse, delegates are used to defer to those methods for 
        // checking validity.
        protected abstract int Count(IEnumerable<T> enumerable);
        protected abstract void Add(IEnumerable<T> enumerable, T value);
        protected abstract void Clear(IEnumerable<T> enumerable);
        protected abstract bool Contains(IEnumerable<T> enumerable, T value);
        protected abstract void CopyTo(IEnumerable<T> enumerable, T[] array, int index);
        protected abstract bool Remove(IEnumerable<T> enumerable);

        #endregion

        #region IEnumerable<T> helper methods

        protected override IEnumerable<T> GenericIEnumerableFactory(int count)
        {
            var collection = GenericIEnumerableFactory();
            AddToCollection(collection, count);
            return collection;
        }

        protected abstract IEnumerable<T> GenericIEnumerableFactory();

        /// <summary>
        /// Returns a set of ModifyEnumerable delegates that modify the enumerable passed to them.
        /// </summary>
        protected override IEnumerable<ModifyEnumerable> ModifyEnumerables
        {
            get
            {
                yield return (IEnumerable<T> enumerable) => {
                    Add(enumerable, CreateT(12));
                    return true;
                };
                yield return (IEnumerable<T> enumerable) => {
                    if (Count(enumerable) > 0)
                    {
                        return Remove(enumerable);
                    }
                    return false;
                };
                yield return (IEnumerable<T> enumerable) => {
                    if (Count(enumerable) > 0)
                    {
                        Clear(enumerable);
                        return true;
                    }
                    return false;
                };
            }
        }
        #endregion

        #region Count

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Count_Validity(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            Assert.Equal(count, Count(collection));
        }

        #endregion

        #region Add

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_DefaultValue(int count)
        {
            if (DefaultValueAllowed && !IsReadOnly)
            {
                var collection = GenericIEnumerableFactory(count);
                Add(collection, default(T));
                Assert.Equal(count + 1, Count(collection));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_InvalidValueToMiddleOfCollection(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, invalidValue =>
                {
                    var collection = GenericIEnumerableFactory(count);
                    Add(collection, invalidValue);
                    for (var i = 0; i < count; i++)
                        Add(collection, CreateT(i));
                    Assert.Equal(count * 2, Count(collection));
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_InvalidValueToBeginningOfCollection(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, invalidValue =>
                {
                    var collection = GenericIEnumerableFactory(0);
                    Add(collection, invalidValue);
                    for (var i = 0; i < count; i++)
                        Add(collection, CreateT(i));
                    Assert.Equal(count, Count(collection));
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_InvalidValueToEndOfCollection(int count)
        {
            if (!IsReadOnly)
            {
                Assert.All(InvalidValues, invalidValue =>
                {
                    var collection = GenericIEnumerableFactory(count);
                    Add(collection, invalidValue);
                    Assert.Equal(count, Count(collection));
                });
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_DuplicateValue(int count)
        {
            if (!IsReadOnly)
            {
                if (DuplicateValuesAllowed)
                {
                    var collection = GenericIEnumerableFactory(count);
                    var duplicateValue = CreateT(700);
                    Add(collection, duplicateValue);
                    Add(collection, duplicateValue);
                    Assert.Equal(count + 2, Count(collection));
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_AfterCallingClear(int count)
        {
            if (!IsReadOnly)
            {
                var collection = GenericIEnumerableFactory(count);
                Clear(collection);
                AddToCollection(collection, 5);
                Assert.Equal(5, Count(collection));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_AfterRemovingAnyValue(int count)
        {
            if (!IsReadOnly)
            {
                var seed = 840;
                var collection = GenericIEnumerableFactory(count);
                var items = collection.ToList();
                var toAdd = CreateT(seed++);
                while (Contains(collection, toAdd))
                    toAdd = CreateT(seed++);
                Add(collection, toAdd);
                Remove(collection);

                toAdd = CreateT(seed++);
                while (Contains(collection, toAdd))
                    toAdd = CreateT(seed++);

                Add(collection, toAdd);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Add_AfterRemovingAllItems(int count)
        {
            if (!IsReadOnly)
            {
                var collection = GenericIEnumerableFactory(count);
                var itemsToRemove = collection.ToList();
                for (var i = 0; i < count; i++)
                    Remove(collection);
                Add(collection, CreateT(254));
                Assert.Equal(1, Count(collection));
            }
        }

        #endregion

        #region Clear

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Clear(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            if (IsReadOnly)
            {
                Assert.Throws<NotSupportedException>(() => Clear(collection));
                Assert.Equal(count, Count(collection));
            }
            else
            {
                Clear(collection);
                Assert.Equal(0, Count(collection));
            }
        }

        #endregion

        #region Contains

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Contains_ValidValueOnCollectionNotContainingThatValue(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            var seed = 4315;
            var item = CreateT(seed++);
            while (Contains(collection, item))
                item = CreateT(seed++);
            Assert.False(Contains(collection, item));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_IGenericSharedAPI_Contains_ValidValueOnCollectionContainingThatValue(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            foreach (var item in collection)
                Assert.True(Contains(collection, item));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Contains_DefaultValueOnCollectionNotContainingDefaultValue(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            if (DefaultValueAllowed)
                Assert.False(Contains(collection, default(T)));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Contains_DefaultValueOnCollectionContainingDefaultValue(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            if (DefaultValueAllowed && !IsReadOnly)
            {
                Add(collection, default(T));
                Assert.True(Contains(collection, default(T)));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Contains_ValidValueThatExistsTwiceInTheCollection(int count)
        {
            if (DuplicateValuesAllowed && !IsReadOnly)
            {
                var collection = GenericIEnumerableFactory(count);
                var item = CreateT(12);
                Add(collection, item);
                Add(collection, item);
                Assert.Equal(count + 2, Count(collection));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_Contains_InvalidValue_ThrowsArgumentException(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            Assert.All(InvalidValues, invalidValue =>
                Assert.Throws<ArgumentException>(() => Contains(collection, invalidValue))
            );
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public virtual void IGenericSharedAPI_Contains_DefaultValueWhenNotAllowed(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            if (!DefaultValueAllowed && !IsReadOnly)
            {
                if (DefaultValueWhenNotAllowed_Throws)
                    Assert.ThrowsAny<ArgumentNullException>(() => Contains(collection, default(T)));
                else
                    Assert.False(Contains(collection, default(T)));
            }
        }

        #endregion

        #region CopyTo

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_CopyTo_NullArray_ThrowsArgumentNullException(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            Assert.Throws<ArgumentNullException>(() => CopyTo(collection, null, 0));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            var array = new T[count];
            Assert.Throws<ArgumentOutOfRangeException>(() => CopyTo(collection, array, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CopyTo(collection, array, int.MinValue));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_CopyTo_IndexEqualToArrayCount_ThrowsArgumentException(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            var array = new T[count];
            if (count > 0)
                Assert.Throws<ArgumentException>(() => CopyTo(collection, array, count));
            else
                CopyTo(collection, array, count); // does nothing since the array is empty
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_CopyTo_IndexLargerThanArrayCount_ThrowsAnyArgumentException(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            var array = new T[count];
            Assert.ThrowsAny<ArgumentException>(() => CopyTo(collection, array, count + 1)); // some implementations throw ArgumentOutOfRangeException for this scenario
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_CopyTo_NotEnoughSpaceInOffsettedArray_ThrowsArgumentException(int count)
        {
            if (count > 0) // Want the T array to have at least 1 element
            {
                var collection = GenericIEnumerableFactory(count);
                var array = new T[count];
                Assert.Throws<ArgumentException>(() => CopyTo(collection, array, 1));
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_CopyTo_ExactlyEnoughSpaceInArray(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            var array = new T[count];
            CopyTo(collection, array, 0);
            Assert.True(Enumerable.SequenceEqual(collection, array));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IGenericSharedAPI_CopyTo_ArrayIsLargerThanCollection(int count)
        {
            var collection = GenericIEnumerableFactory(count);
            var array = new T[count * 3 / 2];
            CopyTo(collection, array, 0);
            Assert.True(Enumerable.SequenceEqual(collection, array.Take(count)));
        }

        #endregion
    }
}
