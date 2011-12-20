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

namespace DiscUtils.Net.Dns
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents a DNS A record.
    /// </summary>
    public sealed class IP4AddressRecord : ResourceRecord
    {
        private IPAddress _address;

        internal IP4AddressRecord(string name, RecordType type, RecordClass rClass, DateTime expiry, PacketReader reader)
            : base(name, type, rClass, expiry)
        {
            ushort dataLen = reader.ReadUShort();
            int pos = reader.Position;

            _address = new IPAddress(reader.ReadBytes(dataLen));

            reader.Position = pos + dataLen;
        }

        /// <summary>
        /// Gets the IPv4 address.
        /// </summary>
        public IPAddress Address
        {
            get { return _address; }
        }
    }
}
