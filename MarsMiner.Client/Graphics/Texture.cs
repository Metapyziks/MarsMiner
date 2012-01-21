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
using System.Drawing;
using System.IO;

using ResourceLib;

using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    public class RTexture2DManager : RManager
    {
        public RTexture2DManager()
            : base( typeof(Texture2D), 2, "png" )
        {

        }

        public override ResourceItem[] LoadFromFile( String keyPrefix, String fileName, String fileExtension, FileStream stream )
        {
            return new ResourceItem[] { new ResourceItem( keyPrefix + fileName, new Texture2D( new Bitmap( stream ) ) ) };
        }

        public override Object LoadFromArchive( BinaryReader stream )
        {
            ushort wid = stream.ReadUInt16();
            ushort hei = stream.ReadUInt16();

            Bitmap bmp = new Bitmap( wid, hei );

            for ( int x = 0; x < wid; ++x )
                for ( int y = 0; y < hei; ++y )
                {
                    bmp.SetPixel( x, y, Color.FromArgb(
                        stream.ReadByte(),
                        stream.ReadByte(),
                        stream.ReadByte(),
                        stream.ReadByte()
                    ) );
                }

            return new Texture2D( bmp );
        }

        public override void SaveToArchive( BinaryWriter stream, Object item )
        {
            Texture2D tex = (Texture2D) item;
            Bitmap bmp = tex.Bitmap;

            ushort wid = (ushort) tex.Width;
            ushort hei = (ushort) tex.Height;

            stream.Write( wid );
            stream.Write( hei );

            for ( int x = 0; x < wid; ++x )
                for ( int y = 0; y < hei; ++y )
                {
                    Color pix = bmp.GetPixel( x, y );
                    stream.Write( pix.A );
                    stream.Write( pix.R );
                    stream.Write( pix.G );
                    stream.Write( pix.B );
                }
        }
    }

    public class Texture
    {
        protected static int GetNextPOTS( int wid, int hei )
        {
            int max = wid > hei ? wid : hei;

            return (int) Math.Pow( 2.0, Math.Ceiling( Math.Log( max, 2.0 ) ) );
        }

        private static Texture stCurrentLoadedTexture;

        public static Texture Current
        {
            get
            {
                return stCurrentLoadedTexture;
            }
        }

        private int myID;
        private bool myLoaded;

        public TextureTarget TextureTarget { get; private set; }

        public bool Ready
        {
            get
            {
                return myID > -1;
            }
        }

        public int ID
        {
            get
            {
                if ( !Ready )
                    GL.GenTextures( 1, out myID );

                return myID;
            }
        }

        public Texture( TextureTarget target )
        {
            TextureTarget = target;

            myID = -1;
            myLoaded = false;
        }

        public void Update()
        {
            myLoaded = false;
        }

        protected virtual void Load()
        {

        }

        public void Bind()
        {
            if ( stCurrentLoadedTexture != this )
            {
                GL.BindTexture( TextureTarget, ID );
                stCurrentLoadedTexture = this;
            }

            if ( !myLoaded )
            {
                Load();
                myLoaded = true;
            }
        }
    }
}
