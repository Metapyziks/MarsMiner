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

using OpenTK;
using OpenTK.Graphics;

using MarsMiner.Client.Graphics;

namespace MarsMiner.Client.UI
{
    public class UISprite : UIObject
    {
        private Sprite mySprite;

        public Color4 Colour
        {
            get
            {
                return mySprite.Colour;
            }
            set
            {
                mySprite.Colour = value;
            }
        }

        public UISprite( Sprite sprite )
            : this( sprite, new Vector2() )
        {
            
        }

        public UISprite( Sprite sprite, Vector2 position )
            : base( sprite.Size, position )
        {
            mySprite = sprite;
        }

        protected override Vector2 OnSetSize( Vector2 newSize )
        {
            mySprite.Size = newSize;

            return base.OnSetSize( newSize );
        }

        protected override bool CheckPositionWithinBounds( Vector2 pos )
        {
            return false;
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            mySprite.Position = renderPosition;

            mySprite.Render( shader );
        }
    }
}
