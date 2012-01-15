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

namespace MarsMiner.Client.UI
{
    public class UIMessageBox : UIWindow
    {
        private UILabel myText;
        private bool myCentreText;

        public bool CentreText
        {
            get
            {
                return myCentreText;
            }
            set
            {
                myCentreText = value;
                if ( myCentreText )
                    myText.Centre();
                else
                    myText.Position = new Vector2( 4, 4 );
            }
        }

        public String Text
        {
            get
            {
                return myText.Text;
            }
            set
            {
                myText.Text = value;
                if( CentreText )
                    myText.Centre();
            }
        }

        public UIMessageBox( String message, String title, bool closeButton = true )
            : base( new Vector2( 480, 64 ) )
        {
            CanClose = closeButton;
            myCentreText = false;

            Title = title;

            myText = new UILabel( Client.Graphics.Font.Large )
            {
                Text = message,
                Position = new Vector2( 4, 4 )
            };
            AddChild( myText );
        }
    }
}
