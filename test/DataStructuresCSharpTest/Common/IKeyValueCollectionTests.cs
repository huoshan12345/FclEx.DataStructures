using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Collections;
using Xunit;

namespace DataStructuresCSharpTest.Common
{
    // ReSharper disable once InconsistentNaming
    public abstract class IKeyValueCollectionTests<TKey, TValue> : IDictionaryGenericTests<TKey, TValue>
    {
        #region ContainsValue

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_ContainsValue_NotPresent(int count)
        {
            var dictionary = (IKeyValueCollection<TKey, TValue>)GenericIDictionaryFactory(count);
            var seed = 4315;
            var notPresent = CreateTValue(seed++);
            while (dictionary.Values.Contains(notPresent))
                notPresent = CreateTValue(seed++);
            Assert.False(dictionary.ContainsValue(notPresent));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_ContainsValue_Present(int count)
        {
            var dictionary = (IKeyValueCollection<TKey, TValue>)GenericIDictionaryFactory(count);
            var seed = 4315;
            var notPresent = CreateT(seed++);
            while (dictionary.Contains(notPresent))
                notPresent = CreateT(seed++);
            dictionary.Add(notPresent.Key, notPresent.Value);
            Assert.True(dictionary.ContainsValue(notPresent.Value));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_ContainsValue_DefaultValueNotPresent(int count)
        {
            var dictionary = (IKeyValueCollection<TKey, TValue>)GenericIDictionaryFactory(count);
            Assert.False(dictionary.ContainsValue(default(TValue)));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Generic_ContainsValue_DefaultValuePresent(int count)
        {
            var dictionary = (IKeyValueCollection<TKey, TValue>)GenericIDictionaryFactory(count);
            var seed = 4315;
            var notPresent = CreateTKey(seed++);
            while (dictionary.ContainsKey(notPresent))
                notPresent = CreateTKey(seed++);
            dictionary.Add(notPresent, default(TValue));
            Assert.True(dictionary.ContainsValue(default(TValue)));
        }
        #endregion
    }
}
