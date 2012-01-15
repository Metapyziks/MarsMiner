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
