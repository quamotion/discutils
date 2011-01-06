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

using System;
using System.IO;
using NUnit.Framework;

namespace DiscUtils.Iso9660
{
    [TestFixture]
    public class IsoDirectoryInfoTest
    {
        [Test]
        public void Exists()
        {
            CDBuilder builder = new CDBuilder();
            builder.AddFile(@"SOMEDIR\CHILDDIR\FILE.TXT", new byte[0]);
            CDReader fs = new CDReader(builder.Build(), false);

            Assert.IsTrue(fs.GetDirectoryInfo(@"\").Exists);
            Assert.IsTrue(fs.GetDirectoryInfo(@"SOMEDIR").Exists);
            Assert.IsTrue(fs.GetDirectoryInfo(@"SOMEDIR\CHILDDIR").Exists);
            Assert.IsTrue(fs.GetDirectoryInfo(@"SOMEDIR\CHILDDIR\").Exists);
            Assert.IsFalse(fs.GetDirectoryInfo(@"NONDIR").Exists);
            Assert.IsFalse(fs.GetDirectoryInfo(@"SOMEDIR\NONDIR").Exists);
        }

        [Test]
        public void FullName()
        {
            CDBuilder builder = new CDBuilder();
            CDReader fs = new CDReader(builder.Build(), false);

            Assert.AreEqual(@"\", fs.Root.FullName);
            Assert.AreEqual(@"SOMEDIR\", fs.GetDirectoryInfo(@"SOMEDIR").FullName);
            Assert.AreEqual(@"SOMEDIR\CHILDDIR\", fs.GetDirectoryInfo(@"SOMEDIR\CHILDDIR").FullName);
        }

        [Test]
        public void SimpleSearch()
        {
            CDBuilder builder = new CDBuilder();
            builder.AddFile(@"SOMEDIR\CHILDDIR\GCHILDIR\FILE.TXT", new byte[0]);
            CDReader fs = new CDReader(builder.Build(), false);

            DiscDirectoryInfo di = fs.GetDirectoryInfo(@"SOMEDIR\CHILDDIR");
            DiscFileInfo[] fis = di.GetFiles("*.*", SearchOption.AllDirectories);
        }

        [Test]
        public void Extension()
        {
            CDBuilder builder = new CDBuilder();
            CDReader fs = new CDReader(builder.Build(), false);

            Assert.AreEqual("dir", fs.GetDirectoryInfo("fred.dir").Extension);
            Assert.AreEqual("", fs.GetDirectoryInfo("fred").Extension);
        }

        [Test]
        public void GetDirectories()
        {
            CDBuilder builder = new CDBuilder();
            builder.AddDirectory(@"SOMEDIR\CHILD\GCHILD");
            builder.AddDirectory(@"A.DIR");
            CDReader fs = new CDReader(builder.Build(), false);


            Assert.AreEqual(2, fs.Root.GetDirectories().Length);

            DiscDirectoryInfo someDir = fs.Root.GetDirectories(@"SoMeDir")[0];
            Assert.AreEqual(1, fs.Root.GetDirectories("SOMEDIR").Length);
            Assert.AreEqual("SOMEDIR", someDir.Name);

            Assert.AreEqual(1, someDir.GetDirectories("*.*").Length);
            Assert.AreEqual("CHILD", someDir.GetDirectories("*.*")[0].Name);
            Assert.AreEqual(2, someDir.GetDirectories("*.*", SearchOption.AllDirectories).Length);

            Assert.AreEqual(4, fs.Root.GetDirectories("*.*", SearchOption.AllDirectories).Length);
            Assert.AreEqual(2, fs.Root.GetDirectories("*.*", SearchOption.TopDirectoryOnly).Length);

            Assert.AreEqual(1, fs.Root.GetDirectories("*.DIR", SearchOption.AllDirectories).Length);
            Assert.AreEqual(@"A.DIR\", fs.Root.GetDirectories("*.DIR", SearchOption.AllDirectories)[0].FullName);

            Assert.AreEqual(1, fs.Root.GetDirectories("GCHILD", SearchOption.AllDirectories).Length);
            Assert.AreEqual(@"SOMEDIR\CHILD\GCHILD\", fs.Root.GetDirectories("GCHILD", SearchOption.AllDirectories)[0].FullName);
        }

        [Test]
        public void GetFiles()
        {
            CDBuilder builder = new CDBuilder();
            builder.AddDirectory(@"SOMEDIR\CHILD\GCHILD");
            builder.AddDirectory(@"AAA.DIR");
            builder.AddFile(@"FOO.TXT", new byte[10]);
            builder.AddFile(@"SOMEDIR\CHILD.TXT", new byte[10]);
            builder.AddFile(@"SOMEDIR\FOO.TXT", new byte[10]);
            builder.AddFile(@"SOMEDIR\CHILD\GCHILD\BAR.TXT", new byte[10]);
            CDReader fs = new CDReader(builder.Build(), false);

            Assert.AreEqual(1, fs.Root.GetFiles().Length);
            Assert.AreEqual("FOO.TXT", fs.Root.GetFiles()[0].FullName);

            Assert.AreEqual(2, fs.Root.GetDirectories("SOMEDIR")[0].GetFiles("*.TXT").Length);
            Assert.AreEqual(4, fs.Root.GetFiles("*.TXT", SearchOption.AllDirectories).Length);

            Assert.AreEqual(0, fs.Root.GetFiles("*.DIR", SearchOption.AllDirectories).Length);
        }

        [Test]
        public void GetFileSystemInfos()
        {
            CDBuilder builder = new CDBuilder();
            builder.AddDirectory(@"SOMEDIR\CHILD\GCHILD");
            builder.AddDirectory(@"AAA.EXT");
            builder.AddFile(@"FOO.TXT", new byte[10]);
            builder.AddFile(@"SOMEDIR\CHILD.TXT", new byte[10]);
            builder.AddFile(@"SOMEDIR\FOO.TXT", new byte[10]);
            builder.AddFile(@"SOMEDIR\CHILD\GCHILD\BAR.TXT", new byte[10]);
            CDReader fs = new CDReader(builder.Build(), false);

            Assert.AreEqual(3, fs.Root.GetFileSystemInfos().Length);

            Assert.AreEqual(1, fs.Root.GetFileSystemInfos("*.EXT").Length);
            Assert.AreEqual(2, fs.Root.GetFileSystemInfos("*.?XT").Length);
        }

        [Test]
        public void Parent()
        {
            CDBuilder builder = new CDBuilder();
            builder.AddDirectory(@"SOMEDIR");
            CDReader fs = new CDReader(builder.Build(), false);

            Assert.AreEqual(fs.Root, fs.Root.GetDirectories("SOMEDIR")[0].Parent);
        }

        [Test]
        public void Parent_Root()
        {
            CDBuilder builder = new CDBuilder();
            CDReader fs = new CDReader(builder.Build(), false);

            Assert.IsNull(fs.Root.Parent);
        }

        [Test]
        public void RootBehaviour()
        {
            // Start time rounded down to whole seconds
            DateTime start = DateTime.UtcNow;
            start = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, start.Second);

            CDBuilder builder = new CDBuilder();
            CDReader fs = new CDReader(builder.Build(), false);
            DateTime end = DateTime.UtcNow;

            Assert.AreEqual(FileAttributes.Directory | FileAttributes.ReadOnly, fs.Root.Attributes);
            Assert.GreaterOrEqual(fs.Root.CreationTimeUtc, start);
            Assert.LessOrEqual(fs.Root.CreationTimeUtc, end);
            Assert.GreaterOrEqual(fs.Root.LastAccessTimeUtc, start);
            Assert.LessOrEqual(fs.Root.LastAccessTimeUtc, end);
            Assert.GreaterOrEqual(fs.Root.LastWriteTimeUtc, start);
            Assert.LessOrEqual(fs.Root.LastWriteTimeUtc, end);
        }

        [Test]
        public void Attributes()
        {
            // Start time rounded down to whole seconds
            DateTime start = DateTime.UtcNow;
            start = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, start.Second);

            CDBuilder builder = new CDBuilder();
            builder.AddDirectory("Foo");
            CDReader fs = new CDReader(builder.Build(), false);
            DateTime end = DateTime.UtcNow;

            DiscDirectoryInfo di = fs.GetDirectoryInfo("Foo");

            Assert.AreEqual(FileAttributes.Directory | FileAttributes.ReadOnly, di.Attributes);
            Assert.GreaterOrEqual(di.CreationTimeUtc, start);
            Assert.LessOrEqual(di.CreationTimeUtc, end);
            Assert.GreaterOrEqual(di.LastAccessTimeUtc, start);
            Assert.LessOrEqual(di.LastAccessTimeUtc, end);
            Assert.GreaterOrEqual(di.LastWriteTimeUtc, start);
            Assert.LessOrEqual(di.LastWriteTimeUtc, end);
        }
    }
}
