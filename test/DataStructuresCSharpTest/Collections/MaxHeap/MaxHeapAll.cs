using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataStructuresCSharpTest.Collections.MaxHeap
{
    public class MaxHeapTestsString : MaxHeapTests<string>
    {
        protected override string CreateT(int seed)
        {
            var stringLength = seed % 10 + 5;
            var rand = new Random(seed);
            var bytes = new byte[stringLength];
            rand.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    public class MaxHeapTestsInt : MaxHeapTests<int>
    {
        protected override int CreateT(int seed) => new Random(seed).Next();
    }
}
