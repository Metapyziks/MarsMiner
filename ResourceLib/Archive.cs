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
using System.Security.Cryptography;
using System.Drawing;
using System.Reflection;

using ICSharpCode.SharpZipLib.BZip2;

namespace ResourceLib
{
    public enum ArchiveDest : byte
    {
        Server = 1,
        Client = 2,
        Shared = 3
    }

    internal class Archive
    {
        private struct ResourceInfo
        {
            public String FilePath;
            public String CurPath;
            public String FileName;
            public String Extension;
            public RManager Manager;
        }

        public const ushort ArchiverVersion = 0x0000;

        internal static Archive CreateFromDirectory( String directoryPath )
        {
            if ( directoryPath.EndsWith( "\\" ) || directoryPath.EndsWith( "/" ) )
                directoryPath = directoryPath.Substring( 0, directoryPath.Length - 1 );

            if ( !File.Exists( directoryPath + "\\info.txt" ) )
                throw new Exception( "info.txt not present!" );

            Archive archive = new Archive();

            archive.LoadPropertiesFromInfoFile( directoryPath + "\\info.txt" );

            List<ResourceInfo> resources = new List<ResourceInfo>();

            ExploreDirectory( resources, directoryPath, "" );

            resources = resources.OrderBy( x => x.Manager.Priority ).ToList();

            foreach ( ResourceInfo info in resources )
            {
                Dictionary<String, Object> dict = archive.myDictionaries[ info.Manager.ValueType ];

                FileStream stream = new FileStream( info.FilePath, FileMode.Open, FileAccess.Read );
                ResourceItem[] items = info.Manager.LoadFromFile( info.CurPath, info.FileName, info.Extension, stream );
                stream.Close();

                foreach ( ResourceItem item in items )
                    dict.Add( item.Key, item.Value );
            }

            return archive;
        }

        private static void ExploreDirectory( List<ResourceInfo> resources, String directoryPath, String curPath )
        {
            foreach ( String file in Directory.EnumerateFiles( directoryPath ) )
            {
                if ( file.Split( '\\' ).Last().Split( '.' ).Length != 2 )
                    continue;

                string fileName = file.Split( '\\' ).Last().Split( '.' )[ 0 ];
                string extension = file.Split( '\\' ).Last().Split( '.' )[ 1 ].ToLower();

                if ( curPath == "" && fileName == "info" && extension == "txt" )
                    continue;

                RManager manager = Res.GetManager( extension );

                resources.Add( new ResourceInfo { FilePath = file, CurPath = curPath, FileName = fileName, Extension = extension, Manager = manager } );
            }

            foreach ( String dir in Directory.EnumerateDirectories( directoryPath ) )
                ExploreDirectory( resources, dir, curPath + dir.Split( '\\' ).Last() + "_" );
        }

        private String myName;
        private String myVersion;
        private String myAuthor;
        private String myAuthorEmail;
        private String myAuthorWebsite;
        private String myDescription;
        private String myHash;
        private ArchiveDest myDest;

        private Dictionary<Type,Dictionary<String,Object>> myDictionaries;

        internal String Name
        {
            get
            {
                return myName;
            }
        }

        internal String Version
        {
            get
            {
                return myVersion;
            }
        }

        internal String Author
        {
            get
            {
                return myAuthor;
            }
        }

        internal String AuthorEmail
        {
            get
            {
                return myAuthorEmail;
            }
        }

        internal String AuthorWebsite
        {
            get
            {
                return myAuthorWebsite;
            }
        }

        internal String Description
        {
            get
            {
                return myDescription;
            }
        }

        internal String Hash
        {
            get
            {
                return myHash;
            }
        }

        internal ArchiveDest Destination
        {
            get
            {
                return myDest;
            }
        }

        internal Dictionary<Type, Dictionary<String, Object>> Dictionaries
        {
            get
            {
                return myDictionaries;
            }
        }

        private Archive()
        {
            myDictionaries = new Dictionary<Type, Dictionary<string, object>>();

            foreach( RManager manager in Res.Managers )
                myDictionaries.Add( manager.ValueType, new Dictionary<String, Object>() );

            myName = "Untitled";
            myAuthor = "Unknown";
            myAuthorEmail = "None given";
            myAuthorWebsite = "None given";
            myDescription = "None given";
            myVersion = "1.0";
            myDest = ArchiveDest.Shared;
        }

        internal Archive( String filePath )
            : this()
        {
            ushort archiverVersion;
            byte[] bytes;
            byte[] hash;

            using ( FileStream fstr = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                using ( BinaryReader bstr = new BinaryReader( fstr ) )
                {
                    archiverVersion = bstr.ReadUInt16();
                    myName = bstr.ReadString();
                    myAuthor = bstr.ReadString();
                    myVersion = bstr.ReadString();
                    myAuthorWebsite = bstr.ReadString();
                    myAuthorEmail = bstr.ReadString();
                    myDescription = bstr.ReadString();
                    myDest = (ArchiveDest) bstr.ReadByte();

                    int len = bstr.ReadInt32();
                    bytes = bstr.ReadBytes( len );
                    len = bstr.ReadInt32();
                    hash = bstr.ReadBytes( len );
                }
            }

            myHash = Encoding.ASCII.GetString( hash );

            byte[] data;

            using ( MemoryStream mstr = new MemoryStream( bytes ) )
            {
                using ( BZip2InputStream cstr = new BZip2InputStream( mstr ) )
                {
                    using( MemoryStream ostr = new MemoryStream() )
                    {
                        int chunkSize = 4096;

                        byte[] buffer = new byte[ chunkSize ];
                        int len;

                        while ( ( len = cstr.Read( buffer, 0, chunkSize ) ) > 0 )
                            ostr.Write( buffer, 0, len );

                        ostr.Flush();

                        data = ostr.ToArray();
                    }
                }
            }

            byte[] newHash = new SHA256CryptoServiceProvider().ComputeHash( data );

            if ( !Enumerable.SequenceEqual( hash, newHash ) )
                throw new Exception( "The data in archive \"" + Name + "\" has been corrupted!" );

            using ( MemoryStream mstr = new MemoryStream( data ) )
            {
                using ( BinaryReader bstr = new BinaryReader( mstr ) )
                {
                    while ( mstr.Position < mstr.Length )
                    {
                        String typeName = bstr.ReadString();
                        Type type = Assembly.GetExecutingAssembly().GetType( typeName );
                        
                        if ( type == null )
                        {
                            foreach ( Type t in myDictionaries.Keys )
                            {
                                if ( typeName == t.FullName )
                                {
                                    type = t;
                                    break;
                                }
                            }
                        }

                        long pos = bstr.ReadUInt32();

                        if ( type != null )
                        {
                            RManager manager = Res.GetManager( type );

                            while ( mstr.Position < pos )
                            {
                                String key = bstr.ReadString();
                                object obj = manager.LoadFromArchive( bstr );
                                myDictionaries[ type ].Add( key, obj );
                            }
                        }

                        mstr.Seek( pos, SeekOrigin.Begin );
                    }
                }
            }
        }

        internal void LoadPropertiesFromInfoFile( String filePath )
        {
            String[] lines = File.ReadAllLines( filePath );

            foreach ( String line in lines )
            {
                if ( line.Trim().Length > 0 )
                {
                    String[] split = line.Split( '"' );
                    String key = split[ 1 ].ToLower();
                    String val = split[ 3 ];

                    switch ( key )
                    {
                        case "name":
                            myName = val; break;
                        case "author":
                            myAuthor = val; break;
                        case "author email":
                            myAuthorEmail = val; break;
                        case "author website":
                            myAuthorWebsite = val; break;
                        case "description":
                            myDescription = val; break;
                        case "version":
                            myVersion = val; break;
                        case "destination":
                            myDest = ( val.ToLower() == "server" ? ArchiveDest.Server : val.ToLower() == "client" ? ArchiveDest.Client : ArchiveDest.Shared ); break;
                    }
                }
            }
        }

        internal void SaveToFile( String filePath )
        {
            byte[] bytes;
            byte[] hash;

            using ( MemoryStream mstr = new MemoryStream() )
            {
                using ( BinaryWriter bstr = new BinaryWriter( mstr ) )
                {
                    foreach ( Type type in myDictionaries.Keys )
                    {
                        Dictionary<String, object> dict = myDictionaries[ type ];

                        if ( dict.Count == 0 )
                            continue;

                        RManager manager = Res.GetManager( type );

                        bstr.Write( type.FullName );
                        long pos = mstr.Position;
                        bstr.Write( (uint) 0xFFFFFFFF );

                        foreach ( String key in dict.Keys )
                        {
                            bstr.Write( key );
                            manager.SaveToArchive( bstr, dict[ key ] );
                        }

                        uint len = (uint) ( mstr.Position );
                        mstr.Seek( pos, SeekOrigin.Begin );
                        bstr.Write( len );
                        mstr.Seek( 0, SeekOrigin.End );
                    }
                }

                bytes = mstr.ToArray();
                hash = new SHA256CryptoServiceProvider().ComputeHash( bytes );
            }

            myHash = Encoding.ASCII.GetString( hash );

            using ( MemoryStream mstr = new MemoryStream() )
            {
                using ( BZip2OutputStream cstr = new BZip2OutputStream( mstr ) )
                {
                    cstr.Write( bytes, 0, bytes.Length );
                }

                bytes = mstr.ToArray();
            }

            using ( FileStream fstr = new FileStream( filePath, FileMode.Create, FileAccess.Write ) )
            {
                using ( BinaryWriter bstr = new BinaryWriter( fstr ) )
                {
                    bstr.Write( ArchiverVersion );
                    bstr.Write( Name );
                    bstr.Write( Author );
                    bstr.Write( Version );
                    bstr.Write( AuthorWebsite );
                    bstr.Write( AuthorEmail );
                    bstr.Write( Description );
                    bstr.Write( (byte) Destination );

                    bstr.Write( bytes.Length );
                    bstr.Write( bytes );

                    bstr.Write( hash.Length );
                    bstr.Write( hash );
                }
            }
        }

        internal T Get<T>( String key )
        {
            Type type = typeof( T );
            
            if ( !myDictionaries[ type ].ContainsKey( key ) )
                throw new ResourceNotFoundException( key );

            return (T) myDictionaries[ type ][ key ];
        }

        internal T Get<T>( String key, T defaultValue )
        {
            Type type = typeof( T );

            if ( !myDictionaries[ type ].ContainsKey( key ) )
                return defaultValue;

            return (T) myDictionaries[ type ][ key ];
        }
    }
}
