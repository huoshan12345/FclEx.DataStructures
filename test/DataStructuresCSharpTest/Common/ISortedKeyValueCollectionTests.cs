using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataStructuresCSharpTest.Common
{
    // ReSharper disable once InconsistentNaming
    public abstract class ISortedKeyValueCollectionTests<TKey, TValue> : IKeyValueCollectionTests<TKey, TValue>
    {
        #region Ordering

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void SortedDictionary_Generic_DictionaryIsProperlySortedAccordingToComparer(int setLength)
        {
            var set = GenericIDictionaryFactory(setLength);
            var expected = set.ToList();
            expected.Sort(GetIComparer());
            var expectedIndex = 0;
            foreach (var value in set)
                Assert.Equal(expected[expectedIndex++], value);
        }

        #endregion
    }
}
