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

namespace MarsMiner.Client.Networking
{
    public delegate bool ServerPacketHandlerDelegate( ServerBase sender,
           ServerPacketType type, Stream stream );

    public class ServerPacketType : PacketType
    {
        public readonly ServerPacketHandlerDelegate PacketHandler;

        internal ServerPacketType( String name, ServerPacketHandlerDelegate handler )
            : base( 0xFFFF, name )
        {
            PacketHandler = handler;
        }
    }

    public static class PacketManager
    {
        private static Dictionary<String,ServerPacketType> stTypeNames
            = new Dictionary<string, ServerPacketType>();
        private static List<ServerPacketType> stTypeIDs = new List<ServerPacketType>();

        public static ServerPacketType Register( String name,
            ServerPacketHandlerDelegate handler )
        {
            if ( stTypeNames.ContainsKey( name ) )
                throw new Exception( "Can not register new packet type: "
                    + "packet type already registered with the name \"" + name + "\"" );

            ServerPacketType type = new ServerPacketType( name, handler );

            stTypeNames.Add( name, type );

            return type;
        }

        public static ServerPacketType Register( String name,
            ServerPacketHandlerDelegate handler, ushort typeID )
        {
            ServerPacketType type = Register( name, handler );

            SetTypeID( name, typeID );

            return type;
        }

        public static void SetTypeID( String name, UInt16 typeID )
        {
            ServerPacketType type = stTypeNames[ name ];

            while ( stTypeIDs.Count < typeID )
                stTypeIDs.Add( null );

            if ( stTypeIDs.Count == typeID )
                stTypeIDs.Add( type );
            else
                stTypeIDs[ typeID ] = type;

            type.ID = typeID;
        }

        public static ServerPacketType GetType( String typeName )
        {
            return stTypeNames[ typeName ];
        }

        public static ServerPacketType GetType( UInt16 typeID )
        {
            return stTypeIDs[ typeID ];
        }

        public static ServerPacketType[] GetAllTypes()
        {
            return stTypeIDs.ToArray();
        }

        public static bool HandlePacket( ServerBase sender, Stream stream )
        {
            UInt16 id = BitConverter.ToUInt16( stream.ReadBytes( 2 ), 0 );

            if ( stTypeIDs.Count <= id )
                throw new Exception( "Unknown packet type recieved" );

            ServerPacketType type = stTypeIDs[ id ];

            if ( type == null )
                throw new Exception( "Unknown packet type recieved" );

            return type.PacketHandler( sender, type, stream );
        }
    }
}
