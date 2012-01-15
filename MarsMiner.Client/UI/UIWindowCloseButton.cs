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

using ResourceLib;

using OpenTK;

using MarsMiner.Client.Graphics;

namespace MarsMiner.Client.UI
{
    public class UIWindowCloseButton : UIObject
    {
        private Sprite mySprite;

        public UIWindowCloseButton( float scale = 1.0f )
            : this( new Vector2(), scale )
        {
        
        }

        public UIWindowCloseButton( Vector2 position, float scale = 1.0f )
            : base( new Vector2(), position )
        {
            Texture texture = Res.Get<Texture>( "images_gui_panels" );
            mySprite = new Sprite( texture, scale )
            {
                SubrectOffset = new Vector2( 32, 0 ),
                SubrectSize = new Vector2( 16, 16 )
            };

            SetSize( 16.0f * scale, 16.0f * scale );

            CanResize = false;
        }

        protected override void OnMouseEnter( Vector2 mousePos )
        {
            mySprite.SubrectLeft = 48.0f;
        }

        protected override void OnMouseLeave( Vector2 mousePos )
        {
            mySprite.SubrectLeft = 32.0f;
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            mySprite.Position = renderPosition;

            mySprite.Render( shader );
        }
    }
}
