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

    internal class AllocationGroupInodeBtreeInfo : IByteArraySerializable
    {
        public const uint AgiMagic = 0x58414749;

        /// <summary>
        /// Specifies the magic number for the AGI sector: "XAGI" (0x58414749)
        /// </summary>
        public uint Magic { get; private set; }

        /// <summary>
        /// Set to XFS_AGI_VERSION which is currently 1.
        /// </summary>
        public uint Version { get; private set; }

        /// <summary>
        /// Specifies the AG number for the sector.
        /// </summary>
        public uint SequenceNumber { get; private set; }

        /// <summary>
        /// Specifies the size of the AG in filesystem blocks.
        /// </summary>
        public uint Length { get; private set; }

        /// <summary>
        /// Specifies the number of inodes allocated for the AG.
        /// </summary>
        public uint Count { get; private set; }

        /// <summary>
        /// Specifies the block number in the AG containing the root of the inode B+tree.
        /// </summary>
        public uint Root { get; private set; }

        /// <summary>
        /// Specifies the number of levels in the inode B+tree.
        /// </summary>
        public uint Level { get; private set; }

        /// <summary>
        /// Specifies the number of free inodes in the AG.
        /// </summary>
        public uint FreeCount { get; private set; }

        /// <summary>
        /// Specifies AG relative inode number most recently allocated.
        /// </summary>
        public uint NewInode { get; private set; }

        /// <summary>
        /// Deprecated and not used, it's always set to NULL (-1).
        /// </summary>
        [Obsolete]
        public int DirInode { get; private set; }

        /// <summary>
        /// Hash table of unlinked (deleted) inodes that are still being referenced.
        /// </summary>
        public int[] Unlinked { get; private set; }

        public int Size
        {
            get { return 296; }
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Magic = Utilities.ToUInt32BigEndian(buffer, offset);
            Version = Utilities.ToUInt32BigEndian(buffer, offset + 0x4);
            SequenceNumber = Utilities.ToUInt32BigEndian(buffer, offset + 0x8);
            Length = Utilities.ToUInt32BigEndian(buffer, offset + 0xc);
            Count = Utilities.ToUInt32BigEndian(buffer, offset + 0x10);
            Root = Utilities.ToUInt32BigEndian(buffer, offset + 0x14);
            Level = Utilities.ToUInt32BigEndian(buffer, offset + 0x18);
            FreeCount = Utilities.ToUInt32BigEndian(buffer, offset + 0x1c);
            NewInode = Utilities.ToUInt32BigEndian(buffer, offset + 0x20);
            DirInode = Utilities.ToInt32BigEndian(buffer, offset + 0x24);
            Unlinked = new int[64];
            for (int i = 0; i < Unlinked.Length; i++)
            {
                Unlinked[i] = Utilities.ToInt32BigEndian(buffer, offset + 0x28 + i*0x4);
            }
            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
