using System;
using System.Collections.Generic;
using DataStructuresCSharpTest.Common;

namespace DataStructuresCSharpTest.Collections.BPlusTree
{

    public class BPlusTreeTestsStrinString : BPlusTreeTests<string, string>
    {
        protected override KeyValuePair<string, string> CreateT(int seed) => new KeyValuePair<string, string>(CreateTKey(seed), CreateTKey(seed + 500));

        protected override string CreateTKey(int seed)
        {
            var stringLength = seed % 10 + 5;
            var bytes1 = new byte[stringLength];
            new Random(seed).NextBytes(bytes1);
            return Convert.ToBase64String(bytes1);
        }

        protected override string CreateTValue(int seed) => CreateTKey(seed);
    }

    public class BPlusTreeTestsIntInt : BPlusTreeTests<int, int>
    {
        protected override bool DefaultValueAllowed => true;

        protected override KeyValuePair<int, int> CreateT(int seed)
        {
            var rand = new Random(seed);
            return new KeyValuePair<int, int>(rand.Next(), rand.Next());
        }

        protected override int CreateTKey(int seed) => new Random(seed).Next();

        protected override int CreateTValue(int seed) => CreateTKey(seed);
    }

    public class BPlusTreeTestsSimpleIntIntWithComparerWrapStructuralSimpleInt : BPlusTreeTests<SimpleInt, int>
    {
        protected override bool DefaultValueAllowed => true;

        public override IEqualityComparer<SimpleInt> GetKeyIEqualityComparer() => new WrapStructuralSimpleInt();

        public override IComparer<SimpleInt> GetKeyIComparer() => new WrapStructuralSimpleInt();

        protected override SimpleInt CreateTKey(int seed) => new SimpleInt(new Random(seed).Next());

        protected override int CreateTValue(int seed) => new Random(seed).Next();

        protected override KeyValuePair<SimpleInt, int> CreateT(int seed) => new KeyValuePair<SimpleInt, int>(CreateTKey(seed), CreateTValue(seed));
    }
}
