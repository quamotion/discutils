//
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

namespace DiscUtils.SquashFs
{
    using System;

    internal class RegularInode : Inode
    {
        public uint StartBlock;
        public uint FragmentKey;
        public uint FragmentOffset;
        private uint _fileSize;

        public override long FileSize
        {
            get
            {
                return _fileSize;
            }

            set
            {
                if (value > uint.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value", value, "File size greater than " + uint.MaxValue);
                }

                _fileSize = (uint)value;
            }
        }

        public override int Size
        {
            get { return 32; }
        }

        public override int ReadFrom(byte[] buffer, int offset)
        {
            base.ReadFrom(buffer, offset);

            NumLinks = 1;
            StartBlock = Utilities.ToUInt32LittleEndian(buffer, offset + 16);
            FragmentKey = Utilities.ToUInt32LittleEndian(buffer, offset + 20);
            FragmentOffset = Utilities.ToUInt32LittleEndian(buffer, offset + 24);
            _fileSize = Utilities.ToUInt32LittleEndian(buffer, offset + 28);

            return 32;
        }

        public override void WriteTo(byte[] buffer, int offset)
        {
            base.WriteTo(buffer, offset);

            Utilities.WriteBytesLittleEndian(StartBlock, buffer, offset + 16);
            Utilities.WriteBytesLittleEndian(FragmentKey, buffer, offset + 20);
            Utilities.WriteBytesLittleEndian(FragmentOffset, buffer, offset + 24);
            Utilities.WriteBytesLittleEndian(_fileSize, buffer, offset + 28);
        }
    }
}
