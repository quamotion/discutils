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

    internal sealed class GenericFixupRecord : FixupRecordBase
    {
        private int _bytesPerSector;
        private byte[] _content;

        public GenericFixupRecord(int bytesPerSector)
            : base(null, bytesPerSector)
        {
            _bytesPerSector = bytesPerSector;
        }

        public byte[] Content
        {
            get { return _content; }
        }

        protected override void Read(byte[] buffer, int offset)
        {
            _content = new byte[(UpdateSequenceCount - 1) * _bytesPerSector];
            Array.Copy(buffer, offset, _content, 0, _content.Length);
        }

        protected override ushort Write(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        protected override int CalcSize()
        {
            throw new NotImplementedException();
        }
    }
}
