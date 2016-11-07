using System;

namespace DataStructuresCSharpTest.Collections.LinkedStack
{
    public class LinkedStackTestsString : LinkedStackTests<string>
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

    public class LinkedStackTestsInt : LinkedStackTests<int>
    {
        protected override int CreateT(int seed)
        {
            var rand = new Random(seed);
            return rand.Next();
        }
    }
}
