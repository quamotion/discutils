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

namespace DiscUtils.Vmdk
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    [VirtualDiskFactory("VMDK", ".vmdk")]
    internal sealed class DiskFactory : VirtualDiskFactory
    {
        public override string[] Variants
        {
            get { return new string[] { "fixed", "dynamic", "vmfsfixed", "vmfsdynamic" }; }
        }

        public override DiskImageBuilder GetImageBuilder(string variant)
        {
            DiskBuilder builder = new DiskBuilder();

            switch (variant)
            {
                case "fixed":
                    builder.DiskType = DiskCreateType.MonolithicFlat;
                    break;
                case "dynamic":
                    builder.DiskType = DiskCreateType.MonolithicSparse;
                    break;
                case "vmfsfixed":
                    builder.DiskType = DiskCreateType.Vmfs;
                    break;
                case "vmfsdynamic":
                    builder.DiskType = DiskCreateType.VmfsSparse;
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unknown VMDK disk variant '{0}'", variant), "variant");
            }

            return builder;
        }

        public override VirtualDisk CreateDisk(FileLocator locator, string variant, string path, long capacity, Geometry geometry, Dictionary<string, string> parameters)
        {
            DiskParameters vmdkParams = new DiskParameters();
            vmdkParams.Capacity = capacity;
            vmdkParams.Geometry = geometry;

            switch (variant)
            {
                case "fixed":
                    vmdkParams.CreateType = DiskCreateType.MonolithicFlat;
                    break;
                case "dynamic":
                    vmdkParams.CreateType = DiskCreateType.MonolithicSparse;
                    break;
                case "vmfsfixed":
                    vmdkParams.CreateType = DiskCreateType.Vmfs;
                    break;
                case "vmfsdynamic":
                    vmdkParams.CreateType = DiskCreateType.VmfsSparse;
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unknown VMDK disk variant '{0}'", variant), "variant");
            }

            return Disk.Initialize(locator, path, vmdkParams);
        }

        public override VirtualDisk OpenDisk(string path, FileAccess access)
        {
            return new Disk(path, access);
        }

        public override VirtualDisk OpenDisk(FileLocator locator, string path, FileAccess access)
        {
            return new Disk(locator, path, access);
        }

        public override VirtualDiskLayer OpenDiskLayer(FileLocator locator, string path, FileAccess access)
        {
            return new DiskImageFile(locator, path, access);
        }
    }
}
