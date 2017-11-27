using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FclEx.Collections;
using FclEx.Extensions;

namespace FclEx
{
    public static class ListTests
    {
        public static void Add(IList<int> list, int[] numbers)
        {
            foreach (var number in numbers)
            {
                list.Add(number);
            }
        }

        public static void Index(IList<int> list, int[] newNumbers)
        {
            var i = 0;
            foreach (var num in newNumbers)
            {
                if (i >= list.Count) break;
                list[i++] = num;
            }
        }

        public static void IndexOf(IList<int> list, int[] numbers)
        {
            foreach (var number in numbers)
            {
                list.IndexOf(number);
            }
        }

        public static void Insert(IList<int> list, int[] numbers)
        {
            var index = 0;
            foreach (var number in numbers)
            {
                index %= list.Count;
                list.Insert(index, number);
                index += 2;
            }
        }
    }


    public static class DicTests
    {
        public static void Add(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var number in numbers)
            {
                dic.Add(number);
            }
        }

        public static void AddOrUpdate(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var number in numbers)
            {
                dic[number.Key] = number.Value;
            }
        }

        public static void ContainsKey(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var number in numbers)
            {
                var flag = dic.ContainsKey(number.Key);
            }
        }

        public static void Remove(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var number in numbers)
            {
                dic.Remove(number.Key);
            }
        }

        public static void IndexGet(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var num in numbers)
            {
                var item = dic[num.Key];
            }
        }

        public static void IndexSet(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var num in numbers)
            {
                dic[num.Key]++;
            }
        }

        public static void GetKeys(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var number in numbers)
            {
                var keys = dic.Keys;
            }
        }

        public static void GetValues(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            foreach (var number in numbers)
            {
                var values = dic.Values;
            }
        }

        public static void Enumerate(IDictionary<int, int> dic, KeyValuePair<int, int>[] numbers)
        {
            var times = Math.Min(numbers.Length, dic.Count);
            var enumarater = dic.GetEnumerator();
            var i = 0;
            while (enumarater.MoveNext() && i++ < times)
            {
                var cur = enumarater.Current;
            }
            enumarater.Dispose();
        }
    }


    public class Program
    {
        private static readonly IList<int>[] _lists = { new List<int>(), new SeqList<int>(), new DoublyCircularLinkedList<int>() };

        private static readonly IDictionary<int, int>[] _dics = {
            //new Dictionary<int, int>(),
            //new SortedDictionary<int, int>(),
            new SkipList<int, int>(),
            //new AvlTree<int, int>(),
            //new RedBlackTree<int, int>(),
            //new BTree<int, int>(10),
            //new OptimizedBTree<int, int>(10),
            //new BPlusTree<int, int>(10),
        };

        private static readonly Dictionary<string, Action<IDictionary<int, int>, KeyValuePair<int, int>[]>> Actions =
            new Dictionary<string, Action<IDictionary<int, int>, KeyValuePair<int, int>[]>>()
        {
            { nameof(DicTests.Add), DicTests.Add},
            { nameof(DicTests.ContainsKey), DicTests.ContainsKey},
            { nameof(DicTests.IndexGet), DicTests.IndexGet},
            { nameof(DicTests.IndexSet), DicTests.IndexSet},
            { nameof(DicTests.GetKeys), DicTests.GetKeys},
            { nameof(DicTests.GetValues), DicTests.GetValues},
            { nameof(DicTests.Enumerate), DicTests.Enumerate},
            { nameof(DicTests.Remove), DicTests.Remove},
        };

        private static void TestLists()
        {
            var testNumbers = GenerateNumbers(10000000);
            PrintTitle(_lists);
            TestAction(ListTests.Add, nameof(ListTests.Add), _lists, testNumbers);
            TestAction(ListTests.Index, nameof(ListTests.Index), _lists, testNumbers.Take(20000).ToArray());
            TestAction(ListTests.IndexOf, nameof(ListTests.IndexOf), _lists, testNumbers.Take(10000).ToArray());
            TestAction(ListTests.Insert, nameof(ListTests.Insert), _lists, testNumbers.Take(500).ToArray());
            Console.WriteLine("-------------------------------------------------------------------");
        }

        private static void TestDics()
        {
            var testNumbers = GenerateNumbers(2000000, true).Select(item => new KeyValuePair<int, int>(item, item)).ToArray();
            PrintTitle(_dics);
            TestAction(DicTests.Add, nameof(DicTests.Add), _dics, testNumbers);
            TestAction(DicTests.Enumerate, nameof(DicTests.Enumerate), _dics, testNumbers);
            TestAction(DicTests.ContainsKey, nameof(DicTests.ContainsKey), _dics, testNumbers);
            TestAction(DicTests.IndexGet, nameof(DicTests.IndexGet), _dics, testNumbers);
            TestAction(DicTests.IndexSet, nameof(DicTests.IndexSet), _dics, testNumbers);
            TestAction(DicTests.GetKeys, nameof(DicTests.GetKeys), _dics, testNumbers);
            TestAction(DicTests.GetValues, nameof(DicTests.GetValues), _dics, testNumbers);
            TestAction(DicTests.Remove, nameof(DicTests.Remove), _dics, testNumbers);

            foreach (var dic in _dics)
            {
                DicTests.AddOrUpdate(dic, testNumbers);
            }

            var random = new Random();
            TestAction(DicTests.Remove, "RemoveRandom", _dics, testNumbers.OrderBy(item => random.Next()).ToArray());
            Console.WriteLine("-------------------------------------------------------------------");

            foreach (var dic in _dics)
            {
                dic.Clear();
            }
        }

        public static void Main(string[] args)
        {
            var dic = Enumerable.Range(1, 10).ToDictionary(m => m, m => m);
            var tree = new TwoFourTree<int, int>();

            foreach (var pair in dic)
            {
                tree.Add(pair);
                // PrintTree(tree.ToLayerItems());
            }

            PrintTree(tree.ToLayerItems());


            tree.Remove(4);
            PrintTree(tree.ToLayerItems());

            var random = new Random();

            while (true)
            {
                foreach (var pair in dic.OrderBy(m => random.Next()))
                {
                    if (!tree.Remove(pair.Key))
                        throw new Exception();
                    PrintTree(tree.ToLayerItems());
                }
            }




            TestDics();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void PrintTitle<TCollection>(IEnumerable<TCollection> cols)
        {
            Console.Write("{0,-15}", "Method");
            Console.Write("{0,-15}", "TestTimes");
            foreach (var col in cols)
            {
                var name = col.GetType().Name.Split('`')[0];
                Console.Write("{0,-15}", GetAbbreviate(name));
            }
            Console.WriteLine();
        }

        private static string GetAbbreviate(string item)
        {
            if (item.Length <= 13) return item;
            else return item.Substring(0, 10).PadRight(13, '.');

        }

        private static void TestAction<TCollection, T>(Action<TCollection, T[]> action, string actionName, IEnumerable<TCollection> lists, T[] numbers)
        {
            var sw = new Stopwatch();
            Console.Write("{0,-15}", actionName);
            Console.Write("{0,-15}", numbers.Length);
            foreach (var list in lists)
            {
                sw.Start();
                action(list, numbers);
                sw.Stop();
                Console.Write("{0,-15}", sw.ElapsedMilliseconds);
                sw.Reset();
            }
            Console.WriteLine();
        }

        private static int[] GenerateNumbers(int count, bool ramdom = false)
        {
            var numbers = Enumerable.Range(1, count);
            if (ramdom)
            {
                var random = new Random();
                numbers = numbers.OrderBy(item => random.Next());
            }
            return numbers.ToArray();
        }

        public static void PrintTree<T>(IEnumerable<List<T[]>> layers)
        {
            foreach (var layer in layers)
            {
                PrintTreeLayer(layer);
            }
            Console.WriteLine();
        }

        public static void PrintTreeLayer<T>(List<T[]> layer)
        {
            var strs = layer.Select(m => $"[{string.Join(",", m)}]");
            var str = string.Join(" ", strs);
            Console.WriteLine(str);
        }
    }
}
