using System;
using System.Collections.Generic;
using DataStructuresCSharpTest.Common;

namespace DataStructuresCSharpTest.Collections.BinarySearchTree
{

    public class BinarySearchTreeTestsStrinString : BinarySearchTreeTests<string, string>
    {
        protected override KeyValuePair<string, string> CreateT(int seed)
        {
            return new KeyValuePair<string, string>(CreateTKey(seed), CreateTKey(seed + 500));
        }

        protected override string CreateTKey(int seed)
        {
            var stringLength = seed % 10 + 5;
            var rand = new Random(seed);
            var bytes1 = new byte[stringLength];
            rand.NextBytes(bytes1);
            return Convert.ToBase64String(bytes1);
        }

        protected override string CreateTValue(int seed)
        {
            return CreateTKey(seed);
        }
    }

    public class BinarySearchTreeTestsIntInt : BinarySearchTreeTests<int, int>
    {
        protected override bool DefaultValueAllowed => true;

        protected override KeyValuePair<int, int> CreateT(int seed)
        {
            var rand = new Random(seed);
            return new KeyValuePair<int, int>(rand.Next(), rand.Next());
        }

        protected override int CreateTKey(int seed)
        {
            var rand = new Random(seed);
            return rand.Next();
        }

        protected override int CreateTValue(int seed)
        {
            return CreateTKey(seed);
        }
    }

    public class BinarySearchTreeTestsSimpleIntIntWithComparerWrapStructuralSimpleInt : BinarySearchTreeTests<SimpleInt, int>
    {
        protected override bool DefaultValueAllowed => true;

        public override IEqualityComparer<SimpleInt> GetKeyIEqualityComparer()
        {
            return new WrapStructuralSimpleInt();
        }

        public override IComparer<SimpleInt> GetKeyIComparer()
        {
            return new WrapStructuralSimpleInt();
        }

        protected override SimpleInt CreateTKey(int seed)
        {
            var rand = new Random(seed);
            return new SimpleInt(rand.Next());
        }

        protected override int CreateTValue(int seed)
        {
            var rand = new Random(seed);
            return rand.Next();
        }

        protected override KeyValuePair<SimpleInt, int> CreateT(int seed)
        {
            return new KeyValuePair<SimpleInt, int>(CreateTKey(seed), CreateTValue(seed));
        }
    }
}
