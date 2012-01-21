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

namespace MarsMiner.Client.Graphics
{
    public class FrameSprite : Sprite
    {
        private Vector2 myFrameSize;
        private Vector2 myFrameTopLeft;
        private Vector2 myFrameBottomRight;

        public override Vector2 Size
        {
            get
            {
                return myFrameSize;
            }
            set
            {
                myFrameSize = value;
            }
        }

        public Vector2 FrameTopLeftOffet
        {
            get
            {
                return myFrameTopLeft;
            }
            set
            {
                myFrameTopLeft = value;
            }
        }

        public float FrameLeftOffet
        {
            get
            {
                return myFrameTopLeft.X;
            }
            set
            {
                myFrameTopLeft.X = value;
            }
        }

        public float FrameTopOffet
        {
            get
            {
                return myFrameTopLeft.Y;
            }
            set
            {
                myFrameTopLeft.Y = value;
            }
        }

        public Vector2 FrameBottomRightOffet
        {
            get
            {
                return myFrameBottomRight;
            }
            set
            {
                myFrameBottomRight = value;
            }
        }

        public float FrameRightOffet
        {
            get
            {
                return myFrameBottomRight.X;
            }
            set
            {
                myFrameBottomRight.X = value;
            }
        }

        public float FrameBottomOffet
        {
            get
            {
                return myFrameBottomRight.Y;
            }
            set
            {
                myFrameBottomRight.Y = value;
            }
        }

        public FrameSprite( Texture2D texture, float scale = 1.0f )
            : base( texture, scale )
        {
            myFrameTopLeft = new Vector2();
            myFrameBottomRight = new Vector2();
        }

        protected override float[] FindVerts()
        {
            Vector2 tMin = Texture.GetCoords( SubrectLeft, SubrectTop );
            Vector2 tMax = Texture.GetCoords( SubrectRight, SubrectBottom );
            Vector2 fMin = Texture.GetCoords( SubrectLeft + FrameLeftOffet, SubrectTop + FrameTopOffet );
            Vector2 fMax = Texture.GetCoords( SubrectRight - FrameRightOffet, SubrectBottom - FrameBottomOffet );

            float xtMin = FlipHorizontal ? tMax.X : tMin.X;
            float ytMin = FlipVertical ? tMax.Y : tMin.Y;
            float xtMax = FlipHorizontal ? tMin.X : tMax.X;
            float ytMax = FlipVertical ? tMin.Y : tMax.Y;
            float xfMin = FlipHorizontal ? fMax.X : fMin.X;
            float yfMin = FlipVertical ? fMax.Y : fMin.Y;
            float xfMax = FlipHorizontal ? fMin.X : fMax.X;
            float yfMax = FlipVertical ? fMin.Y : fMax.Y;

            float[,] verts = new float[ , ]
            {
                { 0, 0 },
                { FrameLeftOffet * Scale.X, 0 },
                { FrameLeftOffet * Scale.X, FrameTopOffet * Scale.Y },
                { 0, FrameTopOffet * Scale.Y },

                { FrameLeftOffet * Scale.X, 0 },
                { Width - FrameRightOffet * Scale.X, 0 },
                { Width - FrameRightOffet * Scale.X, FrameTopOffet * Scale.Y },
                { FrameLeftOffet * Scale.X, FrameTopOffet * Scale.Y },

                { Width - FrameRightOffet * Scale.X, 0 },
                { Width, 0 },
                { Width, FrameTopOffet * Scale.Y },
                { Width - FrameRightOffet * Scale.X, FrameTopOffet * Scale.Y },

                //

                { 0, FrameTopOffet * Scale.Y },
                { FrameLeftOffet * Scale.X, FrameTopOffet * Scale.Y },
                { FrameLeftOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },
                { 0, Height - FrameBottomOffet * Scale.Y },

                { FrameLeftOffet * Scale.X, FrameTopOffet * Scale.Y },
                { Width - FrameRightOffet * Scale.X, FrameTopOffet * Scale.Y },
                { Width - FrameRightOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },
                { FrameLeftOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },

                { Width - FrameRightOffet * Scale.X, FrameTopOffet * Scale.Y },
                { Width, FrameTopOffet * Scale.Y },
                { Width, Height - FrameBottomOffet * Scale.Y },
                { Width - FrameRightOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },

                //

                { 0, Height - FrameBottomOffet * Scale.Y },
                { FrameLeftOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },
                { FrameLeftOffet * Scale.X, Height },
                { 0, Height },

                { FrameLeftOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },
                { Width - FrameRightOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },
                { Width - FrameRightOffet * Scale.X, Height },
                { FrameLeftOffet * Scale.X, Height },

                { Width - FrameRightOffet * Scale.X, Height - FrameBottomOffet * Scale.Y },
                { Width, Height - FrameBottomOffet * Scale.Y },
                { Width, Height },
                { Width - FrameRightOffet * Scale.X, Height }
            };

            float[,] mat = new float[ , ]
            {
                { (float) Math.Cos( Rotation ), -(float) Math.Sin( Rotation ) },
                { (float) Math.Sin( Rotation ),  (float) Math.Cos( Rotation ) }
            };

            for ( int i = 0; i < 4 * 9; ++i )
            {
                float x = verts[ i, 0 ];
                float y = verts[ i, 1 ];
                verts[ i, 0 ] = X + mat[ 0, 0 ] * x + mat[ 0, 1 ] * y;
                verts[ i, 1 ] = Y + mat[ 1, 0 ] * x + mat[ 1, 1 ] * y;
            }

            int v = 0;

            return new float[]
            {
                verts[ v, 0 ], verts[ v ++, 1 ], xtMin, ytMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, ytMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMin, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, ytMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, ytMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, ytMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMax, ytMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMax, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,

                //
                
                verts[ v, 0 ], verts[ v ++, 1 ], xtMin, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMin, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMax, yfMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMax, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,

                //
                
                verts[ v, 0 ], verts[ v ++, 1 ], xtMin, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, ytMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMin, ytMax, Colour.R, Colour.G, Colour.B, Colour.A,
                
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, ytMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMin, ytMax, Colour.R, Colour.G, Colour.B, Colour.A,
                
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMax, yfMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xtMax, ytMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ v, 0 ], verts[ v ++, 1 ], xfMax, ytMax, Colour.R, Colour.G, Colour.B, Colour.A,
            };
        }
    }
}
