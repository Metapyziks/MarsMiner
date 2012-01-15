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

using MarsMiner.Client.Graphics;

namespace MarsMiner.Client.UI
{
    public class UIPanel : UIObject
    {
        private UISprite myBackSprite;

        public OpenTK.Graphics.Color4 Colour
        {
            get
            {
                return myBackSprite.Colour;
            }
            set
            {
                myBackSprite.Colour = value;
            }
        }

        public UIPanel()
            : this( new Vector2(), new Vector2() )
        {

        }

        public UIPanel( Vector2 size )
            : this( size, new Vector2() )
        {

        }

        public UIPanel( Vector2 size, Vector2 position )
            : base( size, position )
        {
            myBackSprite = new UISprite( new Sprite( size.X, size.Y, OpenTK.Graphics.Color4.White ) );
            AddChild( myBackSprite );
        }

        protected override Vector2 OnSetSize( Vector2 newSize )
        {
            myBackSprite.SetSize( newSize );

            return base.OnSetSize( newSize );
        }
    }
}
