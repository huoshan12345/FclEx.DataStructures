using System;
using System.Collections.Generic;
using DataStructuresCSharpTest.Common;
using Xunit;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.AvlTree
{
    public class AvlTreeTestsValues : ICollectionGenericTests<string>
    {
        protected override bool DefaultValueAllowed => true;
        protected override bool DuplicateValuesAllowed => true;
        protected override bool IsReadOnly => true;
        protected override bool EnumeratorCurrentUndefinedOperationThrows => true;
        protected override IEnumerable<ModifyEnumerable> ModifyEnumerables => new List<ModifyEnumerable>();

        protected override ICollection<string> GenericICollectionFactory()
        {
            return new AvlTree<string, string>().Values;
        }

        protected override ICollection<string> GenericICollectionFactory(int count)
        {
            var list = new AvlTree<string, string>();
            var seed = 13453;
            for (var i = 0; i < count; i++)
                list.Add(CreateT(seed++), CreateT(seed++));
            return list.Values;
        }

        protected override string CreateT(int seed)
        {
            var stringLength = seed % 10 + 5;
            var rand = new Random(seed);
            var bytes = new byte[stringLength];
            rand.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void Dictionary_Generic_ValueCollection_GetEnumerator(int count)
        {
            var dictionary = new AvlTree<string, string>();
            var seed = 13453;
            while (dictionary.Count < count)
                dictionary.Add(CreateT(seed++), CreateT(seed++));
            dictionary.Values.GetEnumerator();
        }
    }
}
