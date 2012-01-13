using System.IO;

using ResourceLib;

namespace MarsMiner
{
    class Program
    {
        static void Main( string[] args )
        {
            Res.RegisterManager( new MarsMiner.Client.Graphics.RTextureManager() );

            Res.MountArchive( Res.LoadArchive( "Data" + Path.DirectorySeparatorChar + "cl_baseui.rsa" ) );

            var window = new MarsMinerWindow();
            window.Run( 60.0f );
            window.Dispose();
        }
    }
}
