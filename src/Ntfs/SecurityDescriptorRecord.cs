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

namespace DiscUtils.Ntfs
{
    using System;

    internal sealed class SecurityDescriptorRecord : IByteArraySerializable
    {
        public uint Hash;
        public uint Id;
        public long OffsetInFile;
        public uint EntrySize;
        public byte[] SecurityDescriptor;

        public int Size
        {
            get { return SecurityDescriptor.Length + 0x14; }
        }

        public bool Read(byte[] buffer, int offset)
        {
            Hash = Utilities.ToUInt32LittleEndian(buffer, offset + 0x00);
            Id = Utilities.ToUInt32LittleEndian(buffer, offset + 0x04);
            OffsetInFile = Utilities.ToInt64LittleEndian(buffer, offset + 0x08);
            EntrySize = Utilities.ToUInt32LittleEndian(buffer, offset + 0x10);

            if (EntrySize > 0)
            {
                SecurityDescriptor = new byte[EntrySize - 0x14];
                Array.Copy(buffer, offset + 0x14, SecurityDescriptor, 0, SecurityDescriptor.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Read(buffer, offset);
            return SecurityDescriptor.Length + 0x14;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            EntrySize = (uint)Size;

            Utilities.WriteBytesLittleEndian(Hash, buffer, offset + 0x00);
            Utilities.WriteBytesLittleEndian(Id, buffer, offset + 0x04);
            Utilities.WriteBytesLittleEndian(OffsetInFile, buffer, offset + 0x08);
            Utilities.WriteBytesLittleEndian(EntrySize, buffer, offset + 0x10);

            Array.Copy(SecurityDescriptor, 0, buffer, offset + 0x14, SecurityDescriptor.Length);
        }
    }
}
