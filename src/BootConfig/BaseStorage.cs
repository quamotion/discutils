//
// Copyright (c) 2008-2010, Kenneth Bell
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

namespace DiscUtils.BootConfig
{
    internal abstract class BaseStorage
    {
        public abstract bool HasValue(Guid obj, int element);
        public abstract string GetString(Guid obj, int element);
        public abstract byte[] GetBinary(Guid obj, int element);
        public abstract void SetString(Guid obj, int element, string value);
        public abstract void SetBinary(Guid obj, int element, byte[] value);
        public abstract string[] GetMultiString(Guid obj, int element);
        public abstract void SetMultiString(Guid obj, int element, string[] values);
        public abstract IEnumerable<Guid> EnumerateObjects();
        public abstract IEnumerable<int> EnumerateElements(Guid obj);
        public abstract int GetObjectType(Guid obj);
        public abstract bool ObjectExists(Guid obj);
        public abstract Guid CreateObject(Guid obj, int type);
        public abstract void CreateElement(Guid obj, int element);
        public abstract void DeleteObject(Guid obj);
        public abstract void DeleteElement(Guid obj, int element);
    }
}
