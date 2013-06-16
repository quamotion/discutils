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
    using System.Collections.Generic;

    /// <summary>
    /// Class representing an individual metadata item.
    /// </summary>
    public sealed class MetadataInfo
    {
        private MetadataEntry _entry;

        internal MetadataInfo(MetadataEntry entry)
        {
            _entry = entry;
        }

        /// <summary>
        /// Gets the unique identifier for the metadata.
        /// </summary>
        public Guid ItemId
        {
            get { return _entry.ItemId; }
        }

        /// <summary>
        /// Gets the descriptive name for well-known metadata.
        /// </summary>
        public string WellKnownName
        {
            get
            {
                if (_entry.ItemId == MetadataTable.FileParametersGuid)
                {
                    return "File Parameters";
                }
                else if (_entry.ItemId == MetadataTable.LogicalSectorSizeGuid)
                {
                    return "Logical Sector Size";
                }
                else if (_entry.ItemId == MetadataTable.Page83DataGuid)
                {
                    return "SCSI Page 83 Data";
                }
                else if (_entry.ItemId == MetadataTable.ParentLocatorGuid)
                {
                    return "Parent Locator";
                }
                else if (_entry.ItemId == MetadataTable.PhysicalSectorSizeGuid)
                {
                    return "Physical Sector Size";
                }
                else if (_entry.ItemId == MetadataTable.VirtualDiskSizeGuid)
                {
                    return "Virtual Disk Size";
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Offset within the metadata region of the metadata.
        /// </summary>
        public long Offset
        {
            get { return _entry.Offset; }
        }

        /// <summary>
        /// Length of the metadata.
        /// </summary>
        public long Length
        {
            get { return _entry.Length; }
        }

        /// <summary>
        /// Indicates whether this is system or user metadata.
        /// </summary>
        public bool IsUser
        {
            get { return (_entry.Flags & MetadataEntryFlags.IsUser) != 0;}
        }

        /// <summary>
        /// Indicates whether parsing this metadata is needed to open the VHDX file.
        /// </summary>
        public bool IsRequired
        {
            get { return (_entry.Flags & MetadataEntryFlags.IsRequired) != 0;}
        }

        /// <summary>
        /// Indicates whether this is virtual disk metadata, or VHDX file metadata.
        /// </summary>
        public bool IsVirtualDisk
        {
            get { return (_entry.Flags & MetadataEntryFlags.IsVirtualDisk) != 0;}
        }
    }
}
