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

using MarsMiner.Shared;
using MarsMiner.Shared.Networking;

namespace MarsMiner.Server.Networking
{
    public delegate bool ClientPacketHandlerDelegate( ClientBase sender,
           ClientPacketType type, Stream stream );

    public class ClientPacketType : PacketType
    {
        public readonly ClientPacketHandlerDelegate PacketHandler;

        internal ClientPacketType( UInt16 id, String name, ClientPacketHandlerDelegate handler )
            : base( id, name )
        {
            PacketHandler = handler;
        }
    }

    public static class PacketManager
    {
        private static UInt16 stNextID = 0x0000;

        private static Dictionary<String,ClientPacketType> stTypeNames
            = new Dictionary<string, ClientPacketType>();
        private static List<ClientPacketType> stTypeIDs = new List<ClientPacketType>();

        public static ClientPacketType Register( String name,
            ClientPacketHandlerDelegate handler )
        {
            return Register( name, handler, stNextID );
        }

        public static ClientPacketType Register( String name,
            ClientPacketHandlerDelegate handler, ushort typeID )
        {
            if ( stTypeNames.ContainsKey( name ) )
                throw new Exception( "Can not register new packet type:"
                    + "packet type already registered with the name \"" + name + "\"" );

            ClientPacketType type = new ClientPacketType( typeID, name, handler );

            while ( stTypeIDs.Count < typeID )
                stTypeIDs.Add( null );

            if ( stTypeIDs.Count == typeID )
                stTypeIDs.Add( type );
            else
                stTypeIDs[ typeID ] = type;

            stTypeNames.Add( name, type );

            stNextID = (ushort) Math.Max( stNextID, typeID + 1 );

            return type;
        }

        public static ClientPacketType GetType( String typeName )
        {
            return stTypeNames[ typeName ];
        }

        public static ClientPacketType GetType( UInt16 typeID )
        {
            return stTypeIDs[ typeID ];
        }

        public static ClientPacketType[] GetAllTypes()
        {
            return stTypeIDs.ToArray();
        }

        public static bool HandlePacket( ClientBase sender, Stream stream )
        {
            UInt16 id = BitConverter.ToUInt16( stream.ReadBytes( 2 ), 0 );

            if ( stNextID <= id )
                throw new Exception( "Unknown packet type recieved" );

            ClientPacketType type = stTypeIDs[ id ];
            return type.PacketHandler( sender, type, stream );
        }
    }
}
