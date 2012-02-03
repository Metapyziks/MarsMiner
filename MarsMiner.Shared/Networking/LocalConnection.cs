/**
 * Copyright (c) 2012 James King [metapyziks@gmail.com]
 *
 * This file is part of MarsMiner.
 * 
 * MarsMiner is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * MarsMiner is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with MarsMiner. If not, see <http://www.gnu.org/licenses/>.
 */

using System.IO;
using System.Threading;

namespace MarsMiner.Shared.Networking
{
    public static class LocalConnection
    {
        private static int myClientToServerWaitingPackets = 0;
        private static MemoryStream myClientToServerStream = new MemoryStream();
        private static long myLastClientToServerReadPos = 0;
        private static long myLastClientToServerWritePos = 0;

        private static int myServerToClientWaitingPackets = 0;
        private static MemoryStream myServerToClientStream = new MemoryStream();
        private static long myLastServerToClientReadPos = 0;
        private static long myLastServerToClientWritePos = 0;

        public static Stream StartClientToServerPacket()
        {
            Monitor.Enter( myClientToServerStream );
            myClientToServerStream.Position = myLastClientToServerWritePos;
            return myClientToServerStream;
        }

        public static Stream StartServerToClientPacket()
        {
            Monitor.Enter( myServerToClientStream );
            myServerToClientStream.Position = myLastServerToClientWritePos;
            return myServerToClientStream;
        }

        public static void SendClientToServerPacket()
        {
            myLastClientToServerWritePos = myClientToServerStream.Position;
            Monitor.Exit( myClientToServerStream );
            ++myClientToServerWaitingPackets;
        }

        public static void SendServerToClientPacket()
        {
            myLastServerToClientWritePos = myServerToClientStream.Position;
            Monitor.Exit( myServerToClientStream );
            ++myServerToClientWaitingPackets;
        }

        public static bool ClientToServerPending()
        {
            return myClientToServerWaitingPackets != 0;
        }

        public static bool ServerToClientPending()
        {
            return myServerToClientWaitingPackets != 0;
        }

        public static BinaryReader ReadClientToServerPacket()
        {
            if ( myClientToServerWaitingPackets == 0 )
                throw new EndOfStreamException();

            --myClientToServerWaitingPackets;

            Monitor.Enter( myClientToServerStream );
            myClientToServerStream.Position = myLastClientToServerReadPos;
            return new BinaryReader( myClientToServerStream );
        }

        public static BinaryReader ReadServerToClientPacket()
        {
            if ( myServerToClientWaitingPackets == 0 )
                throw new EndOfStreamException();

            --myServerToClientWaitingPackets;

            Monitor.Enter( myServerToClientStream );
            myServerToClientStream.Position = myLastServerToClientReadPos;
            return new BinaryReader( myServerToClientStream );
        }

        public static void EndReadingClientToServerPacket()
        {
            myLastClientToServerReadPos = myClientToServerStream.Position;
            if ( myLastClientToServerReadPos == myLastClientToServerWritePos && myClientToServerStream.Length >= 2048 )
            {
                myClientToServerStream.Position = 0;
                myLastClientToServerReadPos = 0;
                myLastClientToServerWritePos = 0;
            }
            Monitor.Exit( myClientToServerStream );
        }

        public static void EndReadingServerToClientPacket()
        {
            myLastServerToClientReadPos = myServerToClientStream.Position;
            if ( myLastServerToClientReadPos == myLastServerToClientWritePos && myServerToClientStream.Length >= 2048 )
            {
                myServerToClientStream.Position = 0;
                myLastServerToClientReadPos = 0;
                myLastServerToClientWritePos = 0;
            }
            Monitor.Exit( myServerToClientStream );
        }

        public static void Reset()
        {
            myClientToServerWaitingPackets = 0;
            myClientToServerStream = new MemoryStream();
            myLastClientToServerReadPos = 0;
            myLastClientToServerWritePos = 0;

            myServerToClientWaitingPackets = 0;
            myServerToClientStream = new MemoryStream();
            myLastServerToClientReadPos = 0;
            myLastServerToClientWritePos = 0;
        }
    }
}
