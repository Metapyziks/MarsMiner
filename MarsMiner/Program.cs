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

namespace MarsMiner
{
    class Program
    {
        static void Main( string[] args )
        {
            Res.RegisterManager( new MarsMiner.Client.Graphics.RTextureManager() );

            String contentDir = "Data" + Path.DirectorySeparatorChar;

            Res.MountArchive( Res.LoadArchive( contentDir + "cl_baseui.rsa" ) );
            Res.MountArchive( Res.LoadArchive( contentDir + "cl_marsminer.rsa" ) );

            var window = new MarsMinerWindow();
            window.Run( 60.0f );
            window.Dispose();
        }
    }
}
