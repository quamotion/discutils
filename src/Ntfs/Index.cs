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

namespace DiscUtils.Ntfs
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class Index : IDisposable
    {
        protected File _file;
        protected string _name;
        protected BiosParameterBlock _bpb;
        private bool _isFileIndex;

        private IComparer<byte[]> _comparer;

        private IndexRoot _root;
        private IndexNode _rootNode;
        private Stream _indexStream;
        private Bitmap _indexBitmap;

        private ObjectCache<long, IndexBlock> _blockCache;

        public Index(File file, string name, BiosParameterBlock bpb, UpperCase upCase)
        {
            _file = file;
            _name = name;
            _bpb = bpb;
            _isFileIndex = name == "$I30";

            _blockCache = new ObjectCache<long, IndexBlock>();

            _root = _file.GetStream(AttributeType.IndexRoot, _name).GetContent<IndexRoot>();
            _comparer = _root.GetCollator(upCase);

            using (Stream s = _file.OpenStream(AttributeType.IndexRoot, _name, FileAccess.Read))
            {
                byte[] buffer = Utilities.ReadFully(s, (int)s.Length);
                _rootNode = new IndexNode(WriteRootNodeToDisk, 0, this, true, buffer, IndexRoot.HeaderOffset);

                // Give the attribute some room to breathe, so long as it doesn't squeeze others out
                // BROKEN, BROKEN, BROKEN - how to figure this out?  Query at the point of adding entries to the root node?
                _rootNode.TotalSpaceAvailable += _file.MftRecordFreeSpace(AttributeType.IndexRoot, _name) - 100;
            }

            if (_file.StreamExists(AttributeType.IndexAllocation, _name))
            {
                _indexStream = _file.OpenStream(AttributeType.IndexAllocation, _name, FileAccess.ReadWrite);
            }

            if (_file.StreamExists(AttributeType.Bitmap, _name))
            {
                _indexBitmap = new Bitmap(_file.OpenStream(AttributeType.Bitmap, _name, FileAccess.ReadWrite), long.MaxValue);
            }
        }

        private Index(AttributeType attrType, AttributeCollationRule collationRule, File file, string name, BiosParameterBlock bpb, UpperCase upCase)
        {
            _file = file;
            _name = name;
            _bpb = bpb;
            _isFileIndex = name == "$I30";

            _blockCache = new ObjectCache<long, IndexBlock>();

            _file.CreateStream(AttributeType.IndexRoot, _name);

            _root = new IndexRoot()
            {
                AttributeType = (uint)attrType,
                CollationRule = collationRule,
                IndexAllocationSize = (uint)bpb.IndexBufferSize,
                RawClustersPerIndexRecord = bpb.RawIndexBufferSize
            };

            _comparer = _root.GetCollator(upCase);

            _rootNode = new IndexNode(WriteRootNodeToDisk, 0, this, true, 32);
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> Entries
        {
            get
            {
                foreach (var entry in Enumerate(_rootNode))
                {
                    yield return new KeyValuePair<byte[], byte[]>(entry.KeyBuffer, entry.DataBuffer);
                }
            }
        }

        public int Count
        {
            get
            {
                int i = 0;
                foreach (var entry in Entries)
                {
                    ++i;
                }

                return i;
            }
        }

        internal Stream AllocationStream
        {
            get { return _indexStream; }
        }

        internal uint IndexBufferSize
        {
            get { return _root.IndexAllocationSize; }
        }

        internal bool IsFileIndex
        {
            get { return _isFileIndex; }
        }

        public byte[] this[byte[] key]
        {
            get
            {
                byte[] value;
                if (TryGetValue(key, out value))
                {
                    return value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }

            set
            {
                IndexEntry oldEntry;
                IndexNode node;
                _rootNode.TotalSpaceAvailable = _rootNode.CalcSize() + _file.MftRecordFreeSpace(AttributeType.IndexRoot, _name);
                if (_rootNode.TryFindEntry(key, out oldEntry, out node))
                {
                    node.UpdateEntry(key, value);
                }
                else
                {
                    _rootNode.AddEntry(key, value);
                }
            }
        }

        public void Dispose()
        {
            if (_indexBitmap != null)
            {
                _indexBitmap.Dispose();
                _indexBitmap = null;
            }
        }

        public static void Create(AttributeType attrType, AttributeCollationRule collationRule, File file, string name)
        {
            Index idx = new Index(attrType, collationRule, file, name, file.Context.BiosParameterBlock, file.Context.UpperCase);

            idx.WriteRootNodeToDisk();
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> FindAll(IComparable<byte[]> query)
        {
            foreach (var entry in FindAllIn(query, _rootNode))
            {
                yield return new KeyValuePair<byte[], byte[]>(entry.KeyBuffer, entry.DataBuffer);
            }
        }

        public bool ContainsKey(byte[] key)
        {
            byte[] value;
            return TryGetValue(key, out value);
        }

        public bool Remove(byte[] key)
        {
            _rootNode.TotalSpaceAvailable = _rootNode.CalcSize() + _file.MftRecordFreeSpace(AttributeType.IndexRoot, _name);

            IndexEntry overflowEntry;
            bool found = _rootNode.RemoveEntry(key, out overflowEntry);
            if (overflowEntry != null)
            {
                throw new IOException("Error removing entry, root overflowed");
            }

            return found;
        }

        public bool TryGetValue(byte[] key, out byte[] value)
        {
            IndexEntry entry;
            IndexNode node;

            if (_rootNode.TryFindEntry(key, out entry, out node))
            {
                value = entry.DataBuffer;
                return true;
            }

            value = default(byte[]);
            return false;
        }

        internal static string EntryAsString(IndexEntry entry, string fileName, string indexName)
        {
            IByteArraySerializable keyValue = null;
            IByteArraySerializable dataValue = null;

            // Try to guess the type of data in the key and data fields from the filename and index name
            if (indexName == "$I30")
            {
                keyValue = new FileNameRecord();
                dataValue = new FileRecordReference();
            }
            else if (fileName == "$ObjId" && indexName == "$O")
            {
                keyValue = new ObjectIds.IndexKey();
                dataValue = new ObjectIdRecord();
            }
            else if (fileName == "$Reparse" && indexName == "$R")
            {
                keyValue = new ReparsePoints.Key();
                dataValue = new ReparsePoints.Data();
            }
            else if (fileName == "$Quota")
            {
                if (indexName == "$O")
                {
                    keyValue = new Quotas.OwnerKey();
                    dataValue = new Quotas.OwnerRecord();
                }
                else if (indexName == "$Q")
                {
                    keyValue = new Quotas.OwnerRecord();
                    dataValue = new Quotas.QuotaRecord();
                }
            }
            else if (fileName == "$Secure")
            {
                if (indexName == "$SII")
                {
                    keyValue = new SecurityDescriptors.IdIndexKey();
                    dataValue = new SecurityDescriptors.IdIndexData();
                }
                else if (indexName == "$SDH")
                {
                    keyValue = new SecurityDescriptors.HashIndexKey();
                    dataValue = new SecurityDescriptors.IdIndexData();
                }
            }

            try
            {
                if (keyValue != null && dataValue != null)
                {
                    keyValue.ReadFrom(entry.KeyBuffer, 0);
                    dataValue.ReadFrom(entry.DataBuffer, 0);
                    return "{" + keyValue + "-->" + dataValue + "}";
                }
            }
            catch
            {
                return "{Parsing-Error}";
            }

            return "{Unknown-Index-Type}";
        }

        internal bool ShrinkRoot()
        {
            if (_rootNode.Depose())
            {
                WriteRootNodeToDisk();
                _rootNode.TotalSpaceAvailable = _rootNode.CalcSize() + _file.MftRecordFreeSpace(AttributeType.IndexRoot, _name);
                return true;
            }

            return false;
        }

        internal IndexBlock GetSubBlock(IndexEntry parentEntry)
        {
            IndexBlock block = _blockCache[parentEntry.ChildrenVirtualCluster];
            if (block == null)
            {
                block = new IndexBlock(this, false, parentEntry, _bpb);
                _blockCache[parentEntry.ChildrenVirtualCluster] = block;
            }

            return block;
        }

        internal IndexBlock AllocateBlock(IndexEntry parentEntry)
        {
            if (_indexStream == null)
            {
                _file.CreateStream(AttributeType.IndexAllocation, _name);
                _indexStream = _file.OpenStream(AttributeType.IndexAllocation, _name, FileAccess.ReadWrite);
            }

            if (_indexBitmap == null)
            {
                _file.CreateStream(AttributeType.Bitmap, _name);
                _indexBitmap = new Bitmap(_file.OpenStream(AttributeType.Bitmap, _name, FileAccess.ReadWrite), long.MaxValue);
            }

            long idx = _indexBitmap.AllocateFirstAvailable(0);

            parentEntry.ChildrenVirtualCluster = idx * Utilities.Ceil(_bpb.IndexBufferSize, _bpb.SectorsPerCluster * _bpb.BytesPerSector);
            parentEntry.Flags |= IndexEntryFlags.Node;

            IndexBlock block = IndexBlock.Initialize(this, false, parentEntry, _bpb);
            _blockCache[parentEntry.ChildrenVirtualCluster] = block;
            return block;
        }

        internal void FreeBlock(long vcn)
        {
            long idx = vcn / Utilities.Ceil(_bpb.IndexBufferSize, _bpb.SectorsPerCluster * _bpb.BytesPerSector);
            _indexBitmap.MarkAbsent(idx);
            _blockCache.Remove(vcn);
        }

        internal int Compare(byte[] x, byte[] y)
        {
            return _comparer.Compare(x, y);
        }

        internal void Dump(TextWriter writer, string prefix)
        {
            NodeAsString(writer, prefix, _rootNode, "R");
        }

        protected IEnumerable<IndexEntry> Enumerate(IndexNode node)
        {
            foreach (var focus in node.Entries)
            {
                if ((focus.Flags & IndexEntryFlags.Node) != 0)
                {
                    IndexBlock block = GetSubBlock(focus);
                    foreach (var subEntry in Enumerate(block.Node))
                    {
                        yield return subEntry;
                    }
                }

                if ((focus.Flags & IndexEntryFlags.End) == 0)
                {
                    yield return focus;
                }
            }
        }

        private IEnumerable<IndexEntry> FindAllIn(IComparable<byte[]> query, IndexNode node)
        {
            foreach (var focus in node.Entries)
            {
                bool searchChildren = true;
                bool matches = false;
                bool keepIterating = true;

                if ((focus.Flags & IndexEntryFlags.End) == 0)
                {
                    int compVal = query.CompareTo(focus.KeyBuffer);
                    if (compVal == 0)
                    {
                        matches = true;
                    }
                    else if (compVal > 0)
                    {
                        searchChildren = false;
                    }
                    else if (compVal < 0)
                    {
                        keepIterating = false;
                    }
                }

                if (searchChildren && (focus.Flags & IndexEntryFlags.Node) != 0)
                {
                    IndexBlock block = GetSubBlock(focus);
                    foreach (var entry in FindAllIn(query, block.Node))
                    {
                        yield return entry;
                    }
                }

                if (matches)
                {
                    yield return focus;
                }

                if (!keepIterating)
                {
                    yield break;
                }
            }
        }

        private void WriteRootNodeToDisk()
        {
            _rootNode.Header.AllocatedSizeOfEntries = (uint)_rootNode.CalcSize();
            byte[] buffer = new byte[_rootNode.Header.AllocatedSizeOfEntries + _root.Size];
            _root.WriteTo(buffer, 0);
            _rootNode.WriteTo(buffer, _root.Size);
            using (Stream s = _file.OpenStream(AttributeType.IndexRoot, _name, FileAccess.Write))
            {
                s.Position = 0;
                s.Write(buffer, 0, buffer.Length);
                s.SetLength(s.Position);
            }
        }

        private void NodeAsString(TextWriter writer, string prefix, IndexNode node, string id)
        {
            writer.WriteLine(prefix + id + ":");
            foreach (var entry in node.Entries)
            {
                if ((entry.Flags & IndexEntryFlags.End) != 0)
                {
                    writer.WriteLine(prefix + "      E");
                }
                else
                {
                    writer.WriteLine(prefix + "      " + EntryAsString(entry, _file.BestName, _name));
                }
                
                if ((entry.Flags & IndexEntryFlags.Node) != 0)
                {
                    NodeAsString(writer, prefix + "        ", GetSubBlock(entry).Node, ":i" + entry.ChildrenVirtualCluster);
                }
            }
        }
    }
}
