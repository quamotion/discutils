﻿//
// Copyright (c) 2008-2009, Kenneth Bell
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscUtils.Ntfs.Attributes;

namespace DiscUtils.Ntfs
{
    internal class ClusterBitmap : File
    {
        private Bitmap _bitmap;

        public ClusterBitmap(NtfsFileSystem fileSystem, FileRecord fileRecord)
            : base(fileSystem, fileRecord)
        {
            _bitmap = new Bitmap(new BitmapAttribute(fileSystem, fileRecord.GetAttribute(AttributeType.Data)));
        }

        public Tuple<long, long>[] AllocateClusters(long count)
        {
            List<Tuple<long, long>> result = new List<Tuple<long, long>>();

            long numFound = 0;

            long numClusters = _fileSystem.RawStream.Length / _fileSystem.BytesPerCluster;
            long focusCluster = numClusters / 8;

            while (numFound < count)
            {
                if (!_bitmap.IsPresent(focusCluster))
                {
                    // Start of a run...
                    long runStart = focusCluster;
                    _bitmap.MarkPresent(focusCluster);
                    ++focusCluster;

                    while (!_bitmap.IsPresent(focusCluster) && focusCluster - runStart < count)
                    {
                        _bitmap.MarkPresent(focusCluster);
                        ++focusCluster;
                    }

                    result.Add(new Tuple<long, long>(runStart, focusCluster - runStart));
                    numFound += (focusCluster - runStart);
                }

                ++focusCluster;
            }

            return result.ToArray();
        }
    }
}
