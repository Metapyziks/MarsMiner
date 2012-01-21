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
using OpenTK.Graphics;

namespace MarsMiner.Client.UI
{
    public class UIButton : UIObject
    {
        private FrameSprite myButtonSprite;
        private UILabel myLabel;
        private bool myCentreText;

        public Color4 Colour;

        public String Text
        {
            get
            {
                return myLabel.Text;
            }
            set
            {
                myLabel.Text = value;
                RepositionText();
            }
        }

        public bool CentreText
        {
            get
            {
                return myCentreText;
            }
            set
            {
                myCentreText = value;
                RepositionText();
            }
        }

        public UIButton( float scale = 1.0f )
            : this( new Vector2(), new Vector2(), scale )
        {

        }

        public UIButton( Vector2 size, float scale = 1.0f )
            : this( size, new Vector2(), scale )
        {

        }

        public UIButton( Vector2 size, Vector2 position, float scale = 1.0f )
            : base( size, position )
        {
            Colour = Color4.White;

            PaddingLeft = PaddingTop = PaddingRight = PaddingBottom = 4.0f * scale;

            myButtonSprite = new FrameSprite( Res.Get<Texture2D>( "images_gui_panels" ), scale )
            {
                SubrectSize = new Vector2( 16, 16 ),
                SubrectOffset = new Vector2( 32, 16 ),
                FrameTopLeftOffet = new Vector2( 4, 4 ),
                FrameBottomRightOffet = new Vector2( 4, 4 ),
                Size = size
            };

            myLabel = new UILabel( Font.Large, scale );
            
            AddChild( myLabel );
        }

        private void RepositionText()
        {
            myLabel.Top = ( InnerHeight - myLabel.Height ) / 2.0f;

            if ( CentreText )
                myLabel.Left = ( InnerWidth - myLabel.Width ) / 2.0f;
            else
                myLabel.Left = 0.0f;
        }

        protected override void OnMouseEnter( Vector2 mousePos )
        {
            if ( IsVisible && IsEnabled )
                myButtonSprite.SubrectLeft = 48.0f;
        }

        protected override void OnMouseLeave( Vector2 mousePos )
        {
            if ( IsVisible && IsEnabled )
                myButtonSprite.SubrectLeft = 32.0f;
        }

        protected override void OnDisable()
        {
            myButtonSprite.SubrectLeft = 32.0f;
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            myButtonSprite.Position = renderPosition;
            myButtonSprite.Colour = ( IsEnabled ? Colour : DisabledColour );
            myButtonSprite.Render( shader );
        }
    }
}
