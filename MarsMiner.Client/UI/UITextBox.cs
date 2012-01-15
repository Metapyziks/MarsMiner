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
