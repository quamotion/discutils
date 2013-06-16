﻿//
// Copyright (c) 2008-2013, Kenneth Bell
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

namespace DiscUtils.Vhdx
{
    using System;

    /// <summary>
    /// Class representing a VHDX header.
    /// </summary>
    public sealed class HeaderInfo
    {
        private VhdxHeader _header;

        internal HeaderInfo(VhdxHeader header)
        {
            _header = header;
        }

        /// <summary>
        /// The signature of the header.
        /// </summary>
        public string Signature
        {
            get
            {
                byte[] buffer = new byte[4];
                Utilities.WriteBytesLittleEndian(_header.Signature, buffer, 0);
                return Utilities.BytesToString(buffer, 0, 4);
            }
        }

        /// <summary>
        /// The checksum of the header information.
        /// </summary>
        public int Checksum
        {
            get { return (int)_header.Checksum; }
        }

        /// <summary>
        /// The sequence number of the header information.
        /// </summary>
        /// <remarks>
        /// VHDX files contain two copies of the header, both contain a sequence number, the highest
        /// sequence number represents the current header information.
        /// </remarks>
        public long SequenceNumber
        {
            get { return (long)_header.SequenceNumber; }
        }

        /// <summary>
        /// Gets a unique GUID indicating when a VHDX file has been substantively modified.
        /// </summary>
        public Guid FileWriteGuid
        {
            get { return _header.FileWriteGuid; }
        }

        /// <summary>
        /// Gets a unique GUID indicating when the content of a VHDX file has changed.
        /// </summary>
        public Guid DataWriteGuid
        {
            get { return _header.DataWriteGuid; }
        }

        /// <summary>
        /// Gets the GUID indicating which log records are valid.
        /// </summary>
        /// <remarks>
        /// The NULL GUID indicates there are no log records to replay.
        /// </remarks>
        public Guid LogGuid
        {
            get { return _header.LogGuid; }
        }

        /// <summary>
        /// Gets the version of the log information, expected to be Zero.
        /// </summary>
        public int LogVersion
        {
            get { return _header.LogVersion; }
        }

        /// <summary>
        /// VHDX file format version, expected to be One.
        /// </summary>
        public int Version
        {
            get { return _header.Version; }
        }

        /// <summary>
        /// The length of the VHDX log.
        /// </summary>
        public long LogLength
        {
            get { return _header.LogLength; }
        }

        /// <summary>
        /// The offset of the VHDX log within the file.
        /// </summary>
        public long LogOffset
        {
            get { return (long)_header.LogOffset; }
        }
    }
}
