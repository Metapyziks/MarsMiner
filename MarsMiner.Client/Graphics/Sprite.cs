using System;

using OpenTK;
using OpenTK.Graphics;

namespace MarsMiner.Client.Graphics
{
    public class Sprite
    {
        internal float[] Vertices
        {
            get
            {
                return myVertices;
            }
        }

        private Texture myTexture;
        private float[] myVertices;

        private Vector2 myPosition;
        private Vector2 myScale;

        private Vector2 mySubrectOffset;
        private Vector2 mySubrectSize;

        private bool myFlipHorz;
        private bool myFlipVert;
        
        private float myRotation;
        private bool myUseCentreAsOrigin;
        private Color4 myColour;

        protected bool VertsChanged;
        
        public virtual Vector2 Position
        {
            get
            {
                return myPosition;
            }
            set
            {
                if ( value != myPosition )
                {
                    myPosition = value;
                    VertsChanged = true;
                }
            }
        }

        public virtual Vector2 Size
        {
            get
            {
                return new Vector2( mySubrectSize.X * Scale.X, mySubrectSize.Y * Scale.Y );
            }
            set
            {
                Scale = new Vector2( value.X / mySubrectSize.X, value.Y / mySubrectSize.Y );
            }
        }

        public virtual Vector2 Scale
        {
            get
            {
                return myScale;
            }
            set
            {
                if ( value != myScale )
                {
                    myScale = value;
                    VertsChanged = true;
                }
            }
        }

        public float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position = new Vector2( value, Y );
            }
        }
        public float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position = new Vector2( X, value );
            }
        }

        public virtual Vector2 SubrectOffset
        {
            get
            {
                return mySubrectOffset;
            }
            set
            {
                if ( value != mySubrectOffset )
                {
                    mySubrectOffset = value;
                    VertsChanged = true;
                }
            }
        }

        public virtual Vector2 SubrectSize
        {
            get
            {
                return mySubrectSize;
            }
            set
            {
                if ( value != mySubrectSize )
                {
                    mySubrectSize = value;
                    VertsChanged = true;
                }
            }
        }

        public float SubrectLeft
        {
            get
            {
                return SubrectOffset.X;
            }
            set
            {
                SubrectOffset = new Vector2( value, SubrectTop );
            }
        }

        public float SubrectTop
        {
            get
            {
                return SubrectOffset.Y;
            }
            set
            {
                SubrectOffset = new Vector2( SubrectLeft, value );
            }
        }

        public float SubrectRight
        {
            get
            {
                return SubrectOffset.X + SubrectSize.X;
            }
            set
            {
                SubrectSize = new Vector2( value - SubrectOffset.X, SubrectHeight );
            }
        }

        public float SubrectBottom
        {
            get
            {
                return SubrectOffset.Y + SubrectSize.Y;
            }
            set
            {
                SubrectSize = new Vector2( SubrectWidth, value - SubrectOffset.Y );
            }
        }

        public float SubrectWidth
        {
            get
            {
                return SubrectSize.X;
            }
            set
            {
                SubrectSize = new Vector2( value, SubrectHeight );
            }
        }

        public float SubrectHeight
        {
            get
            {
                return SubrectSize.Y;
            }
            set
            {
                SubrectSize = new Vector2( SubrectWidth, value );
            }
        }

        public float Width
        {
            get
            {
                return Size.X;
            }
            set
            {
                Scale = new Vector2( value / SubrectSize.X, Scale.Y );
            }
        }
        public float Height
        {
            get
            {
                return Size.Y;
            }
            set
            {
                Scale = new Vector2( Scale.X, value / SubrectSize.Y );
            }
        }

        public bool FlipHorizontal
        {
            get
            {
                return myFlipHorz;
            }
            set
            {
                if ( value != myFlipHorz )
                {
                    myFlipHorz = value;
                    VertsChanged = true;
                }
            }
        }

        public bool FlipVertical
        {
            get
            {
                return myFlipVert;
            }
            set
            {
                if ( value != myFlipVert )
                {
                    myFlipVert = value;
                    VertsChanged = true;
                }
            }
        }

        public float Rotation
        {
            get
            {
                return myRotation;
            }
            set
            {
                if ( value != myRotation )
                {
                    myRotation = value;
                    VertsChanged = true;
                }
            }
        }

        public bool UseCentreAsOrigin
        {
            get
            {
                return myUseCentreAsOrigin;
            }
            set
            {
                if ( value != myUseCentreAsOrigin )
                {
                    myUseCentreAsOrigin = value;
                    VertsChanged = true;
                }
            }
        }

        public Color4 Colour
        {
            get
            {
                return myColour;
            }
            set
            {
                if ( value != myColour )
                {
                    myColour = value;
                    VertsChanged = true;
                }
            }
        }

        public Texture Texture
        {
            get
            {
                return myTexture;
            }
        }

        public Sprite( float width, float height, Color4 colour )
        {
            myTexture = Texture.Blank;

            Position = new Vector2();
            Scale = new Vector2( width, height );
            SubrectOffset = new Vector2( 0, 0 );
            SubrectSize = new Vector2( myTexture.Width, myTexture.Height );
            FlipHorizontal = false;
            FlipVertical = false;
            Rotation = 0;
            UseCentreAsOrigin = false;
            Colour = colour;
        }

        public Sprite( Texture texture, float scale = 1.0f )
        {
            myTexture = texture;

            Position = new Vector2();
            Scale = new Vector2( 1, 1 );
            SubrectOffset = new Vector2( 0, 0 );
            SubrectSize = new Vector2( myTexture.Width, myTexture.Height );
            FlipHorizontal = false;
            FlipVertical = false;
            Rotation = 0;
            UseCentreAsOrigin = false;
            Colour = new Color4( 1.0f, 1.0f, 1.0f, 1.0f );

            Scale = new Vector2( scale, scale );
        }

        protected virtual float[] FindVerts()
        {
            Vector2 tMin = myTexture.GetCoords( SubrectLeft, SubrectTop );
            Vector2 tMax = myTexture.GetCoords( SubrectRight, SubrectBottom );
            float xMin = FlipHorizontal ? tMax.X : tMin.X;
            float yMin = FlipVertical ? tMax.Y : tMin.Y;
            float xMax = FlipHorizontal ? tMin.X : tMax.X;
            float yMax = FlipVertical ? tMin.Y : tMax.Y;

            float halfWid = Width / 2;
            float halfHei = Height / 2;

            float[,] verts = UseCentreAsOrigin ? new float[ , ]
            {
                { -halfWid, -halfHei },
                { +halfWid, -halfHei },
                { +halfWid, +halfHei },
                { -halfWid, +halfHei }
            } : new float[ , ]
            {
                { 0, 0 },
                { Width, 0 },
                { Width, Height },
                { 0, Height }
            };

            float[,] mat = new float[,]
            {
                { (float) Math.Cos( Rotation ), -(float) Math.Sin( Rotation ) },
                { (float) Math.Sin( Rotation ),  (float) Math.Cos( Rotation ) }
            };

            for ( int i = 0; i < 4; ++i )
            {
                float x = verts[ i, 0 ];
                float y = verts[ i, 1 ];
                verts[ i, 0 ] = X + mat[ 0, 0 ] * x + mat[ 0, 1 ] * y;
                verts[ i, 1 ] = Y + mat[ 1, 0 ] * x + mat[ 1, 1 ] * y;
            }

            return new float[]
            {
                verts[ 0, 0 ], verts[ 0, 1 ], xMin, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ 1, 0 ], verts[ 1, 1 ], xMax, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ 2, 0 ], verts[ 2, 1 ], xMax, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ 3, 0 ], verts[ 3, 1 ], xMin, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
            };
        }

        public virtual void Render( SpriteShader shader )
        {
            if ( VertsChanged )
            {
                myVertices = FindVerts();
                VertsChanged = false;
            }

            if ( !Texture.Ready || shader.Texture.ID != Texture.ID )
                shader.Texture = Texture;

            shader.Render( myVertices );
        }
    }
}
