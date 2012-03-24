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

namespace DiscUtils.Iso9660
{
    using System.IO;

    internal sealed class SharingProtocolSystemUseEntry : SystemUseEntry
    {
        public byte SystemAreaSkip;

        public SharingProtocolSystemUseEntry(byte[] data, int offset)
        {
            byte len = data[offset + 2];

            Name = "SP";
            Version = data[offset + 3];

            CheckLengthAndVersion(len, 7, 1);

            if (data[offset + 4] != 0xBE || data[offset + 5] != 0xEF)
            {
                throw new InvalidDataException("Invalid SUSP SP entry - invalid checksum bytes");
            }

            SystemAreaSkip = data[offset + 6];
        }
    }
}
