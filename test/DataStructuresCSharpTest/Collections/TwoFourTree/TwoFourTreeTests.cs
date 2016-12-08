using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStructuresCSharpTest.Common;
using Xunit;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.TwoFourTree
{
    public abstract class TwoFourTreeTests<TKey, TValue> : ISortedKeyValueCollectionTests<TKey, TValue>
    {
        #region IDictionary<TKey, TValue Helper Methods

        protected override bool EnumeratorCurrentUndefinedOperationThrows => true;

        protected override IDictionary<TKey, TValue> GenericIDictionaryFactory() => new TwoFourTree<TKey, TValue>();

        #endregion

        #region Constructors

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_Constructor_IDictionary(int count)
        {
            var source = GenericIDictionaryFactory(count);
            IDictionary<TKey, TValue> copied = new TwoFourTree<TKey, TValue>(source);
            Assert.Equal(source.Count, copied.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_Constructor_IDictionary_IComparer(int count)
        {
            var comparer = GetKeyIComparer();
            var source = GenericIDictionaryFactory(count);
            var copied = new TwoFourTree<TKey, TValue>(source, comparer);
            Assert.Equal(source, copied);
        }


        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_Constructor_IComparer(int count)
        {
            var comparer = GetKeyIComparer();
            var source = GenericIDictionaryFactory(count);
            var copied = new TwoFourTree<TKey, TValue>(source, comparer);
            Assert.Equal(source, copied);
        }

        #endregion
    }
}
