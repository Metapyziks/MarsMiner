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
                Res.RegisterManager( new MarsMiner.Client.Graphics.RTextureManager() );
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
