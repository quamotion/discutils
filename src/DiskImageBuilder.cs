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


namespace DiscUtils
{
    /// <summary>
    /// Base class for all disk image builders.
    /// </summary>
    public abstract class DiskImageBuilder
    {
        private SparseStream _content;
        private Geometry _geometry;

        /// <summary>
        /// Sets the content for this disk, implying the size of the disk.
        /// </summary>
        public SparseStream Content
        {
            get { return _content; }
            set { _content = value; }
        }

        /// <summary>
        /// Sets the geometry of this disk, will be implied from the content stream if not set.
        /// </summary>
        public Geometry Geometry
        {
            get { return _geometry; }
            set { _geometry = value; }
        }

        /// <summary>
        /// Initiates the construction of the disk image.
        /// </summary>
        /// <param name="baseName">The base name for the disk images.</param>
        /// <returns>A set of one or more logical files that constitute the
        /// disk image.  The first file is the 'primary' file that is normally attached to VMs.</returns>
        /// <remarks>The supplied <c>baseName</c> is the start of the file name, with no file
        /// extension.  The set of file specifications will indicate the actual name corresponding
        /// to each logical file that comprises the disk image.  For example, given a base name
        /// 'foo', the files 'foo.vmdk' and 'foo-flat.vmdk' could be returned.</remarks>
        public abstract DiskImageFileSpecification[] Build(string baseName);
    }
}
