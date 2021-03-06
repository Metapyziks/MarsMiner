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

using MarsMiner.Shared.Octree;

namespace MarsMiner.Shared.Geometry
{
    public static class BlockManager
    {
        private static UInt16 myNextID = 0;
        private static List<BlockType> myBlockTypes = new List<BlockType>();
        private static Dictionary<String,List<UInt16>> myIDs = new Dictionary<string, List<ushort>>();

        public static int TypeCount
        {
            get { return myBlockTypes.Count; }
        }

        public static void ClearTypes()
        {
            myNextID = 0;
            myBlockTypes.Clear();
            myIDs.Clear();
        }

        public static BlockType RegisterType( String name, int subType = 0 )
        {
            if ( TypeCount == 0xFFFF )
                throw new Exception( "No more than 65536 block types can be registered." );

            BlockType type = new BlockType( name, subType );

            myBlockTypes.Add( type );
            if( !myIDs.ContainsKey( type.Name ) )
                myIDs.Add( type.Name, new List<ushort>() );

            List<ushort> subTypes = myIDs[ type.Name ];
            while ( subTypes.Count <= type.SubType )
                subTypes.Add( 0xFFFF );
            subTypes[ type.SubType ] = myNextID++;

            return type;
        }

        public static BlockType Get( UInt16 id )
        {
            return myBlockTypes[ id ];
        }

        public static BlockType[] GetAll()
        {
            return myBlockTypes.ToArray();
        }

        public static BlockType Get( String name, int subType = 0 )
        {
            return Get( GetID( name, subType ) );
        }

        public static UInt16 GetID( String name, int subType = 0 )
        {
            return myIDs[ name ][ subType ];
        }
    }
}
