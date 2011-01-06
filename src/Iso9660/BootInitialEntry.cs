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

namespace DiscUtils.Iso9660
{
    using System;

    internal class BootInitialEntry
    {
        public byte BootIndicator;
        public BootDeviceEmulation BootMediaType;
        public ushort LoadSegment;
        public byte SystemType;
        public ushort SectorCount;
        public uint ImageStart;

        public BootInitialEntry()
        {
        }

        public BootInitialEntry(byte[] buffer, int offset)
        {
            BootIndicator = buffer[offset + 0x00];
            BootMediaType = (BootDeviceEmulation)buffer[offset + 0x01];
            LoadSegment = Utilities.ToUInt16LittleEndian(buffer, offset + 0x02);
            SystemType = buffer[offset + 0x04];
            SectorCount = Utilities.ToUInt16LittleEndian(buffer, offset + 0x06);
            ImageStart = Utilities.ToUInt32LittleEndian(buffer, offset + 0x08);
        }

        internal void WriteTo(byte[] buffer, int offset)
        {
            Array.Clear(buffer, offset, 0x20);
            buffer[offset + 0x00] = BootIndicator;
            buffer[offset + 0x01] = (byte)BootMediaType;
            Utilities.WriteBytesLittleEndian(LoadSegment, buffer, offset + 0x02);
            buffer[offset + 0x04] = SystemType;
            Utilities.WriteBytesLittleEndian(SectorCount, buffer, offset + 0x06);
            Utilities.WriteBytesLittleEndian(ImageStart, buffer, offset + 0x08);
        }
    }
}
