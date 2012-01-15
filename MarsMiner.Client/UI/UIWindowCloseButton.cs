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
