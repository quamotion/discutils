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
    using System.IO;
    using DiscUtils.Vfs;

    internal sealed class VfsXfsFileSystem :VfsReadOnlyFileSystem<DirEntry, File, Directory, Context>,IUnixFileSystem
    {
        public VfsXfsFileSystem(Stream stream, FileSystemParameters parameters)
            :base(new XfsFileSystemOptions(parameters))
        {
            stream.Position = 0;
            byte[] superblockData = Utilities.ReadFully(stream, 264);

            SuperBlock superblock = new SuperBlock();
            superblock.ReadFrom(superblockData, 0);

            if (superblock.Magic != SuperBlock.XfsMagic)
            {
                throw new IOException("Invalid superblock magic - probably not an xfs file system");
            }

            Context = new Context
            {
                RawStream = stream,
                SuperBlock = superblock,
                Options = (XfsFileSystemOptions) Options
            };

            var allocationGroups = new AllocationGroup[superblock.AgCount];
            long offset = 0;
            for (int i = 0; i < allocationGroups.Length; i++)
            {
                var ag = new AllocationGroup(Context, offset);
                allocationGroups[ag.InodeBtreeInfo.SequenceNumber] = ag;
                offset += ag.FreeBlockInfo.Length*superblock.Blocksize;
            }
            Context.AllocationGroups = allocationGroups;

            RootDirectory = new Directory(Context, Context.GetInode(superblock.RootInode));
        }
        
        public override string FriendlyName
        {
            get { return "XFS"; }
        }

        /// <inheritdoc />
        public override string VolumeLabel { get { return Context.SuperBlock.FilesystemName; } }

        /// <inheritdoc />
        protected override File ConvertDirEntryToFile(DirEntry dirEntry)
        {
            if (dirEntry.IsDirectory)
            {
                return dirEntry.CachedDirectory ?? (dirEntry.CachedDirectory = new Directory(Context, dirEntry.Inode));
            }
            else if (dirEntry.IsSymlink)
            {
                return new Symlink(Context, dirEntry.Inode);
            }
            else if (dirEntry.Inode.FileType == UnixFileType.Regular)
            {
                return new File(Context, dirEntry.Inode);
            }
            else
            {
                throw new NotSupportedException(String.Format("Type {0} is not supported in XFS", dirEntry.Inode.FileType));
            }
        }
        
        /// <summary>
        /// Size of the Filesystem in bytes
        /// </summary>
        public override long Size
        {
            get
            {
                var superblock = Context.SuperBlock;
                var lsize = superblock.Logstart != 0 ? superblock.LogBlocks : 0;
                return (long) ((superblock.DataBlocks - lsize) * superblock.Blocksize);
            }
        }

        /// <summary>
        /// Used space of the Filesystem in bytes
        /// </summary>
        public override long UsedSpace
        {
            get { return Size - AvailableSpace; }
        }

        /// <summary>
        /// Available space of the Filesystem in bytes
        /// </summary>
        public override long AvailableSpace
        {
            get
            {
                var superblock = Context.SuperBlock;
                ulong fdblocks = 0;
                foreach (var agf in Context.AllocationGroups)
                {
                    fdblocks += agf.FreeBlockInfo.FreeBlocks;
                }
                ulong alloc_set_aside = 0;

                alloc_set_aside = 4 + (superblock.AgCount * (uint)XFS_ALLOC_AGFL_RESERVE);

                if ((superblock.ReadOnlyCompatibleFeatures & ReadOnlyCompatibleFeatures.RMAPBT) != 0)
                {
                    uint rmapMaxlevels = 9;
                    if ((superblock.ReadOnlyCompatibleFeatures & ReadOnlyCompatibleFeatures.REFLINK) != 0)
                    {
                        rmapMaxlevels = superblock.xfs_btree_compute_maxlevels();
                    }
                    alloc_set_aside += superblock.AgCount * rmapMaxlevels;
                }
                return (long) ((fdblocks - alloc_set_aside) * superblock.Blocksize);
            }
        }

        public UnixFileSystemInfo GetUnixFileInfo(string path)
        {
            throw new NotImplementedException();
        }

    }
}
