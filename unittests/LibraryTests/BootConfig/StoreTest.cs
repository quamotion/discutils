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
using DiscUtils.Registry;
using NUnit.Framework;

namespace DiscUtils.BootConfig
{
    [TestFixture]
    public class StoreTest
    {
        [Test]
        public void Initialize()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);


            int i = 0;
            foreach (var obj in s.Objects)
            {
                ++i;
            }
            Assert.AreEqual(0, i);
        }

        [Test]
        public void CreateApplication()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);

            BcdObject obj = s.CreateApplication(ApplicationImageType.WindowsBoot, ApplicationType.BootManager);
            Assert.AreNotEqual(Guid.Empty, obj.Identity);

            Assert.AreEqual(ObjectType.Application, obj.ObjectType);

            BcdObject reGet = s.GetObject(obj.Identity);
            Assert.AreEqual(obj.Identity, reGet.Identity);
        }

        [Test]
        public void CreateDevice()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);

            BcdObject obj = s.CreateDevice();
            Assert.AreNotEqual(Guid.Empty, obj.Identity);

            Assert.AreEqual(ObjectType.Device, obj.ObjectType);

            BcdObject reGet = s.GetObject(obj.Identity);
            Assert.AreEqual(obj.Identity, reGet.Identity);
        }

        [Test]
        public void CreateInherit()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);

            BcdObject obj = s.CreateInherit(InheritType.ApplicationObjects);
            Assert.AreNotEqual(Guid.Empty, obj.Identity);

            Assert.AreEqual(ObjectType.Inherit, obj.ObjectType);

            Assert.IsTrue(obj.IsInheritableBy(ObjectType.Application));
            Assert.IsFalse(obj.IsInheritableBy(ObjectType.Device));

            BcdObject reGet = s.GetObject(obj.Identity);
            Assert.AreEqual(obj.Identity, reGet.Identity);
        }

        [Test]
        public void RemoveObject()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);

            BcdObject obj = s.CreateInherit(InheritType.AnyObject);
            s.RemoveObject(obj.Identity);
        }

        [Test]
        public void RemoveObject_NonExistent()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);

            s.RemoveObject(Guid.NewGuid());
        }
    }
}
