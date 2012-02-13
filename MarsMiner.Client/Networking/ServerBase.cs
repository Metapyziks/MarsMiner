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
using System.IO;

using MarsMiner.Shared.Networking;

namespace MarsMiner.Client.Networking
{
    public class ServerBase : RemoteNetworkedObject
    {
        protected static readonly ServerPacketType PTAliveCheck =
            PacketManager.Register( "AliveCheck", delegate( ServerBase sender,
                ServerPacketType type, Stream stream )
            {
                return sender.OnReceiveAliveCheck( stream );
            } );

        protected static readonly ServerPacketType PTPacketDictionary =
            PacketManager.Register( "PacketDictionary", delegate( ServerBase sender,
                ServerPacketType type, Stream stream )
            {
                return sender.OnReceivePacketDictionary( stream );
            } );


        protected override bool ReadPacket( Stream stream )
        {
            base.ReadPacket( stream );
            return PacketManager.HandlePacket( this, stream );
        }

        public override Stream StartPacket( String typeName )
        {
            return StartPacket( PacketManager.GetType( typeName ) );
        }

        public void SendPacketDictionary()
        {
            StartPacket( PTPacketDictionary );
            SendPacket();
        }

        protected bool OnReceivePacketDictionary( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            UInt16 count = reader.ReadUInt16();
            for ( int i = 0; i < count; ++i )
            {
                String name = reader.ReadString();
                UInt16 id = reader.ReadUInt16();

                try
                {
                    PacketManager.SetTypeID( name, id );
                }
                catch ( KeyNotFoundException )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
