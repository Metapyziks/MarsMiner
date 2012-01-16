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
using System.Drawing;

namespace ResourceLib
{
    public class ArchiveNotLoadedException : Exception
    {
        public ArchiveNotLoadedException( int index )
            : base( index < 256 ? "No archives are loaded at the given index (" + index + ")" : "No more than 256 archives can be loaded at a time" )
        {

        }
    }

    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException( String key )
            : base( "Resource not found with the key \"" + key + "\"." )
        {

        }
    }

    public enum ArchiveProperty
    {
        Name,
        Author,
        Version,
        AuthorEmail,
        AuthorWebsite,
        Description
    }

    public static class Res
    {
        public const String DefaultResourceExtension = ".rsa";

        private static Archive[] stLoadedArchives = new Archive[256];
        private static int stFirstAvaliableIndex = 0;

        private static Dictionary<String,RManager> stResourceExtensions = new Dictionary<string,RManager>();
        private static Dictionary<Type,RManager> stResourceManagers = new Dictionary<Type,RManager>();

        private static Dictionary<Type,Dictionary<String,Object>> stResourceDictionaries = new Dictionary<Type,Dictionary<String,Object>>();
        
        static Res()
        {
            RegisterManager( new RTextManager() );
            RegisterManager( new RInfoManager() );
        }

        internal static RManager[] Managers
        {
            get
            {
                return stResourceManagers.Values.ToArray();
            }
        }

        internal static RManager GetManager( Type valueType )
        {
            return stResourceManagers[ valueType ];
        }

        internal static RManager GetManager( String extension )
        {
            return stResourceExtensions[ extension ];
        }

        public static void RegisterManager( RManager manager )
        {
            stResourceManagers.Add( manager.ValueType, manager );
            
            foreach( String ext in manager.FileExtensions )
                stResourceExtensions.Add( ext, manager );

            stResourceDictionaries.Add( manager.ValueType, new Dictionary<string, object>() );
        }

        public static void LoadFromOrderFile( String orderFilePath )
        {
            UnloadAllArchives();

            String rootPath = orderFilePath.Substring( 0, orderFilePath.LastIndexOf( Path.DirectorySeparatorChar ) ) + Path.DirectorySeparatorChar;

            String[] lines = File.ReadAllLines( orderFilePath );
            foreach ( String line in lines )
            {
                string path = line.Split( new String[] { "//" }, StringSplitOptions.None )[ 0 ].Trim();
                if ( path != "" )
                {
                    int archive = LoadArchive( rootPath + path );
                    MountArchive( archive );
                }
            }
        }

        public static int LoadArchive( String filePath )
        {
            if ( stFirstAvaliableIndex >= 256 )
                throw new ArchiveNotLoadedException( 256 );

            try
            {
                stLoadedArchives[ stFirstAvaliableIndex ] = new Archive( filePath );
            }
            catch ( Exception e )
            {
                throw new Exception( "Resources failed to load from file \"" + filePath + "\"", e );
            }

            int usedIndex = stFirstAvaliableIndex;

            IncrementIndex();

            return usedIndex;
        }

        public static void UnloadArchive( int archive )
        {
            if ( archive < 0 || archive >= 256 || stLoadedArchives[ archive ] == null )
                throw new ArchiveNotLoadedException( archive );

            stLoadedArchives[ archive ] = null;

            if ( archive < stFirstAvaliableIndex )
                stFirstAvaliableIndex = archive;
        }

        public static void UnloadAllArchives()
        {
            stLoadedArchives = new Archive[ 256 ];
            stFirstAvaliableIndex = 0;
        }

        public static void MountArchive( int archive )
        {
            if ( archive < 0 || archive >= 256 || stLoadedArchives[ archive ] == null )
                throw new ArchiveNotLoadedException( archive );

            Archive arch = stLoadedArchives[ archive ];

            foreach ( Type t in arch.Dictionaries.Keys )
            {
                Dictionary<String,Object> aDict = arch.Dictionaries[t];
                Dictionary<String,Object> mDict = stResourceDictionaries[t];

                foreach( String key in aDict.Keys )
                {
                    if ( !mDict.ContainsKey( key ) )
                        mDict.Add( key, aDict[ key ] );
                    else
                        mDict[ key ] = aDict[ key ];
                }
            }
        }

        public static void UnmountAllArchives()
        {
            foreach( Type key in stResourceDictionaries.Keys )
                stResourceDictionaries[ key ] = new Dictionary<string,object>();
        }

        public static int CreateFromDirectory( string directoryPath )
        {
            if ( stFirstAvaliableIndex >= 256 )
                throw new ArchiveNotLoadedException( 256 );

            //try
            {
                stLoadedArchives[ stFirstAvaliableIndex ] = Archive.CreateFromDirectory( directoryPath );
            }
            //catch ( Exception e )
            //{
            //    throw new Exception( "Resources failed to be created from directory \"" + directoryPath + "\"", e );
            //}

            int usedIndex = stFirstAvaliableIndex;

            IncrementIndex();

            return usedIndex;
        }

        public static void SaveArchiveToFile( int archive, string filePath )
        {
            if ( archive < 0 || archive >= 256 || stLoadedArchives[ archive ] == null )
                throw new ArchiveNotLoadedException( archive );

            stLoadedArchives[ archive ].SaveToFile( filePath );
        }

        public static String GetArchiveProperty( int archive, ArchiveProperty property )
        {
            if ( archive < 0 || archive >= 256 || stLoadedArchives[ archive ] == null )
                throw new ArchiveNotLoadedException( archive );

            Archive arch = stLoadedArchives[ archive ];

            switch( property )
            {
                case ArchiveProperty.Author:
                    return arch.Author;
                case ArchiveProperty.AuthorEmail:
                    return arch.AuthorEmail;
                case ArchiveProperty.AuthorWebsite:
                    return arch.AuthorWebsite;
                case ArchiveProperty.Description:
                    return arch.Description;
                case ArchiveProperty.Name:
                    return arch.Name;
                case ArchiveProperty.Version:
                    return arch.Version;
            }

            return "";
        }

        public static ArchiveDest GetArchiveDestination( int archive )
        {
            if ( archive < 0 || archive >= 256 || stLoadedArchives[ archive ] == null )
                throw new ArchiveNotLoadedException( archive );

            return stLoadedArchives[ archive ].Destination;
        }

        public static T Get<T>( String key, int archive = -1 )
        {
            if ( archive == -1 )
            {
                if ( !stResourceDictionaries[ typeof( T ) ].ContainsKey( key ) )
                    throw new ResourceNotFoundException( key );
                
                return (T) stResourceDictionaries[ typeof( T ) ][ key ];
            }
            
            if ( archive < 0 || archive >= 256 || stLoadedArchives[ archive ] == null )
                throw new ArchiveNotLoadedException( archive );

            return stLoadedArchives[ archive ].Get<T>( key );
        }

        public static T Get<T>( String key, T defaultValue, int archive = -1 )
        {
            if ( archive == -1 )
            {
                if ( !stResourceDictionaries[ typeof( T ) ].ContainsKey( key ) )
                    return defaultValue;

                return (T) stResourceDictionaries[ typeof( T ) ][ key ];
            }

            if ( archive < 0 || archive >= 256 || stLoadedArchives[ archive ] == null )
                throw new ArchiveNotLoadedException( archive );

            return stLoadedArchives[ archive ].Get<T>( key, defaultValue );
        }

        private static void IncrementIndex()
        {
            while ( stFirstAvaliableIndex != 256 && stLoadedArchives[ stFirstAvaliableIndex ] != null )
                ++stFirstAvaliableIndex;
        }
    }
}
