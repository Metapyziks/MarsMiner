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
