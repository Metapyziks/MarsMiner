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

using MarsMiner.Client.Graphics;

namespace MarsMiner.Client.UI
{
    public class UITextBox : UIObject
    {
        private Font myFont;
        private FrameSprite mySprite;
        private UILabel myText;
        private Sprite myUnderlineChar;

        private DateTime myLastFlashTime;

        public int CharacterLimit;

        public String Text
        {
            get
            {
                return myText.Text;
            }
            set
            {
                if ( value.Length <= CharacterLimit )
                    myText.Text = value;
                else
                    myText.Text = value.Substring( 0, CharacterLimit );
            }
        }

        public UITextBox( float scale = 1.0f )
            : this( new Vector2(), new Vector2(), scale )
        {

        }

        public UITextBox( Vector2 size, float scale = 1.0f )
            : this( size, new Vector2(), scale )
        {

        }

        public UITextBox( Vector2 size, Vector2 position, float scale = 1.0f )
            : base( size, position )
        {
            PaddingLeft = PaddingTop = PaddingRight = PaddingBottom = 4.0f * scale;

            mySprite = new FrameSprite( Res.Get<Texture>( "images_gui_panels" ), scale )
            {
                SubrectSize = new Vector2( 16, 16 ),
                SubrectOffset = new Vector2( 0, 32 ),
                FrameTopLeftOffet = new Vector2( 4, 4 ),
                FrameBottomRightOffet = new Vector2( 4, 4 ),
                Size = size
            };
            
            myFont = Font.Large;
            myText = new UILabel( myFont, scale );
            AddChild( myText );

            CharacterLimit = (int)( InnerWidth / ( myFont.CharWidth * scale ) );

            myUnderlineChar = new Sprite( scale * myFont.CharWidth, scale * 2.0f, OpenTK.Graphics.Color4.Black );
        }

        protected override void OnFocus()
        {
            myLastFlashTime = DateTime.Now;
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            char c = e.KeyChar;

            if ( c == 8 )
            {
                if ( Text.Length > 0 )
                    Text = Text.Substring( 0, Text.Length - 1 );
            }
            else if ( c == 13 || c == 27 )
                UnFocus();
            else if ( Char.IsLetterOrDigit( c ) || Char.IsPunctuation( c ) || Char.IsSymbol( c ) || c == ' ' )
                Text += c;
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            mySprite.Position = renderPosition;
            mySprite.Colour = ( IsEnabled ? OpenTK.Graphics.Color4.White : DisabledColour );
            mySprite.Render( shader );

            double timeSinceFlash = ( DateTime.Now - myLastFlashTime ).TotalSeconds;

            if ( timeSinceFlash < 0.5 && IsFocused && IsEnabled && Text.Length < CharacterLimit )
            {
                myUnderlineChar.Position = renderPosition + new Vector2( PaddingLeft + myText.Width, PaddingTop + Math.Max( myText.Height, myFont.CharHeight ) - myUnderlineChar.Height );
                myUnderlineChar.Render( shader );
            }
            else if( timeSinceFlash >= 1.0 )
                myLastFlashTime = DateTime.Now;
        }
    }
}
