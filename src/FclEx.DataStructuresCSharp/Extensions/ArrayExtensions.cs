using System;
using System.Runtime.CompilerServices;

namespace FclEx.Extensions
{
    public static class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this T[] items, T item)
        {
            return Array.IndexOf(items, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] items)
        {
            Array.Clear(items, 0, items.Length);
        }
    }
}
