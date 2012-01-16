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
