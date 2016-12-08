using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FclEx.Extensions;
using FclEx.Node;

namespace FclEx.Algorithms
{

    public static class HuffmanTree
    {
        private class HuffmanTreeNode : BinaryNode<int, HuffmanTreeNode>, IComparable<HuffmanTreeNode>
        {
            private readonly int _frequency;
            private readonly int _id;

            public HuffmanTreeNode(int item, int frequency, int id) : base(item)
            {
                _id = id;
                _frequency = frequency;
            }

            public HuffmanTreeNode(HuffmanTreeNode left, HuffmanTreeNode right, int id)
                : this(-1, left._frequency + right._frequency, id)
            {
                LeftChild = left;
                RightChild = right;
                left.Parent = this;
                right.Parent = this;
            }

            public int CompareTo(HuffmanTreeNode other)
            {
                return _frequency == other._frequency ? _id - other._id
                    : _frequency - other._frequency;
            }

            public IEnumerable<HuffmanTreeNode> Traverse()
            {
                var queue = new Queue<HuffmanTreeNode>();
                queue.Enqueue(this);
                while (queue.Count != 0)
                {
                    var node = queue.Dequeue();
                    yield return node;
                    if (node.HasLeftChild) queue.Enqueue(node.LeftChild);
                    if (node.HasRightChild) queue.Enqueue(node.RightChild);
                }
            }

            public HuffmanTreeNode GetRoot()
            {
                var p = this;
                while (p.Parent != null)
                {
                    p = p.Parent;
                }
                return p;
            }
        }

        private class HuffmanTreeHeader
        {
            private readonly byte[] _compressedFreqArray;
            public int HeaderLength => _compressedFreqArray.Length + 4 + 4;
            public int DataLength { get; }
            public int[] FreqArray { get; }

            public byte[] ToBytes()
            {
                var index = 0;
                var bytes = new byte[HeaderLength];
                BitConverter.GetBytes(HeaderLength).CopyTo(bytes, index);
                index += 4;
                BitConverter.GetBytes(DataLength).CopyTo(bytes, index);
                index += 4;
                _compressedFreqArray.CopyTo(bytes, index);
                return bytes;
            }

            private static byte[] CompressFreqArray(int[] freqArray)
            {
                Debug.Assert(MaxLength == freqArray.Length);
                var byteDic = new Dictionary<byte, int>(MaxLength);
                var ushortDic = new Dictionary<byte, int>(MaxLength);
                var intDic = new Dictionary<byte, int>(MaxLength);

                for (var i = 0; i < freqArray.Length; ++i)
                {
                    if (freqArray[i] == 0) continue;
                    else if (freqArray[i] < byte.MaxValue) byteDic.Add((byte)i, freqArray[i]);
                    else if (freqArray[i] < ushort.MaxValue) ushortDic.Add((byte)i, freqArray[i]);
                    else intDic.Add((byte)i, freqArray[i]);
                }
                var bytes = new byte[4 * 3 + byteDic.Count * 2 + ushortDic.Count * 3 + intDic.Count * 5];
                var index = 0;

                BitConverter.GetBytes(byteDic.Count).CopyTo(bytes, index);
                index += 4;
                foreach (var item in byteDic)
                {
                    bytes[index++] = item.Key;
                    bytes[index++] = (byte)item.Value;
                }

                BitConverter.GetBytes(ushortDic.Count).CopyTo(bytes, index);
                index += 4;
                foreach (var item in ushortDic)
                {
                    bytes[index++] = item.Key;
                    BitConverter.GetBytes((ushort)item.Value).CopyTo(bytes, index);
                    index += 2;
                }

                BitConverter.GetBytes(intDic.Count).CopyTo(bytes, index);
                index += 4;
                foreach (var item in intDic)
                {
                    bytes[index++] = item.Key;
                    BitConverter.GetBytes(item.Value).CopyTo(bytes, index);
                    index += 4;
                }
                return bytes.ToArray();
            }

            private static int[] DecompressFreqArray(byte[] datas)
            {
                if (datas == null) throw new ArgumentNullException(nameof(datas));

                var freqArray = new int[MaxLength];
                var index = 0;
                var byteLen = BitConverter.ToInt32(datas, index);
                index += 4;
                for (var i = 0; i < byteLen; ++i)
                {
                    var itemIndex = datas[index++];
                    var freq = datas[index++];
                    freqArray[itemIndex] = freq;
                }

                var ushortLen = BitConverter.ToInt32(datas, index);
                index += 4;
                for (var i = 0; i < ushortLen; ++i)
                {
                    var itemIndex = datas[index++];
                    var freq = BitConverter.ToUInt16(datas, index);
                    index += 2;
                    freqArray[itemIndex] = freq;
                }

                var intLen = BitConverter.ToInt32(datas, index);
                index += 4;
                for (var i = 0; i < intLen; ++i)
                {
                    var itemIndex = datas[index++];
                    var freq = BitConverter.ToInt32(datas, index);
                    index += 4;
                    freqArray[itemIndex] = freq;
                }
                Debug.Assert(index == datas.Length);
                return freqArray;
            }

            public HuffmanTreeHeader(byte[] datas, bool create)
            {
                var tuple = create ? CreateHeader(datas) : GetHeader(datas);
                DataLength = tuple.Item1;
                FreqArray = tuple.Item2;
                _compressedFreqArray = tuple.Item3;
            }

            private static int[] CreateFreqArray(IEnumerable<byte> datas)
            {
                var result = new int[MaxLength];
                foreach (var data in datas)
                {
                    result[data]++;
                }
                return result;
            }

            private static Tuple<int, int[], byte[]> GetHeader(byte[] datas)
            {
                var index = 0;
                var headerLength = BitConverter.ToInt32(datas, index);
                index += 4;
                var dataLength = BitConverter.ToInt32(datas, index);
                index += 4;
                var compressedFreqArray = datas.Skip(index).Take(headerLength - index).ToArray();
                return Tuple.Create(dataLength, DecompressFreqArray(compressedFreqArray), compressedFreqArray);
            }

            private static Tuple<int, int[], byte[]> CreateHeader(byte[] datas)
            {
                var dataLength = datas.Length;
                var freqArray = CreateFreqArray(datas);
                var compressedFreqArray = CompressFreqArray(freqArray);
                return Tuple.Create(dataLength, freqArray, compressedFreqArray);
            }
        }

        private const int MaxLength = byte.MaxValue + 1;

        public static byte[] Encode(byte[] datas)
        {
            var header = new HuffmanTreeHeader(datas, true);
            var leafNodeList = BuildTree(header.FreqArray).Item2;
            var encodingTable = BuildEncodingTable(leafNodeList);
            var encodedSource = new List<bool>(datas.Length * 8 + header.HeaderLength * 8);
            foreach (var data in datas)
            {
                encodedSource.AddRange(encodingTable[data]);
            }
            var encodedSourceBytes = encodedSource.ToBytes();
            encodedSource.Clear();
            var bytes = new byte[header.HeaderLength + encodedSourceBytes.Length];
            header.ToBytes().CopyTo(bytes, 0);
            encodedSourceBytes.CopyTo(bytes, header.HeaderLength);
            return bytes;
        }

        private static byte Decode(BitArray bits, ref int bitIndex, HuffmanTreeNode root)
        {
            var p = root;
            while (!p.IsLeafNode)
            {
                var bit = bits[bitIndex++];
                p = p.Children[bit ? 1 : 0];
            }
            return (byte)p.Item;
        }

        public static byte[] Decode(byte[] datas)
        {
            var header = new HuffmanTreeHeader(datas, false);
            var root = BuildTree(header.FreqArray).Item1;
            var resultBytes = new byte[header.DataLength];
            var bits = new BitArray(datas.Skip(header.HeaderLength).ToArray());

            var bitIndex = 0;
            for (var i = 0; i < header.DataLength; i++)
            {
                resultBytes[i] = Decode(bits, ref bitIndex, root);
            }
            return resultBytes;
        }

        private static Tuple<HuffmanTreeNode, HuffmanTreeNode[]> BuildTree(int[] freqArray)
        {
            var nodeArr = new HuffmanTreeNode[MaxLength];
            for (var i = 0; i < freqArray.Length; i++)
            {
                nodeArr[i] = new HuffmanTreeNode(i, freqArray[i], i);
            }
            var priQueue = new SortedSet<HuffmanTreeNode>(nodeArr);
            Debug.Assert(priQueue.Count == nodeArr.Length);
            var leafNodeArr = new HuffmanTreeNode[MaxLength];

            var leafIndex = 0;
            var id = freqArray.Length;
            while (priQueue.Count >= 2)
            {
                var node1 = priQueue.Min();
                if (!priQueue.Remove(node1)) Debug.Assert(false);
                var node2 = priQueue.Min();
                if (!priQueue.Remove(node2)) Debug.Assert(false);
                if (node1.IsLeafNode) leafNodeArr[leafIndex++] = node1;
                if (node2.IsLeafNode) leafNodeArr[leafIndex++] = node2;
                if (!priQueue.Add(new HuffmanTreeNode(node1, node2, id++))) Debug.Assert(false);
            }
#if DEBUG
            var list = priQueue.Min().Traverse().ToList();
            Debug.Assert(list.Count == 2 * MaxLength - 1);
            Debug.Assert(list.Count(item => item.IsLeafNode) == MaxLength);
#endif
            return Tuple.Create(priQueue.Min(), leafNodeArr);
        }

        private static List<bool>[] BuildEncodingTable(IEnumerable<HuffmanTreeNode> leafNodeList)
        {
            var table = new List<bool>[MaxLength];
            foreach (var leafNode in leafNodeList)
            {
                var code = GetCode(leafNode);
                table[leafNode.Item] = code;
            }
            return table;
        }

        private static List<bool> GetCode(HuffmanTreeNode leafNode)
        {
            Debug.Assert(leafNode.IsLeafNode);
            var p = leafNode;
            var bits = new List<bool>();
            while (p.Parent != null)
            {
                bits.Add(p.Parent.LeftChild != p);
                p = p.Parent;
            }
            bits.Reverse();
            return bits;
        }
    }
}
