﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataStructuresCSharpTest.Common
{
    internal static class CollectionAsserts
    {
        public static void Equal(ICollection expected, ICollection actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }
            Assert.Equal(expected.Count, actual.Count);
            var e = expected.GetEnumerator();
            var a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                Assert.True(a.MoveNext(), "actual has fewer elements");
                if (e.Current == null)
                {
                    Assert.Null(a.Current);
                }
                else
                {
                    Assert.IsType(e.Current.GetType(), a.Current);
                    Assert.Equal(e.Current, a.Current);
                }
            }
            Assert.False(a.MoveNext(), "actual has more elements");
        }

        public static void Equal<T>(ICollection<T> expected, ICollection<T> actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }
            Assert.Equal(expected.Count, actual.Count);
            using var e = expected.GetEnumerator();
            using var a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                Assert.True(a.MoveNext(), "actual has fewer elements");
                if (e.Current == null)
                {
                    Assert.Null(a.Current);
                }
                else
                {
                    Assert.IsType(e.Current.GetType(), a.Current);
                    Assert.Equal(e.Current, a.Current);
                }
            }
            Assert.False(a.MoveNext(), "actual has more elements");
        }

        public static void EqualUnordered(ICollection expected, ICollection actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }

            // Lookups are an aggregated collections (enumerable contents), but ordered.
            var e = expected.Cast<object>().ToLookup(key => key);
            var a = actual.Cast<object>().ToLookup(key => key);

            // Dictionaries can't handle null keys, which is a possibility
            Assert.Equal(e.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()), a.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()));

            // Get count of null keys.  Returns an empty sequence (and thus a 0 count) if no null key
            Assert.Equal(e[null].Count(), a[null].Count());
        }

        public static void EqualUnordered<T>(ICollection<T> expected, ICollection<T> actual)
        {
            Assert.Equal(expected == null, actual == null);
            if (expected == null)
            {
                return;
            }

            // Lookups are an aggregated collections (enumerable contents), but ordered.
            var e = expected.Cast<object>().ToLookup(key => key);
            var a = actual.Cast<object>().ToLookup(key => key);

            // Dictionaries can't handle null keys, which is a possibility
            Assert.Equal(e.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()), a.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()));

            // Get count of null keys.  Returns an empty sequence (and thus a 0 count) if no null key
            Assert.Equal(e[null].Count(), a[null].Count());
        }
    }
}
