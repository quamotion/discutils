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

    internal class AllocationGroupFreeBlockInfo : IByteArraySerializable
    {
        public const uint AgfMagic = 0x58414746;
        /// <summary>
        /// Specifies the magic number for the AGF sector: "XAGF" (0x58414746).
        /// </summary>
        public uint Magic;
        
        /// <summary>
        /// Set to XFS_AGF_VERSION which is currently 1.
        /// </summary>
        public uint Version;

        /// <summary>
        /// Specifies the AG number for the sector.
        /// </summary>
        public uint SequenceNumber;

        /// <summary>
        /// Specifies the size of the AG in filesystem blocks. For all AGs except the last, this must be equal
        /// to the superblock's <see cref="SuperBlock.AgBlocks"/> value. For the last AG, this could be less than the
        /// <see cref="SuperBlock.AgBlocks"/> value. It is this value that should be used to determine the size of the AG.
        /// </summary>
        public uint Length;

        /// <summary>
        /// Specifies the block number for the root of the two free space B+trees.
        /// </summary>
        public uint[] RootBlockNumbers;
        
        public uint Spare0;

        /// <summary>
        /// Specifies the level or depth of the two free space B+trees. For a fresh AG, this will be one, and
        /// the "roots" will point to a single leaf of level 0.
        /// </summary>
        public uint[] Levels;

        public uint Spare1;

        /// <summary>
        /// Specifies the index of the first "free list" block.
        /// </summary>
        public uint FreeListFirst;

        /// <summary>
        /// Specifies the index of the last "free list" block.
        /// </summary>
        public uint FreeListLast;

        /// <summary>
        /// Specifies the number of blocks in the "free list".
        /// </summary>
        public uint FreeListCount;

        /// <summary>
        /// Specifies the current number of free blocks in the AG.
        /// </summary>
        public uint FreeBlocks;

        /// <summary>
        /// Specifies the number of blocks of longest contiguous free space in the AG.
        /// </summary>
        public uint Longest;

        /// <summary>
        /// Specifies the number of blocks used for the free space B+trees. This is only used if the
        /// XFS_SB_VERSION2_LAZYSBCOUNTBIT bit is set in <see cref="SuperBlock.Features2"/>.
        /// </summary>
        public uint BTreeBlocks;

        public int Size
        {
            get { return 0x40; }
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Magic = Utilities.ToUInt32BigEndian(buffer, offset);
            Version = Utilities.ToUInt32BigEndian(buffer, offset + 0x4);
            SequenceNumber = Utilities.ToUInt32BigEndian(buffer, offset + 0x8);
            Length = Utilities.ToUInt32BigEndian(buffer, offset + 0xC);
            RootBlockNumbers = new uint[2];
            RootBlockNumbers[0] = Utilities.ToUInt32BigEndian(buffer, offset + 0x10);
            RootBlockNumbers[1] = Utilities.ToUInt32BigEndian(buffer, offset + 0x14);
            Spare0 = Utilities.ToUInt32BigEndian(buffer, offset + 0x18);
            Levels = new uint[2];
            Levels[0] = Utilities.ToUInt32BigEndian(buffer, offset + 0x1C);
            Levels[1] = Utilities.ToUInt32BigEndian(buffer, offset + 0x20);
            Spare1 = Utilities.ToUInt32BigEndian(buffer, offset + 0x24);
            FreeListFirst = Utilities.ToUInt32BigEndian(buffer, offset + 0x28);
            FreeListLast = Utilities.ToUInt32BigEndian(buffer, offset + 0x2C);
            FreeListCount = Utilities.ToUInt32BigEndian(buffer, offset + 0x30);
            FreeBlocks = Utilities.ToUInt32BigEndian(buffer, offset + 0x34);
            Longest = Utilities.ToUInt32BigEndian(buffer, offset + 0x38);
            BTreeBlocks = Utilities.ToUInt32BigEndian(buffer, offset + 0x3C);
            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
