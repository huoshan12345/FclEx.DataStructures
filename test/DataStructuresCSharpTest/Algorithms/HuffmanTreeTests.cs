using System;
using System.Text;
using DataStructuresCSharp.Algorithms;
using Xunit;

namespace DataStructuresCSharpTest.Algorithms
{
    public class HuffmanTreeTests
    {
        private string CreateString(int stringLength)
        {
            var rand = new Random(stringLength);
            var bytes1 = new byte[stringLength];
            rand.NextBytes(bytes1);
            return Convert.ToBase64String(bytes1);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void HuffmanTree_Encode_Decode(int count)
        {
            var sourceStr = CreateString(count);
            var sourceBytes = Encoding.UTF8.GetBytes(sourceStr);
            var cipherBytes = HuffmanTree.Encode(sourceBytes);
            var plainBytes = HuffmanTree.Decode(cipherBytes);
            var plainStr = Encoding.UTF8.GetString(plainBytes);

            Assert.Equal(sourceStr.Length, plainStr.Length);
            Assert.Equal(sourceStr, plainStr);
            Assert.Equal(sourceBytes.Length, plainBytes.Length);
            Assert.Equal(sourceBytes, plainBytes);
        }
    }
}
