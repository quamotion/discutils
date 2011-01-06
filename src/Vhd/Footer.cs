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

namespace DiscUtils.Vhd
{
    using System;

    internal class Footer
    {
        public const string FileCookie = "conectix";
        public const uint FeatureNone = 0x0;
        public const uint FeatureTemporary = 0x1;
        public const uint FeatureReservedMustBeSet = 0x2;
        public const uint Version1 = 0x00010000;
        public const uint Version6Point1 = 0x00060001;
        public static readonly DateTime EpochUtc = new DateTime(2000, 1, 1, 0, 0, 0, 0);
        public const string VirtualPCSig = "vpc ";
        public const string VirtualServerSig = "vs  ";
        public const uint VirtualPC2004Version = 0x00050000;
        public const uint VirtualServer2004Version = 0x00010000;
        public const string WindowsHostOS = "Wi2k";
        public const string MacHostOS = "Mac ";
        public const uint CylindersMask = 0x0000FFFF;
        public const uint HeadsMask = 0x00FF0000;
        public const uint SectorsMask = 0xFF000000;

        public string Cookie;
        public uint Features;
        public uint FileFormatVersion;
        public long DataOffset;
        public DateTime Timestamp;
        public string CreatorApp;
        public uint CreatorVersion;
        public string CreatorHostOS;
        public long OriginalSize;
        public long CurrentSize;
        public Geometry Geometry;
        public FileType DiskType;
        public uint Checksum;
        public Guid UniqueId;
        public byte SavedState;

        public Footer(Geometry geometry, long capacity, FileType type)
        {
            Cookie = FileCookie;
            Features = FeatureReservedMustBeSet;
            FileFormatVersion = Version1;
            DataOffset = -1;
            Timestamp = DateTime.UtcNow;
            CreatorApp = "dutl";
            CreatorVersion = Version6Point1;
            CreatorHostOS = WindowsHostOS;
            OriginalSize = capacity;
            CurrentSize = capacity;
            Geometry = geometry;
            DiskType = type;
            UniqueId = Guid.NewGuid();
            ////SavedState = 0;
        }

        public Footer(Footer toCopy)
        {
            Cookie = toCopy.Cookie;
            Features = toCopy.Features;
            FileFormatVersion = toCopy.FileFormatVersion;
            DataOffset = toCopy.DataOffset;
            Timestamp = toCopy.Timestamp;
            CreatorApp = toCopy.CreatorApp;
            CreatorVersion = toCopy.CreatorVersion;
            CreatorHostOS = toCopy.CreatorHostOS;
            OriginalSize = toCopy.OriginalSize;
            CurrentSize = toCopy.CurrentSize;
            Geometry = toCopy.Geometry;
            DiskType = toCopy.DiskType;
            Checksum = toCopy.Checksum;
            UniqueId = toCopy.UniqueId;
            SavedState = toCopy.SavedState;
        }

        private Footer()
        {
        }

        #region Marshalling
        public static Footer FromBytes(byte[] buffer, int offset)
        {
            Footer result = new Footer();
            result.Cookie = Utilities.BytesToString(buffer, offset + 0, 8);
            result.Features = Utilities.ToUInt32BigEndian(buffer, offset + 8);
            result.FileFormatVersion = Utilities.ToUInt32BigEndian(buffer, offset + 12);
            result.DataOffset = Utilities.ToInt64BigEndian(buffer, offset + 16);
            result.Timestamp = EpochUtc.AddSeconds(Utilities.ToUInt32BigEndian(buffer, offset + 24));
            result.CreatorApp = Utilities.BytesToString(buffer, offset + 28, 4);
            result.CreatorVersion = Utilities.ToUInt32BigEndian(buffer, offset + 32);
            result.CreatorHostOS = Utilities.BytesToString(buffer, offset + 36, 4);
            result.OriginalSize = Utilities.ToInt64BigEndian(buffer, offset + 40);
            result.CurrentSize = Utilities.ToInt64BigEndian(buffer, offset + 48);
            result.Geometry = new Geometry(Utilities.ToUInt16BigEndian(buffer, offset + 56), buffer[58], buffer[59]);
            result.DiskType = (FileType)Utilities.ToUInt32BigEndian(buffer, offset + 60);
            result.Checksum = Utilities.ToUInt32BigEndian(buffer, offset + 64);
            result.UniqueId = Utilities.ToGuidBigEndian(buffer, offset + 68);
            result.SavedState = buffer[84];

            return result;
        }

        public void ToBytes(byte[] buffer, int offset)
        {
            Utilities.StringToBytes(Cookie, buffer, offset + 0, 8);
            Utilities.WriteBytesBigEndian(Features, buffer, offset + 8);
            Utilities.WriteBytesBigEndian(FileFormatVersion, buffer, offset + 12);
            Utilities.WriteBytesBigEndian(DataOffset, buffer, offset + 16);
            Utilities.WriteBytesBigEndian((uint)(Timestamp - EpochUtc).TotalSeconds, buffer, offset + 24);
            Utilities.StringToBytes(CreatorApp, buffer, offset + 28, 4);
            Utilities.WriteBytesBigEndian(CreatorVersion, buffer, offset + 32);
            Utilities.StringToBytes(CreatorHostOS, buffer, offset + 36, 4);
            Utilities.WriteBytesBigEndian(OriginalSize, buffer, offset + 40);
            Utilities.WriteBytesBigEndian(CurrentSize, buffer, offset + 48);
            Utilities.WriteBytesBigEndian((ushort)Geometry.Cylinders, buffer, offset + 56);
            buffer[offset + 58] = (byte)Geometry.HeadsPerCylinder;
            buffer[offset + 59] = (byte)Geometry.SectorsPerTrack;
            Utilities.WriteBytesBigEndian((uint)DiskType, buffer, offset + 60);
            Utilities.WriteBytesBigEndian(Checksum, buffer, offset + 64);
            Utilities.WriteBytesBigEndian(UniqueId, buffer, offset + 68);
            buffer[84] = SavedState;
            Array.Clear(buffer, 85, 427);
        }
        #endregion

        public bool IsValid()
        {
            return (Cookie == FileCookie)
                && IsChecksumValid()
                ////&& ((Features & FeatureReservedMustBeSet) != 0)
                && FileFormatVersion == Version1;
        }

        public bool IsChecksumValid()
        {
            return Checksum == CalculateChecksum();
        }

        public uint UpdateChecksum()
        {
            Checksum = CalculateChecksum();
            return Checksum;
        }

        private uint CalculateChecksum()
        {
            Footer copy = new Footer(this);
            copy.Checksum = 0;

            byte[] asBytes = new byte[512];
            copy.ToBytes(asBytes, 0);
            uint checksum = 0;
            foreach (uint value in asBytes)
            {
                checksum += value;
            }

            checksum = ~checksum;

            return checksum;
        }
    }
}
