using System;
using System.Diagnostics;

namespace DataStructuresCSharpTest.TestHelper
{
    public class TestTraceListener : DefaultTraceListener
    {
        private const string DefaultMessage = "Debug Assertion Failed";
        public override void Fail(string message, string detailMessage)
        {
            if (string.IsNullOrEmpty(message)) message = DefaultMessage;
            if (!string.IsNullOrEmpty(detailMessage)) message = $"{message}:{detailMessage}";
            throw new Exception(message);
        }

        public override void Fail(string message)
        {
            if (string.IsNullOrEmpty(message)) message = DefaultMessage;
            throw new Exception(message);
        }
    }
}
