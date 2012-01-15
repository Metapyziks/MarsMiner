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
