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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using MarsMiner.Shared.Networking;

namespace MarsMiner.Client.Networking
{
    public class LocalServer : ServerBase
    {
        public override IPAddress IPAddress
        {
            get
            {
                return IPAddress.Loopback;
            }
        }

        public override void CheckForPackets()
        {
            while ( LocalConnection.ServerToClientPending() )
            {
                Stream str = LocalConnection.ReadServerToClientPacket();
                ReadPacket( str );
                LocalConnection.EndReadingServerToClientPacket();
            }
        }

        protected override bool ReadPacket( Stream stream )
        {
            return base.ReadPacket( stream );
        }

        public override Stream StartPacket( PacketType type )
        {
            Stream str = LocalConnection.StartClientToServerPacket();
            return base.StartPacket( type, str );
        }

        public override bool PacketPending()
        {
            return LocalConnection.ServerToClientPending();
        }

        public override void SendPacket()
        {
            LocalConnection.SendClientToServerPacket();
        }

        protected override bool AttemptConnection()
        {
            LocalConnection.EstablishConnection();

            DateTime start = DateTime.Now;

            while ( LocalConnection.ConnectionWaiting )
                if ( ( DateTime.Now - start ).TotalSeconds > 5.0 )
                    return false;
            
            return true;
        }
    }
}
