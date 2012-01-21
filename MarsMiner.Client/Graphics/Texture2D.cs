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

using System.Drawing;
using System.Drawing.Imaging;

using ResourceLib;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    public class Texture2D : Texture
    {
        public static readonly Texture2D Blank;

        static Texture2D()
        {
            Bitmap blankBmp = new Bitmap( 1, 1 );
            blankBmp.SetPixel( 0, 0, Color.White );
            Blank = new Texture2D( blankBmp );
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Bitmap Bitmap { get; private set; }

        public Texture2D( Bitmap bitmap )
            : base( TextureTarget.Texture2D )
        {
            Width = bitmap.Width;
            Height = bitmap.Height;

            int size = GetNextPOTS( bitmap.Width, bitmap.Height );

            if ( size == bitmap.Width && size == bitmap.Height )
                Bitmap = bitmap;
            else
            {
                Bitmap = new Bitmap( size, size );

                for ( int x = 0; x < Width; ++x )
                    for ( int y = 0; y < Height; ++y )
                        Bitmap.SetPixel( x, y, bitmap.GetPixel( x, y ) );
            }
        }

        public Texture2D( string resourceKey )
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
                X = x / Bitmap.Width,
                Y = y / Bitmap.Height
            };
        }

        public Color GetPixel( int x, int y )
        {
            return Bitmap.GetPixel( x, y );
        }

        public void SetPixel( int x, int y, Color colour )
        {
            if ( this == Blank )
                return;

            Bitmap.SetPixel( x, y, colour );
            Update();
        }

        protected override void Load()
        {
            GL.TexEnv( TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float) TextureEnvMode.Modulate );

            BitmapData data = Bitmap.LockBits( new Rectangle( 0, 0, Bitmap.Width, Bitmap.Height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );

            Bitmap.UnlockBits( data );

            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) TextureMinFilter.Nearest );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) TextureMagFilter.Nearest );
        }
    }
}
