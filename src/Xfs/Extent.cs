//
// Copyright (c) 2016, Bianco Veigel
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

namespace DiscUtils.Xfs
{
    using System;
    using System.IO;

    internal class Extent : IByteArraySerializable
    {
        /// <summary>
        /// Number of Blocks
        /// </summary>
        public uint BlockCount { get; private set; }

        public ulong StartBlock { get; private set; }

        public ulong StartOffset { get; private set; }

        public ExtentFlag Flag { get; private set; }

        public int Size
        {
            get { return 16; }
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            ulong lower = Utilities.ToUInt64BigEndian(buffer, offset);
            ulong middle = Utilities.ToUInt64BigEndian(buffer, offset + 0x2);
            ulong upper = Utilities.ToUInt64BigEndian(buffer, offset + 0x8);
            BlockCount = (uint)(lower & 0x001FFFFF);
            StartBlock = (middle >> 5) & 0x000FFFFFFFFFFFFF;
            StartOffset = (upper >> 9) & 0x003FFFFFFFFFFFFF;
            Flag = (ExtentFlag) ((buffer[offset + 0x0] >> 6) & 0x3);
            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
