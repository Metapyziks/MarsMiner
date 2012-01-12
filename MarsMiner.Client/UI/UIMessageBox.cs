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
