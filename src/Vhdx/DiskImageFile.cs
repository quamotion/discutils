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

namespace DiscUtils.Vhdx
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Represents a single .VHDX file.
    /// </summary>
    public sealed class DiskImageFile : VirtualDiskLayer
    {
        /// <summary>
        /// The stream containing the VHDX file.
        /// </summary>
        private Stream _fileStream;

        /// <summary>
        /// Indicates if this object controls the lifetime of the stream.
        /// </summary>
        private Ownership _ownsStream;

        /// <summary>
        /// The object that can be used to locate relative file paths.
        /// </summary>
        private FileLocator _fileLocator;

        /// <summary>
        /// The file name of this VHDX.
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Value of the active VHDX header.
        /// </summary>
        private VhdxHeader _header;

        /// <summary>
        /// Which VHDX header is active.
        /// </summary>
        private int _activeHeader;

        /// <summary>
        /// The set of VHDX regions.
        /// </summary>
        private RegionTable _regionTable;

        /// <summary>
        /// VHDX metadata region content.
        /// </summary>
        private Metadata _metadata;

        /// <summary>
        /// Block Allocation Table for disk content.
        /// </summary>
        private BlockAllocationTable _bat;

        /// <summary>
        /// Initializes a new instance of the DiskImageFile class.
        /// </summary>
        /// <param name="stream">The stream to interpret</param>
        public DiskImageFile(Stream stream)
        {
            _fileStream = stream;

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the DiskImageFile class.
        /// </summary>
        /// <param name="stream">The stream to interpret</param>
        /// <param name="ownsStream">Indicates if the new instance should control the lifetime of the stream.</param>
        public DiskImageFile(Stream stream, Ownership ownsStream)
        {
            _fileStream = stream;
            _ownsStream = ownsStream;

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the DiskImageFile class.
        /// </summary>
        /// <param name="path">The file path to open</param>
        /// <param name="access">Controls how the file can be accessed</param>
        public DiskImageFile(string path, FileAccess access)
            : this(new LocalFileLocator(Path.GetDirectoryName(path)), Path.GetFileName(path), access)
        {
        }

        internal DiskImageFile(FileLocator locator, string path, Stream stream, Ownership ownsStream)
            : this(stream, ownsStream)
        {
            _fileLocator = locator.GetRelativeLocator(locator.GetDirectoryFromPath(path));
            _fileName = locator.GetFileFromPath(path);
        }

        internal DiskImageFile(FileLocator locator, string path, FileAccess access)
        {
            FileShare share = access == FileAccess.Read ? FileShare.Read : FileShare.None;
            _fileStream = locator.Open(path, FileMode.Open, access, share);
            _ownsStream = Ownership.Dispose;

            _fileLocator = locator.GetRelativeLocator(locator.GetDirectoryFromPath(path));
            _fileName = locator.GetFileFromPath(path);

            Initialize();
        }

        /// <summary>
        /// Gets the unique id of the parent disk.
        /// </summary>
        public Guid ParentUniqueId
        {
            get
            {
                if ((_metadata.FileParameters.Flags & FileParametersFlags.HasParent) == 0)
                {
                    return Guid.Empty;
                }

                string parentLinkage;
                if (_metadata.ParentLocator.Entries.TryGetValue("parent_linkage", out parentLinkage))
                {
                    return new Guid(parentLinkage);
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the geometry of the virtual disk.
        /// </summary>
        public override Geometry Geometry
        {
            get { return Geometry.FromCapacity(Capacity); }
        }

        /// <summary>
        /// Gets a value indicating if the layer only stores meaningful sectors.
        /// </summary>
        public override bool IsSparse
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the full path to this disk layer, or empty string.
        /// </summary>
        public override string FullPath
        {
            get
            {
                if (_fileLocator != null && _fileName != null)
                {
                    return _fileLocator.GetFullPath(_fileName);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the file is a differencing disk.
        /// </summary>
        public override bool NeedsParent
        {
            get { return (_metadata.FileParameters.Flags & FileParametersFlags.HasParent) != 0; }
        }

        /// <summary>
        /// Gets the unique id of this file.
        /// </summary>
        public Guid UniqueId
        {
            get { return _header.DataWriteGuid; }
        }

        /// <summary>
        /// Gets the extent that comprises this file.
        /// </summary>
        public override IList<VirtualDiskExtent> Extents
        {
            get
            {
                List<VirtualDiskExtent> result = new List<VirtualDiskExtent>();
                result.Add(new DiskExtent(this));
                return result;
            }
        }

        internal override long Capacity
        {
            get { return (long)_metadata.DiskSize; }
        }

        internal override FileLocator RelativeFileLocator
        {
            get { return _fileLocator; }
        }

        internal long StoredSize
        {
            get { return _fileStream.Length; }
        }

        /// <summary>
        /// Initializes a stream as a fixed-sized VHDX file.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="ownsStream">Indicates if the new instance controls the lifetime of the stream.</param>
        /// <param name="capacity">The desired capacity of the new disk</param>
        /// <returns>An object that accesses the stream as a VHDX file</returns>
        public static DiskImageFile InitializeFixed(Stream stream, Ownership ownsStream, long capacity)
        {
            return InitializeFixed(stream, ownsStream, capacity, null);
        }

        /// <summary>
        /// Initializes a stream as a fixed-sized VHDX file.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="ownsStream">Indicates if the new instance controls the lifetime of the stream.</param>
        /// <param name="capacity">The desired capacity of the new disk</param>
        /// <param name="geometry">The desired geometry of the new disk, or <c>null</c> for default</param>
        /// <returns>An object that accesses the stream as a VHDX file</returns>
        public static DiskImageFile InitializeFixed(Stream stream, Ownership ownsStream, long capacity, Geometry geometry)
        {
            InitializeFixedInternal(stream, capacity, geometry);
            return new DiskImageFile(stream, ownsStream);
        }

        /// <summary>
        /// Initializes a stream as a dynamically-sized VHDX file.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="ownsStream">Indicates if the new instance controls the lifetime of the stream.</param>
        /// <param name="capacity">The desired capacity of the new disk</param>
        /// <returns>An object that accesses the stream as a VHDX file</returns>
        public static DiskImageFile InitializeDynamic(Stream stream, Ownership ownsStream, long capacity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a stream as a dynamically-sized VHDX file.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="ownsStream">Indicates if the new instance controls the lifetime of the stream.</param>
        /// <param name="capacity">The desired capacity of the new disk</param>
        /// <param name="geometry">The desired geometry of the new disk, or <c>null</c> for default</param>
        /// <returns>An object that accesses the stream as a VHDX file</returns>
        public static DiskImageFile InitializeDynamic(Stream stream, Ownership ownsStream, long capacity, Geometry geometry)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a stream as a dynamically-sized VHDX file.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="ownsStream">Indicates if the new instance controls the lifetime of the stream.</param>
        /// <param name="capacity">The desired capacity of the new disk</param>
        /// <param name="blockSize">The size of each block (unit of allocation)</param>
        /// <returns>An object that accesses the stream as a VHDX file</returns>
        public static DiskImageFile InitializeDynamic(Stream stream, Ownership ownsStream, long capacity, long blockSize)
        {
            return InitializeDynamic(stream, ownsStream, capacity, null, blockSize);
        }

        /// <summary>
        /// Initializes a stream as a dynamically-sized VHDX file.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="ownsStream">Indicates if the new instance controls the lifetime of the stream.</param>
        /// <param name="capacity">The desired capacity of the new disk</param>
        /// <param name="geometry">The desired geometry of the new disk, or <c>null</c> for default</param>
        /// <param name="blockSize">The size of each block (unit of allocation)</param>
        /// <returns>An object that accesses the stream as a VHDX file</returns>
        public static DiskImageFile InitializeDynamic(Stream stream, Ownership ownsStream, long capacity, Geometry geometry, long blockSize)
        {
            InitializeDynamicInternal(stream, capacity, geometry, blockSize);

            return new DiskImageFile(stream, ownsStream);
        }

        /// <summary>
        /// Initializes a stream as a differencing disk VHDX file.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="ownsStream">Indicates if the new instance controls the lifetime of the stream.</param>
        /// <param name="parent">The disk this file is a different from.</param>
        /// <param name="parentAbsolutePath">The full path to the parent disk.</param>
        /// <param name="parentRelativePath">The relative path from the new disk to the parent disk.</param>
        /// <param name="parentModificationTimeUtc">The time the parent disk's file was last modified (from file system).</param>
        /// <returns>An object that accesses the stream as a VHDX file</returns>
        public static DiskImageFile InitializeDifferencing(
            Stream stream,
            Ownership ownsStream,
            DiskImageFile parent,
            string parentAbsolutePath,
            string parentRelativePath,
            DateTime parentModificationTimeUtc)
        {
            InitializeDifferencingInternal(stream, parent, parentAbsolutePath, parentRelativePath, parentModificationTimeUtc);

            return new DiskImageFile(stream, ownsStream);
        }

        /// <summary>
        /// Opens an existing region within the VHDX file.
        /// </summary>
        /// <param name="region">Identifier for the region to open.</param>
        /// <returns>A stream containing the region data.</returns>
        /// <remarks>Regions are an extension mechanism in VHDX - with some regions defined by
        /// the VHDX specification to hold metadata and the block allocation data.</remarks>
        public Stream OpenRegion(Guid region)
        {
            RegionEntry metadataRegion = _regionTable.Regions[region];
            return new SubStream(_fileStream, metadataRegion.FileOffset, metadataRegion.Length);
        }

        /// <summary>
        /// Opens the content of the disk image file as a stream.
        /// </summary>
        /// <param name="parent">The parent file's content (if any)</param>
        /// <param name="ownsParent">Whether the created stream assumes ownership of parent stream</param>
        /// <returns>The new content stream</returns>
        public override SparseStream OpenContent(SparseStream parent, Ownership ownsParent)
        {
            return DoOpenContent(parent, ownsParent);
        }

        /// <summary>
        /// Gets the location of the parent file, given a base path.
        /// </summary>
        /// <returns>Array of candidate file locations</returns>
        public override string[] GetParentLocations()
        {
            return GetParentLocations(_fileLocator);
        }

        /// <summary>
        /// Gets the location of the parent file, given a base path.
        /// </summary>
        /// <param name="basePath">The full path to this file</param>
        /// <returns>Array of candidate file locations</returns>
        [Obsolete("Use GetParentLocations() by preference")]
        public string[] GetParentLocations(string basePath)
        {
            return GetParentLocations(new LocalFileLocator(basePath));
        }

        internal static DiskImageFile InitializeFixed(FileLocator locator, string path, long capacity, Geometry geometry)
        {
            DiskImageFile result = null;
            Stream stream = locator.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            try
            {
                InitializeFixedInternal(stream, capacity, geometry);
                result = new DiskImageFile(locator, path, stream, Ownership.Dispose);
                stream = null;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return result;
        }

        internal static DiskImageFile InitializeDynamic(FileLocator locator, string path, long capacity, Geometry geometry, long blockSize)
        {
            DiskImageFile result = null;
            Stream stream = locator.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            try
            {
                InitializeDynamicInternal(stream, capacity, geometry, blockSize);
                result = new DiskImageFile(locator, path, stream, Ownership.Dispose);
                stream = null;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return result;
        }

        internal DiskImageFile CreateDifferencing(FileLocator fileLocator, string path)
        {
            Stream stream = fileLocator.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            string fullPath = _fileLocator.GetFullPath(_fileName);
            string relativePath = fileLocator.MakeRelativePath(_fileLocator, _fileName);
            DateTime lastWriteTime = _fileLocator.GetLastWriteTimeUtc(_fileName);

            InitializeDifferencingInternal(stream, this, fullPath, relativePath, lastWriteTime);

            return new DiskImageFile(fileLocator, path, stream, Ownership.Dispose);
        }

        internal MappedStream DoOpenContent(SparseStream parent, Ownership ownsParent)
        {
            SparseStream theParent = parent;
            Ownership theOwnership = ownsParent;

            if (parent == null)
            {
                theParent = new ZeroStream(Capacity);
                theOwnership = Ownership.Dispose;
            }

            return new ContentStream(SparseStream.FromStream(_fileStream, Ownership.None), _bat, Capacity, theParent, theOwnership);
        }

        /// <summary>
        /// Disposes of underlying resources.
        /// </summary>
        /// <param name="disposing">Set to <c>true</c> if called within Dispose(),
        /// else <c>false</c>.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_ownsStream == Ownership.Dispose)
                    {
                        _fileStream.Dispose();
                        _fileStream = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private static void InitializeFixedInternal(Stream stream, long capacity, Geometry geometry)
        {
            throw new NotImplementedException();
        }

        private static void InitializeDynamicInternal(Stream stream, long capacity, Geometry geometry, long blockSize)
        {
            throw new NotImplementedException();
        }

        private static void InitializeDifferencingInternal(Stream stream, DiskImageFile parent, string parentAbsolutePath, string parentRelativePath, DateTime parentModificationTimeUtc)
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            _fileStream.Position = 0;
            FileHeader fileHeader = Utilities.ReadStruct<FileHeader>(_fileStream);
            if (!fileHeader.IsValid)
            {
                throw new IOException("Invalid VHDX file - file signature mismatch");
            }

            ReadHeaders();

            if (_header.LogGuid != Guid.Empty)
            {
                throw new NotSupportedException("Detected VHDX file with replay log - not yet supported");
            }

            ReadRegionTable();

            ReadMetadata();

            Stream batStream = OpenRegion(RegionTable.BatGuid);
            _bat = new BlockAllocationTable(_fileStream, batStream, _metadata);
        }

        private void ReadMetadata()
        {
            Stream regionStream = OpenRegion(RegionTable.MetadataRegionGuid);
            _metadata = new Metadata(regionStream);
        }

        private void ReadRegionTable()
        {
            _fileStream.Position = 192 * Sizes.OneKiB;
            _regionTable = Utilities.ReadStruct<RegionTable>(_fileStream);
            foreach (var entry in _regionTable.Regions.Values)
            {
                if ((entry.Flags & RegionFlags.Required) != 0)
                {
                    if (entry.Guid != RegionTable.BatGuid && entry.Guid != RegionTable.MetadataRegionGuid)
                    {
                        throw new IOException("Invalid VHDX file - unrecognised required region");
                    }
                }
            }
        }

        private void ReadHeaders()
        {
            _activeHeader = 0;

            _fileStream.Position = 64 * Sizes.OneKiB;
            VhdxHeader vhdxHeader1 = Utilities.ReadStruct<VhdxHeader>(_fileStream);
            if (vhdxHeader1.IsValid)
            {
                _header = vhdxHeader1;
                _activeHeader = 1;
            }

            _fileStream.Position = 128 * Sizes.OneKiB;
            VhdxHeader vhdxHeader2 = Utilities.ReadStruct<VhdxHeader>(_fileStream);
            if (vhdxHeader2.IsValid && (_activeHeader == 0 || _header.SequenceNumber < vhdxHeader2.SequenceNumber))
            {
                _header = vhdxHeader2;
                _activeHeader = 2;
            }

            if (_activeHeader == 0)
            {
                throw new IOException("Invalid VHDX file - no valid VHDX headers found");
            }
        }

        /// <summary>
        /// Gets the locations of the parent file.
        /// </summary>
        /// <param name="fileLocator">The file locator to use</param>
        /// <returns>Array of candidate file locations</returns>
        private string[] GetParentLocations(FileLocator fileLocator)
        {
            if (!NeedsParent)
            {
                throw new InvalidOperationException("Only differencing disks contain parent locations");
            }

            if (fileLocator == null)
            {
                // Use working directory by default
                fileLocator = new LocalFileLocator(string.Empty);
            }

            List<string> paths = new List<string>();

            ParentLocator locator = _metadata.ParentLocator;
            string value;

            if (locator.Entries.TryGetValue("relative_path", out value))
            {
                paths.Add(fileLocator.ResolveRelativePath(value));
            }

            if (locator.Entries.TryGetValue("volume_path", out value))
            {
                paths.Add(value);
            }

            if (locator.Entries.TryGetValue("absolute_win32_path", out value))
            {
                paths.Add(value);
            }

            return paths.ToArray();
        }
    }
}
