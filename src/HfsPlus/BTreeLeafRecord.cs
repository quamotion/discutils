﻿//
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

namespace DiscUtils.HfsPlus
{
    using System;

    internal sealed class BTreeLeafRecord<TKey> : BTreeNodeRecord
        where TKey : BTreeKey, new()
    {
        private int _size;
        private TKey _key;
        private byte[] _data;

        public BTreeLeafRecord(int size)
        {
            _size = size;
        }

        public TKey Key
        {
            get { return _key; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public override int Size
        {
            get { return _size; }
        }

        public override int ReadFrom(byte[] buffer, int offset)
        {
            _key = new TKey();
            int keySize = _key.ReadFrom(buffer, offset);

            if ((keySize & 1) != 0)
            {
                ++keySize;
            }

            _data = new byte[_size - keySize];
            Array.Copy(buffer, offset + keySize, _data, 0, _data.Length);

            return _size;
        }

        public override void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return _key + ":" + _data;
        }
    }
}
