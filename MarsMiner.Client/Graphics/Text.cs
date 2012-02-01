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

using OpenTK;

using ResourceLib;

using MarsMiner.Shared;

namespace MarsMiner.Client.Graphics
{
    public class Font
    {
        private static Font stFontDefault;
        private static Font stFontLarge;

        public static Font Default
        {
            get
            {
                if ( stFontDefault == null )
                    stFontDefault = new Font( "images_gui_fontdefault" );
                return stFontDefault;
            }
        }
        public static Font Large
        {
            get
            {
                if ( stFontLarge == null )
                    stFontLarge = new Font( "images_gui_fontlarge" );
                return stFontLarge;
            }
        }

        internal readonly Texture2D Texture;

        public readonly Vector2 CharSize;

        public int CharWidth
        {
            get
            {
                return (int) CharSize.X;
            }
        }

        public int CharHeight
        {
            get
            {
                return (int) CharSize.Y;
            }
        }
        
        public Font( String charMap )
        {
            Texture = Res.Get<Texture2D>( charMap );

            CharSize = new Vector2( Texture.Width / 16, Texture.Height / 16 );
        }

        public Vector2 GetCharOffset( Char character )
        {
            int id = (int) character;

            return new Vector2( ( id % 16 ) * CharWidth, ( id / 16 ) * CharHeight );
        }
    }

    public class Text : Sprite
    {
        private String myText;
        private Font myFont;
        private float myWrapWidth;

        public Font Font
        {
            get
            {
                return myFont;
            }
        }

        public String String
        {
            get
            {
                return myText;
            }
            set
            {
                myText = value;
                VertsChanged = true;
            }
        }

        public float WrapWidth
        {
            get
            {
                return myWrapWidth;
            }
            set
            {
                myWrapWidth = value;
                VertsChanged = true;
            }
        }

        public Text()
            : this( Font.Default )
        {

        }

        public Text( Font font, float scale = 1.0f )
            : base( font.Texture, scale )
        {
            myText = "";
            myFont = font;
        }

        protected override float[] FindVerts()
        {
            String text = myText.ApplyWordWrap( Font.CharWidth * Scale.X, WrapWidth );

            int characters = text.Length;

            float[,] mat = new float[ , ]
            {
                { (float) Math.Cos( Rotation ) * Scale.X, -(float) Math.Sin( Rotation ) * Scale.Y },
                { (float) Math.Sin( Rotation ) * Scale.X,  (float) Math.Cos( Rotation ) * Scale.Y }
            };

            int quads = 0;

            for ( int i = 0; i < characters; ++i )
                if ( !char.IsWhiteSpace( text[ i ] ) )
                    ++quads;

            float[] verts = new float[ quads * 8 * 4 ];

            for ( int i = 0, index = 0, x = 0, y = 0; i < characters; ++i )
                GetCharVerts( text[ i ], verts, ref index, mat, ref x, ref y );

            return verts;
        }

        private void GetCharVerts( char character, float[] verts, ref int index, float[ , ] rotationMat, ref int x, ref int y )
        {
            if ( char.IsWhiteSpace( character ) )
            {
                if ( character == '\t' )
                    x += 4;
                else if ( character == '\n' )
                {
                    y += 1;
                    x = 0;
                }
                else
                    x += 1;

                return;
            }

            Vector2 subMin = myFont.GetCharOffset( character );

            Vector2 tMin = Texture.GetCoords( subMin.X, subMin.Y );
            Vector2 tMax = Texture.GetCoords( subMin.X + myFont.CharWidth, subMin.Y + myFont.CharHeight );
            float xMin = tMin.X;
            float yMin = tMin.Y;
            float xMax = tMax.X;
            float yMax = tMax.Y;

            float minX = x * myFont.CharWidth;
            float minY = y * myFont.CharHeight;

            float[,] pos = new float[ , ]
            {
                { minX, minY },
                { minX + myFont.CharWidth, minY },
                { minX + myFont.CharWidth, minY + myFont.CharHeight },
                { minX, minY + myFont.CharHeight }
            };

            for ( int i = 0; i < 4; ++i )
            {
                float xp = pos[ i, 0 ];
                float yp = pos[ i, 1 ];
                pos[ i, 0 ] = X + rotationMat[ 0, 0 ] * xp + rotationMat[ 0, 1 ] * yp;
                pos[ i, 1 ] = Y + rotationMat[ 1, 0 ] * xp + rotationMat[ 1, 1 ] * yp;
            }

            Array.Copy( new float[]
            {
                pos[ 0, 0 ], pos[ 0, 1 ], xMin, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                pos[ 1, 0 ], pos[ 1, 1 ], xMax, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                pos[ 2, 0 ], pos[ 2, 1 ], xMax, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
                pos[ 3, 0 ], pos[ 3, 1 ], xMin, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
            }, 0, verts, index, 8 * 4 );

            index += 8 * 4;
            x += 1;
        }
    }
}
