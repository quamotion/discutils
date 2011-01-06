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

namespace DiscUtils.Udf
{
    using System;
    using System.IO;

    internal enum TagIdentifier : ushort
    {
        None = 0,
        PrimaryVolumeDescriptor = 1,
        AnchorVolumeDescriptorPointer = 2,
        VolumeDescriptorPointer = 3,
        ImplementationUseVolumeDescriptor = 4,
        PartitionDescriptor = 5,
        LogicalVolumeDescriptor = 6,
        UnallocatedSpaceDescriptor = 7,
        TerminatingDescriptor = 8,
        LogicalVolumeIntegrityDescriptor = 9,

        FileSetDescriptor = 256,
        FileIdentifierDescriptor = 257,
        AllocationExtentDescriptor = 258,
        IndirectEntry = 259,
        TerminalEntry = 260,
        FileEntry = 261,
        ExtendedAttributeHeaderDescriptor = 262,
        UnallocatedSpaceEntry = 263,
        SpaceBitmapDescriptor = 264,
        PartitionIntegrityEntry = 265,
        ExtendedFileEntry = 266,
    }

    internal class DescriptorTag : IByteArraySerializable
    {
        public TagIdentifier TagIdentifier;
        public ushort DescriptorVersion;
        public byte TagChecksum;
        public ushort TagSerialNumber;
        public ushort DescriptorCrc;
        public ushort DescriptorCrcLength;
        public uint TagLocation;

        public int Size
        {
            get { return 16; }
        }

        public static bool IsValid(byte[] buffer, int offset)
        {
            byte checkSum = 0;

            if (Utilities.ToUInt16LittleEndian(buffer, offset) == 0)
            {
                return false;
            }

            for (int i = 0; i < 4; ++i)
            {
                checkSum += buffer[offset + i];
            }

            for (int i = 5; i < 16; ++i)
            {
                checkSum += buffer[offset + i];
            }

            return checkSum == buffer[offset + 4];
        }

        public static bool TryFromStream(Stream stream, out DescriptorTag result)
        {
            byte[] next = Utilities.ReadFully(stream, 512);
            if (!DescriptorTag.IsValid(next, 0))
            {
                result = null;
                return false;
            }

            DescriptorTag dt = new DescriptorTag();
            dt.ReadFrom(next, 0);

            result = dt;
            return true;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            TagIdentifier = (TagIdentifier)Utilities.ToUInt16LittleEndian(buffer, offset);
            DescriptorVersion = Utilities.ToUInt16LittleEndian(buffer, offset + 2);
            TagChecksum = buffer[offset + 4];
            TagSerialNumber = Utilities.ToUInt16LittleEndian(buffer, offset + 6);
            DescriptorCrc = Utilities.ToUInt16LittleEndian(buffer, offset + 8);
            DescriptorCrcLength = Utilities.ToUInt16LittleEndian(buffer, offset + 10);
            TagLocation = Utilities.ToUInt32LittleEndian(buffer, offset + 12);

            return 16;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
