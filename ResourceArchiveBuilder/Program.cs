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
using System.IO;

using ResourceLib;

namespace ResourceArchiveBuilder
{
    class Program
    {
        private static bool first = true;

        static void Main( string[] args )
        {
            if ( first )
            {
                Res.RegisterManager( new MarsMiner.Client.Graphics.RTexture2DManager() );
                first = false;
            }

            if ( args.Length > 1 )
            {
                foreach ( String str in args )
                    Main( new String[] { str } );

                return;
            }

            String directoryPath = "";

            if ( args.Length > 0 )
                directoryPath = args[ 0 ];
            else
            {
                do
                {
                    Console.WriteLine( "Directory path to create archive from:" );
                    directoryPath = Console.ReadLine();

                    if ( !Directory.Exists( directoryPath ) )
                    {
                        Console.WriteLine( "Directory does not exist!" );
                        directoryPath = "";
                    }
                }
                while ( directoryPath == "" );
            }

            Console.WriteLine( "Creating archive..." );

            int archive = Res.CreateFromDirectory( directoryPath );

            ArchiveDest dest = Res.GetArchiveDestination( archive );

            Console.WriteLine( ( dest == ArchiveDest.Client ? "Client" : dest == ArchiveDest.Server ? "Server" : "Shared" ) + " archive \"" + Res.GetArchiveProperty( archive, ArchiveProperty.Name ) + "\" created." );

            directoryPath = "";

            if ( args.Length > 0 )
                directoryPath = Directory.GetCurrentDirectory() + "\\";
            else
            {
                do
                {
                    Console.WriteLine( "Directory path to save archive to:" );
                    directoryPath = Console.ReadLine();

                    if ( directoryPath.Length < 2 || directoryPath[ 1 ] != ':' )
                        directoryPath = Directory.GetCurrentDirectory() + ( directoryPath.StartsWith( "\\" ) || directoryPath.StartsWith( "/" ) ? "" : "\\" ) + directoryPath;

                    if ( !directoryPath.EndsWith( "\\" ) && !directoryPath.EndsWith( "/" ) )
                        directoryPath += "\\";

                    if ( !Directory.Exists( directoryPath ) )
                    {
                        Console.WriteLine( "Directory does not exist!" );
                        Console.WriteLine( "Create directory \"" + directoryPath + "\"? [y/n]" );

                        if ( Console.ReadLine().ToLower()[ 0 ] == 'y' )
                        {
                            Directory.CreateDirectory( directoryPath );
                            break;
                        }

                        directoryPath = "";
                    }
                }
                while ( directoryPath == "" );
            }

            Console.WriteLine( "Compressing..." );
            Res.SaveArchiveToFile( archive, directoryPath + ( dest == ArchiveDest.Server ? "sv_" : dest == ArchiveDest.Client ? "cl_" : "sh_" ) + Res.GetArchiveProperty( archive, ArchiveProperty.Name ).ToLower().Replace( " ", "" ) + Res.DefaultResourceExtension );

            if ( args.Length == 0 )
            {
                Console.WriteLine( "Archive saved. Press any key to exit..." );
                Console.ReadKey();
            }
        }
    }
}
