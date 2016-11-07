using System;

namespace DataStructuresCSharpTest.Collections.LinkedQueue
{
    public class LinkedQueueTestsString : LinkedQueueTests<string>
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

    public class LinkedQueueTestsInt : LinkedQueueTests<int>
    {
        protected override int CreateT(int seed)
        {
            var rand = new Random(seed);
            return rand.Next();
        }
    }
}
