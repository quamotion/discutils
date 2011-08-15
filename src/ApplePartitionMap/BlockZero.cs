﻿//
// Copyright (c) 2008-2011, Kenneth Bell
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

namespace DiscUtils.ApplePartitionMap
{
    internal sealed class BlockZero : IByteArraySerializable
    {
        public ushort Signature;
        public ushort BlockSize;
        public uint BlockCount;
        public ushort DeviceType;
        public ushort DeviceId;
        public uint DriverData;
        public ushort DriverCount;

        public int Size
        {
            get { return 512; }
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Signature = Utilities.ToUInt16BigEndian(buffer, offset + 0);
            BlockSize = Utilities.ToUInt16BigEndian(buffer, offset + 2);
            BlockCount = Utilities.ToUInt32BigEndian(buffer, offset + 4);
            DeviceType = Utilities.ToUInt16BigEndian(buffer, offset + 8);
            DeviceId = Utilities.ToUInt16BigEndian(buffer, offset + 10);
            DriverData = Utilities.ToUInt32BigEndian(buffer, offset + 12);
            DriverCount = Utilities.ToUInt16LittleEndian(buffer, offset + 16);

            return 512;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new System.NotImplementedException();
        }
    }
}
