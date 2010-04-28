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

using System.Security.AccessControl;

namespace DiscUtils
{
    /// <summary>
    /// Provides the base class for all file systems that support Windows semantics.
    /// </summary>
    public interface IWindowsFileSystem : IFileSystem
    {
        /// <summary>
        /// Gets the security descriptor associated with the file or directory.
        /// </summary>
        /// <param name="path">The file or directory to inspect.</param>
        /// <returns>The security descriptor.</returns>
        RawSecurityDescriptor GetSecurity(string path);

        /// <summary>
        /// Sets the security descriptor associated with the file or directory.
        /// </summary>
        /// <param name="path">The file or directory to change.</param>
        /// <param name="securityDescriptor">The new security descriptor.</param>
        void SetSecurity(string path, RawSecurityDescriptor securityDescriptor);

        /// <summary>
        /// Gets the reparse point data associated with a file or directory.
        /// </summary>
        /// <param name="path">The file to query</param>
        /// <returns>The reparse point information</returns>
        ReparsePoint GetReparsePoint(string path);

        /// <summary>
        /// Sets the reparse point data on a file or directory.
        /// </summary>
        /// <param name="path">The file to set the reparse point on.</param>
        /// <param name="reparsePoint">The new reparse point.</param>
        void SetReparsePoint(string path, ReparsePoint reparsePoint);

        /// <summary>
        /// Removes a reparse point from a file or directory, without deleting the file or directory.
        /// </summary>
        /// <param name="path">The path to the file or directory to remove the reparse point from</param>
        void RemoveReparsePoint(string path);

        /// <summary>
        /// Gets the short name for a given path.
        /// </summary>
        /// <param name="path">The path to convert</param>
        /// <returns>The short name</returns>
        /// <remarks>
        /// This method only gets the short name for the final part of the path, to
        /// convert a complete path, call this method repeatedly, once for each path
        /// segment.  If there is no short name for the given path,<c>null</c> is
        /// returned.
        /// </remarks>
        string GetShortName(string path);

        /// <summary>
        /// Sets the short name for a given file or directory.
        /// </summary>
        /// <param name="path">The full path to the file or directory to change.</param>
        /// <param name="shortName">The shortName, which should not include a path.</param>
        void SetShortName(string path, string shortName);
    }
}
