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
