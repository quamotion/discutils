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
    public class BcdObjectTest
    {
        [Test]
        public void AddElement()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);
            BcdObject obj = s.CreateInherit(InheritType.AnyObject);

            Assert.IsFalse(obj.HasElement(WellKnownElement.LibraryApplicationPath));
            obj.AddElement(WellKnownElement.LibraryApplicationPath, ElementValue.ForString(@"\a\path\to\nowhere"));
            Assert.IsTrue(obj.HasElement(WellKnownElement.LibraryApplicationPath));

            Assert.AreEqual(@"\a\path\to\nowhere", obj.GetElement(WellKnownElement.LibraryApplicationPath).Value.ToString());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddElement_WrongType()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);
            BcdObject obj = s.CreateInherit(InheritType.AnyObject);

            obj.AddElement(WellKnownElement.LibraryApplicationDevice, ElementValue.ForString(@"\a\path\to\nowhere"));
        }

        [Test]
        public void RemoveElement()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);
            BcdObject obj = s.CreateInherit(InheritType.AnyObject);

            obj.AddElement(WellKnownElement.LibraryApplicationPath, ElementValue.ForString(@"\a\path\to\nowhere"));
            obj.RemoveElement(WellKnownElement.LibraryApplicationPath);

            Assert.IsFalse(obj.HasElement(WellKnownElement.LibraryApplicationPath));
        }

        [Test]
        public void RemoveElement_NonExistent()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);
            BcdObject obj = s.CreateInherit(InheritType.AnyObject);

            obj.RemoveElement(WellKnownElement.LibraryApplicationPath);
        }

        [Test]
        public void FriendlyName()
        {
            RegistryHive hive = RegistryHive.Create(new MemoryStream());
            Store s = Store.Initialize(hive.Root);
            BcdObject obj = s.CreateInherit(InheritType.AnyObject);

            Assert.AreEqual(obj.Identity.ToString("B"), obj.FriendlyName);
        }
    }
}
