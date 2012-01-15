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
using System.Collections.Generic;

using OpenTK;

namespace MarsMiner.Client.UI
{
    public class UIMenu : UIWindow
    {
        private List<UIButton> myButtons;

        public int ButtonHeight;
        public int ButtonSpacing;
        public int ButtonMargin;

        public UIMenu( Vector2 size, float scale = 1.0f )
            : base( size, scale )
        {
            Title = "Menu";

            ButtonHeight = 32;
            ButtonSpacing = 8;
            ButtonMargin = 8;

            myButtons = new List<UIButton>();
        }

        public UIButton CreateButton( String text, MouseButtonEventHandler clickHandler = null )
        {
            return InsertButton( myButtons.Count, text, clickHandler );
        }

        public UIButton InsertButton( int index, String text, MouseButtonEventHandler clickHandler = null )
        {
            float y = ButtonMargin;
            if ( myButtons.Count != 0 && index != 0 )
                y = myButtons[ index - 1 ].Bottom + ButtonSpacing;

            for ( int i = index; i < myButtons.Count; ++i )
                myButtons[ i ].Top += myButtons[ i ].Height + ButtonSpacing;

            UIButton newButton = new UIButton( new Vector2( InnerWidth - ButtonMargin * 2, ButtonHeight ), new Vector2( ButtonMargin, y ) )
            {
                Text = text,
                CentreText = true
            };
            myButtons.Insert( index, newButton );
            AddChild( newButton );

            if ( clickHandler != null )
                newButton.Click += clickHandler;

            return newButton;
        }

        public void RemoveButton( UIButton button )
        {
            RemoveButton( myButtons.IndexOf( button ) );
        }

        public void RemoveButton( int index )
        {
            UIButton button = myButtons[ index ];
            myButtons.RemoveAt( index );
            RemoveChild( button );

            for ( int i = index; i < myButtons.Count; ++i )
                myButtons[ i ].Top -= myButtons[ i ].Height + ButtonSpacing;
        }

        public void AutoSize()
        {
            Height = myButtons.Count * ( ButtonHeight + ButtonSpacing ) - ButtonSpacing +
                ButtonMargin * 2 + PaddingTop + PaddingBottom;
        }
    }
}
