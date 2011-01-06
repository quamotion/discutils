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

namespace DiscUtils.Iscsi
{
    internal enum LogoutReason
    {
        CloseSession = 0,
        CloseConnection = 1,
        CloseForRecovery = 2
    }

    internal class LogoutRequest
    {
        private Connection _connection;

        public LogoutRequest(Connection connection)
        {
            _connection = connection;
        }

        public byte[] GetBytes(LogoutReason reason)
        {
            BasicHeaderSegment _basicHeader = new BasicHeaderSegment();
            _basicHeader.Immediate = true;
            _basicHeader.OpCode = OpCode.LogoutRequest;
            _basicHeader.FinalPdu = true;
            _basicHeader.TotalAhsLength = 0;
            _basicHeader.DataSegmentLength = 0;
            _basicHeader.InitiatorTaskTag = _connection.Session.CurrentTaskTag;

            byte[] buffer = new byte[Utilities.RoundUp(48, 4)];
            _basicHeader.WriteTo(buffer, 0);
            buffer[1] |= (byte)((byte)reason & 0x7F);
            Utilities.WriteBytesBigEndian(_connection.Id, buffer, 20);
            Utilities.WriteBytesBigEndian(_connection.Session.CommandSequenceNumber, buffer, 24);
            Utilities.WriteBytesBigEndian(_connection.ExpectedStatusSequenceNumber, buffer, 28);
            return buffer;
        }
    }
}
