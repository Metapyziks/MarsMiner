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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using ResourceLib;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    public class RTextureManager : RManager
    {
        public RTextureManager()
            : base( typeof( Texture ), 2, "png" )
        {

        }

        public override ResourceItem[] LoadFromFile( String keyPrefix, String fileName, String fileExtension, FileStream stream )
        {
            return new ResourceItem[] { new ResourceItem( keyPrefix + fileName, new Texture( new Bitmap( stream ) ) ) };
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

            return new Texture( bmp );
        }

        public override void SaveToArchive( BinaryWriter stream, Object item )
        {
            Texture tex = item as Texture;
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
        public static readonly Texture Blank;

        static Texture()
        {
            Bitmap blankBmp = new Bitmap( 1, 1 );
            blankBmp.SetPixel( 0, 0, Color.White );
            Blank = new Texture( blankBmp );
        }

        private static int GetNextPOTS( int wid, int hei )
        {
            int max = wid > hei ? wid : hei;

            return (int) Math.Pow( 2.0, Math.Ceiling( Math.Log( max, 2.0 ) ) );
        }

        private static int stCurrentLoadedTexture = -1;

        public static int Current
        {
            get
            {
                return stCurrentLoadedTexture;
            }
        }

        private int myID;
        private Bitmap myBitmap;
        private int myWidth;
        private int myHeight;
        private bool myLoaded;

        public bool Ready
        {
            get
            {
                return myID != 0;
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

        public int Width
        {
            get
            {
                return myWidth;
            }
        }

        public int Height
        {
            get
            {
                return myHeight;
            }
        }

        public Bitmap Bitmap
        {
            get
            {
                return myBitmap;
            }
        }

        public Texture( Bitmap bitmap )
        {
            myWidth = bitmap.Width;
            myHeight = bitmap.Height;

            int size = GetNextPOTS( bitmap.Width, bitmap.Height );

            if ( size == bitmap.Width && size == bitmap.Height )
                myBitmap = bitmap;
            else
            {
                myBitmap = new Bitmap( size, size );

                for ( int x = 0; x < myWidth; ++x )
                    for ( int y = 0; y < myHeight; ++y )
                        myBitmap.SetPixel( x, y, bitmap.GetPixel( x, y ) );
            }

            myLoaded = false;
        }

        public Texture( string resourceKey )
            : this( Res.Get<Bitmap>( resourceKey ) )
        {
           
        }

        public Vector2 GetCoords( Vector2 pos )
        {
            return GetCoords( pos.X, pos.Y );
        }

        public Vector2 GetCoords( float x, float y )
        {
            return new Vector2
            {
                X = x / myBitmap.Width,
                Y = y / myBitmap.Height
            };
        }

        public Color GetPixel( int x, int y )
        {
            return myBitmap.GetPixel( x, y );
        }

        public void SetPixel( int x, int y, Color colour )
        {
            if ( this == Blank )
                return;

            myBitmap.SetPixel( x, y, colour );
            myLoaded = false;
        }

        private void Use()
        {
            GL.TexEnv( TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float) TextureEnvMode.Modulate );

            BitmapData data = myBitmap.LockBits( new Rectangle( 0, 0, myBitmap.Width, myBitmap.Height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, myBitmap.Width, myBitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );
            //GL.GenerateMipmap( GenerateMipmapTarget.Texture2D );

            myBitmap.UnlockBits( data );

            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) TextureMinFilter.Nearest );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) TextureMagFilter.Nearest );

            myLoaded = true;
        }

        public void Bind()
        {
            if ( stCurrentLoadedTexture != ID )
            {
                GL.BindTexture( TextureTarget.Texture2D, ID );
                stCurrentLoadedTexture = ID;
            }

            if ( !myLoaded )
                Use();
        }
    }
}
