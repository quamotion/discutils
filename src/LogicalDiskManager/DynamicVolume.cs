//
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

namespace DiscUtils.LogicalDiskManager
{
    using System;
    using System.IO;

    internal class DynamicVolume
    {
        private DynamicDiskGroup _group;
        private Guid _volumeId;

        internal DynamicVolume(DynamicDiskGroup group, Guid volumeId)
        {
            _group = group;
            _volumeId = volumeId;
        }

        public byte BiosType
        {
            get { return Record.BiosType; }
        }

        public Guid Identity
        {
            get { return _volumeId; }
        }

        public long Length
        {
            get { return Record.Size * Sizes.Sector; }
        }

        public LogicalVolumeStatus Status
        {
            get { return _group.GetVolumeStatus(Record.Id); }
        }

        private VolumeRecord Record
        {
            get { return _group.GetVolume(_volumeId); }
        }

        public SparseStream Open()
        {
            if (Status == LogicalVolumeStatus.Failed)
            {
                throw new IOException("Attempt to open 'failed' volume");
            }
            else
            {
                return _group.OpenVolume(Record.Id);
            }
        }
    }
}
