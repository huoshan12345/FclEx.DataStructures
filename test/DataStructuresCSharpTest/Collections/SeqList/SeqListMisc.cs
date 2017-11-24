using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.SeqList
{
    public class SeqListMisc
    {
        internal class Driver<T>
        {
            #region Insert

            public void BasicInsert(T[] items, T item, int index, int repeat)
            {
                var list = new SeqList<T>(items);

                for (var i = 0; i < repeat; i++)
                {
                    list.Insert(index, item);
                }

                Assert.True(list.Contains(item)); //"Expect it to contain the item."
                Assert.Equal(list.Count, items.Length + repeat); //"Expect to be the same."


                for (var i = 0; i < index; i++)
                {
                    Assert.Equal(list[i], items[i]); //"Expect to be the same."
                }

                for (var i = index; i < index + repeat; i++)
                {
                    Assert.Equal(list[i], item); //"Expect to be the same."
                }


                for (var i = index + repeat; i < list.Count; i++)
                {
                    Assert.Equal(list[i], items[i - repeat]); //"Expect to be the same."
                }
            }

            public void InsertValidations(T[] items)
            {
                var list = new SeqList<T>(items);
                var bad = new int[] { items.Length + 1, items.Length + 2, int.MaxValue, -1, -2, int.MinValue };
                for (var i = 0; i < bad.Length; i++)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(bad[i], items[0])); //"ArgumentOutOfRangeException expected."
                }
            }

            #endregion

            #region InsertRange

            public void InsertRangeICollection(T[] itemsX, T[] itemsY, int index, int repeat, Func<T[], IEnumerable<T>> constructIEnumerable)
            {
                var list = new SeqList<T>(constructIEnumerable(itemsX));

                for (var i = 0; i < repeat; i++)
                {
                    list.InsertRange(index, constructIEnumerable(itemsY));
                }

                foreach (var item in itemsY)
                {
                    Assert.True(list.Contains(item)); //"Should contain the item."
                }
                Assert.Equal(list.Count, itemsX.Length + (itemsY.Length * repeat)); //"Should have the same result."

                for (var i = 0; i < index; i++)
                {
                    Assert.Equal(list[i], itemsX[i]); //"Should have the same result."
                }

                for (var i = index; i < index + (itemsY.Length * repeat); i++)
                {
                    Assert.Equal(list[i], itemsY[(i - index) % itemsY.Length]); //"Should have the same result."
                }

                for (var i = index + (itemsY.Length * repeat); i < list.Count; i++)
                {
                    Assert.Equal(list[i], itemsX[i - (itemsY.Length * repeat)]); //"Should have the same result."
                }

                //InsertRange into itself
                list = new SeqList<T>(constructIEnumerable(itemsX));
                list.InsertRange(index, list);

                foreach (var item in itemsX)
                {
                    Assert.True(list.Contains(item)); //"Should contain the item."
                }
                Assert.Equal(list.Count, itemsX.Length + (itemsX.Length)); //"Should have the same result."

                for (var i = 0; i < index; i++)
                {
                    Assert.Equal(list[i], itemsX[i]); //"Should have the same result."
                }

                for (var i = index; i < index + (itemsX.Length); i++)
                {
                    Assert.Equal(list[i], itemsX[(i - index) % itemsX.Length]); //"Should have the same result."
                }

                for (var i = index + (itemsX.Length); i < list.Count; i++)
                {
                    Assert.Equal(list[i], itemsX[i - (itemsX.Length)]); //"Should have the same result."
                }
            }

            public void InsertRangeList(T[] itemsX, T[] itemsY, int index, int repeat, Func<T[], IEnumerable<T>> constructIEnumerable)
            {
                var list = new SeqList<T>(constructIEnumerable(itemsX));

                for (var i = 0; i < repeat; i++)
                {
                    list.InsertRange(index, new SeqList<T>(constructIEnumerable(itemsY)));
                }

                foreach (var item in itemsY)
                {
                    Assert.True(list.Contains(item)); //"Should contain the item."
                }
                Assert.Equal(list.Count, itemsX.Length + (itemsY.Length * repeat)); //"Should have the same result."

                for (var i = 0; i < index; i++)
                {
                    Assert.Equal(list[i], itemsX[i]); //"Should have the same result."
                }

                for (var i = index; i < index + (itemsY.Length * repeat); i++)
                {
                    Assert.Equal(list[i], itemsY[(i - index) % itemsY.Length]); //"Should have the same result."
                }

                for (var i = index + (itemsY.Length * repeat); i < list.Count; i++)
                {
                    Assert.Equal(list[i], itemsX[i - (itemsY.Length * repeat)]); //"Should have the same result."
                }
            }

            public void InsertRangeValidations(T[] items, Func<T[], IEnumerable<T>> constructIEnumerable)
            {
                var list = new SeqList<T>(constructIEnumerable(items));
                var bad = new int[] { items.Length + 1, items.Length + 2, int.MaxValue, -1, -2, int.MinValue };
                for (var i = 0; i < bad.Length; i++)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(bad[i], constructIEnumerable(items))); //"ArgumentOutOfRangeException expected"
                }

                Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, null)); //"ArgumentNullException expected."
            }

            public IEnumerable<T> ConstructTestCollection(T[] items)
            {
                return items;
            }

            #endregion

            #region Contains

            public void BasicContains(T[] items)
            {
                var list = new SeqList<T>(items);

                for (var i = 0; i < items.Length; i++)
                {
                    Assert.True(list.Contains(items[i])); //"Should contain item."
                }
            }

            public void NonExistingValues(T[] itemsX, T[] itemsY)
            {
                var list = new SeqList<T>(itemsX);

                for (var i = 0; i < itemsY.Length; i++)
                {
                    Assert.False(list.Contains(itemsY[i])); //"Should not contain item"
                }
            }

            public void RemovedValues(T[] items)
            {
                var list = new SeqList<T>(items);
                for (var i = 0; i < items.Length; i++)
                {
                    list.Remove(items[i]);
                    Assert.False(list.Contains(items[i])); //"Should not contain item"
                }
            }

            public void AddRemoveValues(T[] items)
            {
                var list = new SeqList<T>(items);
                for (var i = 0; i < items.Length; i++)
                {
                    list.Add(items[i]);
                    list.Remove(items[i]);
                    list.Add(items[i]);
                    Assert.True(list.Contains(items[i])); //"Should contain item."
                }
            }

            public void MultipleValues(T[] items, int times)
            {
                var list = new SeqList<T>(items);

                for (var i = 0; i < times; i++)
                {
                    list.Add(items[items.Length / 2]);
                }

                for (var i = 0; i < times + 1; i++)
                {
                    Assert.True(list.Contains(items[items.Length / 2])); //"Should contain item."
                    list.Remove(items[items.Length / 2]);
                }
                Assert.False(list.Contains(items[items.Length / 2])); //"Should not contain item"
            }

            public void ContainsNullWhenReference(T[] items, T value)
            {
                if ((object)value != null)
                {
                    throw new ArgumentException("invalid argument passed to testcase");
                }

                var list = new SeqList<T>(items);
                list.Add(value);
                Assert.True(list.Contains(value)); //"Should contain item."
            }
            
            #endregion

            #region Clear

            public void ClearEmptyList()
            {
                var list = new SeqList<T>();
                Assert.Empty(list); //"Should be equal to 0"
                list.Clear();
                Assert.Empty(list); //"Should be equal to 0."
            }

            public void ClearMultipleTimesEmptyList(int times)
            {
                var list = new SeqList<T>();
                Assert.Empty(list); //"Should be equal to 0."
                for (var i = 0; i < times; i++)
                {
                    list.Clear();
                    Assert.Empty(list); //"Should be equal to 0."
                }
            }

            public void ClearNonEmptyList(T[] items)
            {
                var list = new SeqList<T>(items);
                list.Clear();
                Assert.Empty(list); //"Should be equal to 0."
            }

            public void ClearMultipleTimesNonEmptyList(T[] items, int times)
            {
                var list = new SeqList<T>(items);
                for (var i = 0; i < times; i++)
                {
                    list.Clear();
                    Assert.Empty(list); //"Should be equal to 0."
                }
            }
            
            #endregion

            #region ToArray

            public void BasicToArray(T[] items)
            {
                var list = new SeqList<T>(items);

                var arr = list.ToArray();

                for (var i = 0; i < items.Length; i++)
                {
                    Assert.Equal(((Object)arr[i]), items[i]); //"Should be equal."
                }
            }

            public void EnsureNotUnderlyingToArray(T[] items, T item)
            {
                var list = new SeqList<T>(items);
                var arr = list.ToArray();
                list[0] = item;
                if (((Object)arr[0]) == null)
                    Assert.NotNull(list[0]); //"Should NOT be null"
                else
                    Assert.NotEqual(((Object)arr[0]), list[0]); //"Should NOT be equal."
            }

            #endregion
        }

        [Fact]
        public static void InsertTests()
        {
            var intDriver = new Driver<int>();
            var intArr1 = new int[100];
            for (var i = 0; i < 100; i++)
                intArr1[i] = i;

            var intArr2 = new int[100];
            for (var i = 0; i < 100; i++)
                intArr2[i] = i + 100;

            intDriver.BasicInsert(new int[0], 1, 0, 3);
            intDriver.BasicInsert(intArr1, 101, 50, 4);
            intDriver.BasicInsert(intArr1, 100, 100, 5);
            intDriver.BasicInsert(intArr1, 100, 99, 6);
            intDriver.BasicInsert(intArr1, 50, 0, 7);
            intDriver.BasicInsert(intArr1, 50, 1, 8);
            intDriver.BasicInsert(intArr1, 100, 50, 50);

            var stringDriver = new Driver<string>();
            var stringArr1 = new string[100];
            for (var i = 0; i < 100; i++)
                stringArr1[i] = "SomeTestString" + i.ToString();
            var stringArr2 = new string[100];
            for (var i = 0; i < 100; i++)
                stringArr2[i] = "SomeTestString" + (i + 100).ToString();

            stringDriver.BasicInsert(stringArr1, "strobia", 99, 2);
            stringDriver.BasicInsert(stringArr1, "strobia", 100, 3);
            stringDriver.BasicInsert(stringArr1, "strobia", 0, 4);
            stringDriver.BasicInsert(stringArr1, "strobia", 1, 5);
            stringDriver.BasicInsert(stringArr1, "strobia", 50, 51);
            stringDriver.BasicInsert(stringArr1, "strobia", 0, 100);
            stringDriver.BasicInsert(new string[] { null, null, null, "strobia", null }, null, 2, 3);
            stringDriver.BasicInsert(new string[] { null, null, null, null, null }, "strobia", 0, 5);
            stringDriver.BasicInsert(new string[] { null, null, null, null, null }, "strobia", 5, 1);
        }

        [Fact]
        public static void InsertTests_negative()
        {
            var intDriver = new Driver<int>();
            var intArr1 = new int[100];
            for (var i = 0; i < 100; i++)
                intArr1[i] = i;
            intDriver.InsertValidations(intArr1);

            var stringDriver = new Driver<string>();
            var stringArr1 = new string[100];
            for (var i = 0; i < 100; i++)
                stringArr1[i] = "SomeTestString" + i.ToString();
            stringDriver.InsertValidations(stringArr1);
        }

        [Fact]
        public static void InsertRangeTests()
        {
            var intDriver = new Driver<int>();
            var intArr1 = new int[100];
            for (var i = 0; i < 100; i++)
                intArr1[i] = i;

            var intArr2 = new int[10];
            for (var i = 0; i < 10; i++)
            {
                intArr2[i] = i + 100;
            }

            intDriver.InsertRangeICollection(new int[0], intArr1, 0, 1, intDriver.ConstructTestCollection);
            intDriver.InsertRangeICollection(intArr1, intArr2, 0, 1, intDriver.ConstructTestCollection);
            intDriver.InsertRangeICollection(intArr1, intArr2, 1, 1, intDriver.ConstructTestCollection);
            intDriver.InsertRangeICollection(intArr1, intArr2, 99, 1, intDriver.ConstructTestCollection);
            intDriver.InsertRangeICollection(intArr1, intArr2, 100, 1, intDriver.ConstructTestCollection);
            intDriver.InsertRangeICollection(intArr1, intArr2, 50, 50, intDriver.ConstructTestCollection);
            intDriver.InsertRangeList(intArr1, intArr2, 0, 1, intDriver.ConstructTestCollection);

            var stringDriver = new Driver<string>();
            var stringArr1 = new string[100];
            for (var i = 0; i < 100; i++)
                stringArr1[i] = "SomeTestString" + i.ToString();
            var stringArr2 = new string[10];
            for (var i = 0; i < 10; i++)
                stringArr2[i] = "SomeTestString" + (i + 100).ToString();

            stringDriver.InsertRangeICollection(new string[0], stringArr1, 0, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(stringArr1, stringArr2, 0, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(stringArr1, stringArr2, 1, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(stringArr1, stringArr2, 99, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(stringArr1, stringArr2, 100, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(stringArr1, stringArr2, 50, 50, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(new string[] { null, null, null, null }, stringArr2, 0, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(new string[] { null, null, null, null }, stringArr2, 4, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(new string[] { null, null, null, null }, new string[] { null, null, null, null }, 0, 1, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeICollection(new string[] { null, null, null, null }, new string[] { null, null, null, null }, 4, 50, stringDriver.ConstructTestCollection);
            stringDriver.InsertRangeList(stringArr1, stringArr2, 0, 1, stringDriver.ConstructTestCollection);
        }

        [Fact]
        public static void InsertRangeTests_Negative()
        {
            var intDriver = new Driver<int>();
            var intArr1 = new int[100];
            for (var i = 0; i < 100; i++)
                intArr1[i] = i;
            var stringDriver = new Driver<string>();
            var stringArr1 = new string[100];
            for (var i = 0; i < 100; i++)
                stringArr1[i] = "SomeTestString" + i.ToString();

            intDriver.InsertRangeValidations(intArr1, intDriver.ConstructTestCollection);
            stringDriver.InsertRangeValidations(stringArr1, stringDriver.ConstructTestCollection);
        }

        [Fact]
        public static void ContainsTests()
        {
            var intDriver = new Driver<int>();
            var intArr1 = new int[10];
            for (var i = 0; i < 10; i++)
            {
                intArr1[i] = i;
            }

            var intArr2 = new int[10];
            for (var i = 0; i < 10; i++)
            {
                intArr2[i] = i + 10;
            }

            intDriver.BasicContains(intArr1);
            intDriver.NonExistingValues(intArr1, intArr2);
            intDriver.RemovedValues(intArr1);
            intDriver.AddRemoveValues(intArr1);
            intDriver.MultipleValues(intArr1, 3);
            intDriver.MultipleValues(intArr1, 5);
            intDriver.MultipleValues(intArr1, 17);

            var stringDriver = new Driver<string>();
            var stringArr1 = new string[10];
            for (var i = 0; i < 10; i++)
            {
                stringArr1[i] = "SomeTestString" + i.ToString();
            }
            var stringArr2 = new string[10];
            for (var i = 0; i < 10; i++)
            {
                stringArr2[i] = "SomeTestString" + (i + 10).ToString();
            }

            stringDriver.BasicContains(stringArr1);
            stringDriver.NonExistingValues(stringArr1, stringArr2);
            stringDriver.RemovedValues(stringArr1);
            stringDriver.AddRemoveValues(stringArr1);
            stringDriver.MultipleValues(stringArr1, 3);
            stringDriver.MultipleValues(stringArr1, 5);
            stringDriver.MultipleValues(stringArr1, 17);
            stringDriver.ContainsNullWhenReference(stringArr1, null);
        }

        [Fact]
        public static void ClearTests()
        {
            var intDriver = new Driver<int>();
            var intArr = new int[10];
            for (var i = 0; i < 10; i++)
            {
                intArr[i] = i;
            }

            intDriver.ClearEmptyList();
            intDriver.ClearMultipleTimesEmptyList(1);
            intDriver.ClearMultipleTimesEmptyList(10);
            intDriver.ClearMultipleTimesEmptyList(100);
            intDriver.ClearNonEmptyList(intArr);
            intDriver.ClearMultipleTimesNonEmptyList(intArr, 2);
            intDriver.ClearMultipleTimesNonEmptyList(intArr, 7);
            intDriver.ClearMultipleTimesNonEmptyList(intArr, 31);

            var stringDriver = new Driver<string>();
            var stringArr = new string[10];
            for (var i = 0; i < 10; i++)
            {
                stringArr[i] = "SomeTestString" + i.ToString();
            }

            stringDriver.ClearEmptyList();
            stringDriver.ClearMultipleTimesEmptyList(1);
            stringDriver.ClearMultipleTimesEmptyList(10);
            stringDriver.ClearMultipleTimesEmptyList(100);
            stringDriver.ClearNonEmptyList(stringArr);
            stringDriver.ClearMultipleTimesNonEmptyList(stringArr, 2);
            stringDriver.ClearMultipleTimesNonEmptyList(stringArr, 7);
            stringDriver.ClearMultipleTimesNonEmptyList(stringArr, 31);
        }
    }
}
