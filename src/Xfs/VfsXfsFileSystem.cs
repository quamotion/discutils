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
        private AllocationGroup[] _allocationGroups;

        private static readonly int XFS_ALLOC_AGFL_RESERVE = 4;

        public VfsXfsFileSystem(Stream stream, FileSystemParameters parameters, bool ignoreRecovery = false)
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

            _allocationGroups = new AllocationGroup[superblock.AgCount];
            long offset = 0;
            for (int i = 0; i < _allocationGroups.Length; i++)
            {
                var ag = new AllocationGroup(superblock, stream, offset);
                _allocationGroups[i] = ag;
                offset += ag.FreeBlockInfo.Length*superblock.Blocksize;
            }

            Context = new Context
            {
                RawStream = stream,
                SuperBlock = superblock
            };

            //TODO RootDirectory = new Directory(Context, superblock.RootInode, GetInode(superblock.RootInode));
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
            throw new NotImplementedException();
        }
        
        public UnixFileSystemInfo GetUnixFileInfo(string path)
        {
            throw new NotImplementedException();
        }

    }
}
