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
    using System.Text;

    internal sealed class NtfsTransaction : IDisposable
    {
        [ThreadStatic]
        private static NtfsTransaction s_instance;

        private bool _ownRecord;

        private DateTime _timestamp;

        public NtfsTransaction()
        {
            if (s_instance == null)
            {
                s_instance = this;
                _timestamp = DateTime.UtcNow;
                _ownRecord = true;
            }
        }

        public static NtfsTransaction Current
        {
            get { return s_instance; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public void Dispose()
        {
            if (_ownRecord)
            {
                s_instance = null;
            }
        }
    }
}
