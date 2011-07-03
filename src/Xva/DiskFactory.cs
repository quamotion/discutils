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

namespace DiscUtils.Xva
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    [VirtualDiskFactory("XVA", ".xva")]
    internal sealed class DiskFactory : VirtualDiskFactory
    {
        public override string[] Variants
        {
            get { return new string[] { "dynamic" }; }
        }

        public override VirtualDiskTypeInfo GetDiskTypeInformation(string variant)
        {
            return MakeDiskTypeInfo();
        }

        public override DiskImageBuilder GetImageBuilder(string variant)
        {
            throw new NotSupportedException();
        }

        public override VirtualDisk CreateDisk(FileLocator locator, string variant, string path, VirtualDiskParameters diskParameters)
        {
            throw new NotSupportedException();
        }

        public override VirtualDisk OpenDisk(string path, FileAccess access)
        {
            return OpenDisk(new LocalFileLocator(Path.GetDirectoryName(path)), Path.GetFileName(path), access);
        }

        public override VirtualDisk OpenDisk(FileLocator locator, string path, FileAccess access)
        {
            return OpenDisk(locator, path, string.Empty, new Dictionary<string, string>(), access);
        }

        public override VirtualDisk OpenDisk(FileLocator locator, string path, string extraInfo, Dictionary<string, string> parameters, FileAccess access)
        {
            VirtualMachine machine = new VirtualMachine(locator.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), Ownership.Dispose);

            int diskIndex;
            if (!int.TryParse(extraInfo, out diskIndex))
            {
                diskIndex = 0;
            }

            int i = 0;
            foreach (var disk in machine.Disks)
            {
                if (i == diskIndex)
                {
                    return disk;
                }

                ++i;
            }

            return null;
        }

        public override VirtualDiskLayer OpenDiskLayer(FileLocator locator, string path, FileAccess access)
        {
            return null;
        }

        internal static VirtualDiskTypeInfo MakeDiskTypeInfo()
        {
            return new VirtualDiskTypeInfo()
            {
                Name = "XVA",
                Variant = "dynamic",
                CanBeHardDisk = true,
                DeterministicGeometry = true,
                PreservesBiosGeometry = false,
                CalcGeometry = c => Geometry.FromCapacity(c),
            };
        }
    }
}
