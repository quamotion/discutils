//
// Copyright (c) 2008-2011, Kenneth Bell
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

    internal class SuperBlock : IByteArraySerializable
    {
        public const uint XfsMagic = 0x58465342;
        /// <summary>
        /// magic number == XFS_SB_MAGIC
        /// </summary>
        public uint Magic;
        /// <summary>
        /// logical block size, bytes
        /// </summary>
        public uint Blocksize;

        /// <summary>
        /// number of data blocks
        /// </summary>
        public ulong DataBlocks;

        /// <summary>
        /// number of realtime blocks
        /// </summary>
        public ulong RealtimeBlocks;

        /// <summary>
        /// number of realtime extents
        /// </summary>
        public ulong RealtimeExtents;

        /// <summary>
        /// user-visible file system unique id
        /// </summary>
        public Guid UniqueId;

        /// <summary>
        /// starting block of log if internal
        /// </summary>
        public ulong Logstart;

        /// <summary>
        /// root inode number
        /// </summary>
        public ulong RootInode;

        /// <summary>
        /// bitmap inode for realtime extents
        /// </summary>
        public ulong RealtimeBitmapInode;

        /// <summary>
        /// summary inode for rt bitmap
        /// </summary>
        public ulong RealtimeSummaryInode;

        /// <summary>
        /// realtime extent size, blocks
        /// </summary>
        public uint RealtimeExtentSize;

        /// <summary>
        /// size of an allocation group
        /// </summary>
        public uint AgBlocks;

        /// <summary>
        /// number of allocation groups
        /// </summary>
        public uint AgCount;

        /// <summary>
        /// number of rt bitmap blocks
        /// </summary>
        public uint RealtimeBitmapBlocks;

        /// <summary>
        /// number of log blocks
        /// </summary>
        public uint LogBlocks;

        /// <summary>
        /// header version == XFS_SB_VERSION
        /// </summary>
        public ushort Version;

        /// <summary>
        /// volume sector size, bytes
        /// </summary>
        public ushort SectorSize;

        /// <summary>
        /// inode size, bytes
        /// </summary>
        public ushort InodeSize;

        /// <summary>
        /// inodes per block
        /// </summary>
        public ushort InodesPerBlock;

        /// <summary>
        /// file system name
        /// </summary>
        public string FilesystemName;

        /// <summary>
        /// log2 of <see cref="Blocksize"/>
        /// </summary>
        public byte BlocksizeLog2;

        /// <summary>
        /// log2 of <see cref="SectorSize"/>
        /// </summary>
        public byte SectorSizeLog2;

        /// <summary>
        /// log2 of <see cref="InodeSize"/>
        /// </summary>
        public byte InodeSizeLog2;

        /// <summary>
        /// log2 of <see cref="InodesPerBlock"/>
        /// </summary>
        public byte InodesPerBlockLog2;

        /// <summary>
        /// log2 of <see cref="AgBlocks"/> (rounded up)
        /// </summary>
        public byte AgBlocksLog2;

        /// <summary>
        /// log2 of <see cref="RealtimeExtents"/>
        /// </summary>
        public byte RealtimeExtentsLog2;

        /// <summary>
        /// mkfs is in progress, don't mount
        /// </summary>
        public byte InProgress;

        /// <summary>
        /// max % of fs for inode space
        /// </summary>
        public byte InodesMaxPercent;

         /*
         * These fields must remain contiguous.  If you really
         * want to change their layout, make sure you fix the
         * code in xfs_trans_apply_sb_deltas().
         */                           
        #region statistics
        
        /// <summary>
        /// allocated inodes
        /// </summary>
        public ulong AllocatedInodes;

        /// <summary>
        /// free inodes
        /// </summary>
        public ulong FreeInodes;

        /// <summary>
        /// free data blocks
        /// </summary>
        public ulong FreeDataBlocks;

        /// <summary>
        /// free realtime extents
        /// </summary>
        public ulong FreeRealtimeExtents;
        
        #endregion

        /// <summary>
        /// user quota inode
        /// </summary>
        public ulong UserQuotaInode;
        
        /// <summary>
        /// group quota inode
        /// </summary>
        public ulong GroupQuotaInode;
        
        /// <summary>
        /// quota flags
        /// </summary>
        public ushort QuotaFlags;

        /// <summary>
        /// misc. flags
        /// </summary>
        public byte Flags;
        
        /// <summary>
        /// shared version number
        /// </summary>
        public byte SharedVersionNumber;
        
        /// <summary>
        /// inode chunk alignment, fsblocks
        /// </summary>
        public uint InodeChunkAlignment;
        
        /// <summary>
        /// stripe or raid unit
        /// </summary>
        public uint Unit;
        
        /// <summary>
        /// stripe or raid width
        /// </summary>
        public uint Width;

        /// <summary>
        /// log2 of dir block size (fsbs)
        /// </summary>
        public byte DirBlockLog2;
        
        /// <summary>
        /// log2 of the log sector size
        /// </summary>
        public byte LogSectorSizeLog2;
        
        /// <summary>
        /// sector size for the log, bytes
        /// </summary>
        public ushort LogSectorSize;
        
        /// <summary>
        /// stripe unit size for the log
        /// </summary>
        public uint LogUnitSize;
        
        /// <summary>
        /// additional feature bits
        /// </summary>
        public uint Features2;

        /*
        * bad features2 field as a result of failing to pad the sb structure to
        * 64 bits. Some machines will be using this field for features2 bits.
        * Easiest just to mark it bad and not use it for anything else.
        *
        * This is not kept up to date in memory; it is always overwritten by
        * the value in sb_features2 when formatting the incore superblock to
        * the disk buffer.
        */
        /// <summary>
        /// bad features2 field as a result of failing to pad the sb structure to
        /// 64 bits. Some machines will be using this field for features2 bits.
        /// Easiest just to mark it bad and not use it for anything else.
        /// 
        /// This is not kept up to date in memory; it is always overwritten by
        /// the value in sb_features2 when formatting the incore superblock to
        /// the disk buffer.
        /// </summary>
        public uint BadFeatures2;

        /* version 5 superblock fields start here */

        /* feature masks */
        public uint CompatibleFeatures;
        public ReadOnlyCompatibleFeatures ReadOnlyCompatibleFeatures;
        public uint IncompatibleFeatures;
        public uint LogIncompatibleFeatures;

        /// <summary>
        /// superblock crc
        /// </summary>
        public uint Crc;

        /// <summary>
        /// sparse inode chunk alignment
        /// </summary>
        public uint SparseInodeAlignment;

        /// <summary>
        /// project quota inode
        /// </summary>
        public ulong ProjectQuotaInode;

        /// <summary>
        /// last write sequence
        /// </summary>
        public long Lsn;

        /// <summary>
        /// metadata file system unique id
        /// </summary>
        public Guid MetaUuid;

        /* must be padded to 64 bit alignment */

        public int Size
        {
            get { return 264; }
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Magic = Utilities.ToUInt32BigEndian(buffer, offset);
            Blocksize = Utilities.ToUInt32BigEndian(buffer, offset + 0x4);
            DataBlocks = Utilities.ToUInt64BigEndian(buffer, offset + 0x8);
            RealtimeBlocks = Utilities.ToUInt64BigEndian(buffer, offset + 0x10);
            RealtimeExtents = Utilities.ToUInt64BigEndian(buffer, offset + 0x18);
            UniqueId = Utilities.ToGuidBigEndian(buffer, offset + 0x20);
            Logstart = Utilities.ToUInt64BigEndian(buffer, offset + 0x30);
            RootInode = Utilities.ToUInt64BigEndian(buffer, offset + 0x38);
            RealtimeBitmapInode = Utilities.ToUInt64BigEndian(buffer, offset + 0x40);
            RealtimeSummaryInode = Utilities.ToUInt64BigEndian(buffer, offset + 0x48);
            RealtimeExtentSize = Utilities.ToUInt32BigEndian(buffer, offset + 0x50);
            AgBlocks = Utilities.ToUInt32BigEndian(buffer, offset + 0x54);
            AgCount = Utilities.ToUInt32BigEndian(buffer, offset + 0x58);
            RealtimeBitmapBlocks = Utilities.ToUInt32BigEndian(buffer, offset + 0x5C);
            LogBlocks = Utilities.ToUInt32BigEndian(buffer, offset + 0x60);
            Version = Utilities.ToUInt16BigEndian(buffer, offset + 0x64);
            SectorSize = Utilities.ToUInt16BigEndian(buffer, offset + 0x66);
            InodeSize = Utilities.ToUInt16BigEndian(buffer, offset + 0x68);
            InodesPerBlock = Utilities.ToUInt16BigEndian(buffer, offset + 0x6A);
            FilesystemName = Utilities.BytesToZString(buffer, offset + 0x6C, 12);
            BlocksizeLog2 = buffer[offset + 0x78];
            SectorSizeLog2 = buffer[offset + 0x79];
            InodeSizeLog2 = buffer[offset + 0x7A];
            InodesPerBlockLog2 = buffer[offset + 0x7B];
            AgBlocksLog2 = buffer[offset + 0x7C];
            RealtimeExtentsLog2 = buffer[offset + 0x7D];
            InProgress = buffer[offset + 0x7E];
            InodesMaxPercent = buffer[offset + 0x7F];
            AllocatedInodes = Utilities.ToUInt64BigEndian(buffer, offset + 0x80);
            FreeInodes = Utilities.ToUInt64BigEndian(buffer, offset + 0x88);
            FreeDataBlocks = Utilities.ToUInt64BigEndian(buffer, offset + 0x90);
            FreeRealtimeExtents = Utilities.ToUInt64BigEndian(buffer, offset + 0x98);
            UserQuotaInode = Utilities.ToUInt64BigEndian(buffer, offset + 0xA0);
            GroupQuotaInode = Utilities.ToUInt64BigEndian(buffer, offset + 0xA8);
            QuotaFlags = Utilities.ToUInt16BigEndian(buffer, offset + 0xB0);
            Flags = buffer[offset + 0xB2];
            SharedVersionNumber = buffer[offset + 0xB3];
            InodeChunkAlignment = Utilities.ToUInt32BigEndian(buffer, offset + 0xB4);
            Unit = Utilities.ToUInt32BigEndian(buffer, offset + 0xB8);
            Width = Utilities.ToUInt32BigEndian(buffer, offset + 0xBC);
            DirBlockLog2 = buffer[offset + 0xC0];
            LogSectorSizeLog2 = buffer[offset + 0xC1];
            LogSectorSize = Utilities.ToUInt16BigEndian(buffer, offset + 0xC2);
            LogUnitSize = Utilities.ToUInt32BigEndian(buffer, offset + 0xC4);
            Features2 = Utilities.ToUInt32BigEndian(buffer, offset + 0xC8);
            BadFeatures2 = Utilities.ToUInt32BigEndian(buffer, offset + 0xCC);
            CompatibleFeatures = Utilities.ToUInt32BigEndian(buffer, offset + 0xD0);
            ReadOnlyCompatibleFeatures = (ReadOnlyCompatibleFeatures)Utilities.ToUInt32BigEndian(buffer, offset + 0xD4);
            IncompatibleFeatures = Utilities.ToUInt32BigEndian(buffer, offset + 0xD8);
            LogIncompatibleFeatures = Utilities.ToUInt32BigEndian(buffer, offset + 0xDC);
            Crc = Utilities.ToUInt32BigEndian(buffer, offset + 0xE0);
            SparseInodeAlignment = Utilities.ToUInt32BigEndian(buffer, offset + 0xE4);
            ProjectQuotaInode = Utilities.ToUInt64BigEndian(buffer, offset + 0xE8);
            Lsn = Utilities.ToInt64BigEndian(buffer, offset + 0xF0);
            MetaUuid = Utilities.ToGuidBigEndian(buffer, offset + 0xF8);
            return 264;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        public uint xfs_btree_compute_maxlevels()
        {
            var len = AgBlocks;
            uint level;
            var limits = new uint[] {xfs_rmapbt_maxrecs(false), xfs_rmapbt_maxrecs(true)};
            ulong maxblocks = (len + limits[0] - 1) / limits[0];
            for (level = 1; maxblocks > 1; level++)
                maxblocks = (maxblocks + limits[1] - 1) / limits[1];
            return level;
        }

        private uint xfs_rmapbt_maxrecs(bool leaf)
        {
            var blocklen = Blocksize - 56;
            if (leaf)
                return blocklen/24;
            return blocklen/(2*20 + 4);
        }
    }
}
