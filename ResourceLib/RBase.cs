/**
 * Copyright (c) 2012 James King
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 * 
 * James King [metapyziks@gmail.com]
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
