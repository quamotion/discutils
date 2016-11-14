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
    using System.IO;

    internal class AllocationGroup
    {
        public const uint IbtMagic = 0x49414254;
        public long Offset { get; private set; }

        public AllocationGroupFreeBlockInfo FreeBlockInfo { get; private set; }

        public AllocationGroupInodeBtreeInfo InodeBtreeInfo { get; private set; }

        public AllocationGroup(SuperBlock superblock, Stream data, long offset)
        {
            Offset = offset;
            FreeBlockInfo = new AllocationGroupFreeBlockInfo();
            data.Position = offset + superblock.SectorSize;
            var agfData = Utilities.ReadFully(data, FreeBlockInfo.Size); 
            FreeBlockInfo.ReadFrom(agfData, 0);
            if (FreeBlockInfo.Magic != AllocationGroupFreeBlockInfo.AgfMagic)
            {
                throw new IOException("Invalid AGF magic - probably not an xfs file system");
            }

            InodeBtreeInfo = new AllocationGroupInodeBtreeInfo();
            data.Position = offset + superblock.SectorSize*2;
            var agiData = Utilities.ReadFully(data, InodeBtreeInfo.Size);
            InodeBtreeInfo.ReadFrom(agiData, 0);
            if (InodeBtreeInfo.Magic != AllocationGroupInodeBtreeInfo.AgiMagic)
            {
                throw new IOException("Invalid AGI magic - probably not an xfs file system");
            }
            InodeBtreeInfo.LoadBtree(data, superblock, offset);
            if (InodeBtreeInfo.RootInodeBtree.Magic != IbtMagic)
            {
                throw new IOException("Invalid IBT magic - probably not an xfs file system");
            }
        }
    }
}
