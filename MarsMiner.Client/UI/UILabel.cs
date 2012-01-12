using System;

using OpenTK;
using OpenTK.Graphics;

using MarsMiner.Shared;
using MarsMiner.Client.Graphics;

namespace MarsMiner.Client.UI
{
    public class UILabel : UIObject
    {
        private Font myFont;
        private Text myText;

        public String Text
        {
            get
            {
                return myText.String;
            }
            set
            {
                myText.String = value;
                FindSize();
            }
        }

        public Font Font
        {
            get
            {
                return myFont;
            }
        }

        public Color4 Colour
        {
            get
            {
                return myText.Colour;
            }
            set
            {
                myText.Colour = value;
            }
        }

        public float WrapWidth
        {
            get
            {
                return myText.WrapWidth;
            }
            set
            {
                myText.WrapWidth = value;
                FindSize();
            }
        }

        public UILabel( Font font, float scale = 1.0f )
            : this( font, new Vector2(), scale )
        {
            
        }

        public UILabel( Font font, Vector2 position, float scale = 1.0f )
            : base( new Vector2(), position )
        {
            myFont = font;
            myText = new Text( font, scale );
            Colour = Color4.Black;
            CanResize = false;
            IsEnabled = false;
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            myText.Position = renderPosition;

            myText.Render( shader );
        }

        private void FindSize()
        {
            String[] lines = myText.String.ApplyWordWrap( Font.CharWidth * myText.Scale.X, myText.WrapWidth ).Split( '\n' );

            int maxLength = 0;

            foreach ( String line in lines )
                if ( line.Length > maxLength )
                    maxLength = line.Length;

            float width = myFont.CharWidth * myText.Scale.X * maxLength;
            float height = myFont.CharHeight * myText.Scale.Y * lines.Length;

            CanResize = true;
            SetSize( width, height );
            CanResize = false;
        }
    }
}
