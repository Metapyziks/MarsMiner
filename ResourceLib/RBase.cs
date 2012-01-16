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
using System.Reflection;
using System.Drawing;

namespace ResourceLib
{
    public struct ResourceItem
    {
        public String Key;
        public Object Value;
    
        public ResourceItem( String key, Object value )
        {
            Key = key;
            Value = value;
        }
    }

    public class RManager
    {
        internal readonly String[] FileExtensions;
        internal readonly Type ValueType;

        internal readonly int Priority;

        public RManager( Type valueType, int priority, params string[] fileExtensions )
        {
            ValueType = valueType;
            FileExtensions = fileExtensions;
            Priority = priority;
        }

        public virtual Object LoadFromArchive( BinaryReader stream )
        {
            return default( Object );
        }

        public virtual void SaveToArchive( BinaryWriter stream, Object item )
        {

        }

        public virtual ResourceItem[] LoadFromFile( String keyPrefix, String fileName, String fileExtension, FileStream stream )
        {
            return new ResourceItem[ 0 ];
        }
    }

    public class RTextManager : RManager
    {
        public RTextManager()
            : base( typeof(String), 0, "txt", "lang" )
        {

        }

        public override ResourceItem[] LoadFromFile( String keyPrefix, String fileName, String fileExtension, FileStream stream )
        {
            StreamReader reader = new StreamReader( stream );
            String text = reader.ReadToEnd();
            reader.Close();

            if ( fileExtension == "lang" )
            {
                String[] split = text.Split( '"' );

                ResourceItem[] items = new ResourceItem[ ( split.Length - 1 ) / 4 ];
                for ( int i = 1, j = 0; i < split.Length; i += 4, ++ j )
                    items[ j ] = new ResourceItem( split[ i ].ToLower(), split[ i + 2 ] );

                return items;
            }
            else
                return new ResourceItem[] { new ResourceItem( keyPrefix + fileName, text ) };
        }

        public override Object LoadFromArchive( BinaryReader stream )
        {
            return stream.ReadString();
        }

        public override void SaveToArchive( BinaryWriter stream, Object item )
        {
            stream.Write( (String) item );
        }
    }
}
