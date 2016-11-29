using System;
using DataStructuresCSharp.Util;

namespace FxUtility.Algorithms.Sorts
{
    public static class ExchangeSorts<T> where T : IComparable<T>
    {
        /// <summary>
        /// 冒泡排序（Bubble Sort）
        /// </summary>
        /// <param name="arr"></param>
        public static void BubbleSort(T[] arr)
        {
            for (var i = 0; i + 1 < arr.Length; ++i)
            {
                for (var j = 0; j + i + 1 < arr.Length; ++j)
                {
                    if (arr[j].CompareTo(arr[j + 1]) > 0)
                    {
                        Helper.Swap(ref arr[j], ref arr[j + 1]);
                    }
                }
            }
        }

        /// <summary>
        /// 冒泡排序改进（Modified Bubble Sort）
        /// </summary>
        /// <param name="arr"></param>
        public static void ModifiedBubbleSort(T[] arr)
        {
            var i = arr.Length - 1;  //初始时,最后位置保持不变  
            while (i > 0)
            {
                var pos = 0; //每趟开始时,无记录交换  
                for (var j = 0; j < i; ++j)
                {
                    if (arr[j].CompareTo(arr[j + 1]) > 0)
                    {
                        pos = j; //记录交换的位置   
                        Helper.Swap(ref arr[j], ref arr[j + 1]);
                    }
                }
                i = pos; //为下一趟排序作准备  
            }
        }

        /// <summary>
        /// 鸡尾酒排序（Cocktail Sort)
        /// </summary>
        /// <param name="arr"></param>
        public static void CocktailSort(T[] arr)
        {
            var low = 0;
            var high = arr.Length - 1; //设置变量的初始值  
            while (low < high)
            {
                for (var i = low; i < high; ++i) //正向冒泡,找到最大者  
                {
                    if (arr[i].CompareTo(arr[i + 1]) > 0)
                    {
                        Helper.Swap(ref arr[i], ref arr[i + 1]);
                    }
                }
                --high;                 //修改high值, 前移一位  

                for (var i = high; i > low; --i) //反向冒泡,找到最小者  
                {
                    if (arr[i].CompareTo(arr[i - 1]) < 0)
                    {
                        Helper.Swap(ref arr[i], ref arr[i - 1]);
                    }
                }
                ++low;                  //修改low值,后移一位  
            }
        }

        /// <summary>
        /// 奇偶排序（odd–even sort）
        /// </summary>
        /// <param name="arr"></param>
        public static void OddEvenSort(T[] arr)
        {
            bool bSorted = false;

            while (!bSorted)
            {
                bSorted = true;
                for (var i = 1; i + 1 < arr.Length; i += 2)
                {
                    if (arr[i].CompareTo(arr[i + 1]) > 0)
                    {
                        Helper.Swap(ref arr[i], ref arr[i + 1]);
                        bSorted = false;
                    }
                }
                for (var i = 0; i + 1 < arr.Length; i += 2)
                {
                    if (arr[i].CompareTo(arr[i + 1]) > 0)
                    {
                        Helper.Swap(ref arr[i], ref arr[i + 1]);
                        bSorted = false;
                    }
                }
            }
        }

        /// <summary>
        /// 梳排序(Comb sort)
        /// </summary>
        /// <param name="arr"></param>
        public static void CombSort(T[] arr)
        {
            var gap = arr.Length;
            bool bSwapped = false;
            const double shrinkFactor = 1.247330950103979;
            while (gap > 1 || bSwapped)
            {
                bSwapped = false;
                var i = 0;
                if (gap > 1) gap = (int)(gap / shrinkFactor);
                while ((gap + i) < arr.Length)
                {
                    if (arr[i].CompareTo(arr[i + gap]) > 0)
                    {
                        Helper.Swap(ref arr[i], ref arr[i + gap]);
                        bSwapped = true;
                    }
                    ++i;
                }
            }
        }
    }
}
