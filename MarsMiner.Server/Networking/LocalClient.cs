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

using System.Net;
using System.IO;

using MarsMiner.Shared.Networking;

namespace MarsMiner.Server.Networking
{
    public class LocalClient : ClientBase
    {
        public override IPAddress IPAddress
        {
            get
            {
                return IPAddress.Loopback;
            }
        }

        public LocalClient( GameServer server )
            : base( server )
        {

        }

        public override void CheckForPackets()
        {
            while ( LocalConnection.ClientToServerPending() )
            {
                Stream str = LocalConnection.ReadClientToServerPacket();
                ReadPacket( str );
                LocalConnection.EndReadingClientToServerPacket();
            }
        }

        public override Stream StartPacket( PacketType type )
        {
            Stream str = LocalConnection.StartServerToClientPacket();
            return base.StartPacket( type, str );
        }

        public override bool PacketPending()
        {
            return LocalConnection.ClientToServerPending();
        }

        public override void SendPacket()
        {
            LocalConnection.SendServerToClientPacket();
        }
    }
}
