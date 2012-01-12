using System;

using OpenTK;

using ResourceLib;

using MarsMiner.Client.Graphics;
using OpenTK.Graphics;

namespace MarsMiner.Client.UI
{
    public class UIButton : UIObject
    {
        private FrameSprite myButtonSprite;
        private UILabel myLabel;
        private bool myCentreText;

        public Color4 Colour;

        public String Text
        {
            get
            {
                return myLabel.Text;
            }
            set
            {
                myLabel.Text = value;
                RepositionText();
            }
        }

        public bool CentreText
        {
            get
            {
                return myCentreText;
            }
            set
            {
                myCentreText = value;
                RepositionText();
            }
        }

        public UIButton( float scale = 1.0f )
            : this( new Vector2(), new Vector2(), scale )
        {

        }

        public UIButton( Vector2 size, float scale = 1.0f )
            : this( size, new Vector2(), scale )
        {

        }

        public UIButton( Vector2 size, Vector2 position, float scale = 1.0f )
            : base( size, position )
        {
            Colour = Color4.White;

            PaddingLeft = PaddingTop = PaddingRight = PaddingBottom = 4.0f * scale;

            myButtonSprite = new FrameSprite( Res.Get<Texture>( "images_gui_panels" ), scale )
            {
                SubrectSize = new Vector2( 16, 16 ),
                SubrectOffset = new Vector2( 32, 16 ),
                FrameTopLeftOffet = new Vector2( 4, 4 ),
                FrameBottomRightOffet = new Vector2( 4, 4 ),
                Size = size
            };

            myLabel = new UILabel( Font.Large, scale );
            
            AddChild( myLabel );
        }

        private void RepositionText()
        {
            myLabel.Top = ( InnerHeight - myLabel.Height ) / 2.0f;

            if ( CentreText )
                myLabel.Left = ( InnerWidth - myLabel.Width ) / 2.0f;
            else
                myLabel.Left = 0.0f;
        }

        protected override void OnMouseEnter( Vector2 mousePos )
        {
            if ( IsVisible && IsEnabled )
                myButtonSprite.SubrectLeft = 48.0f;
        }

        protected override void OnMouseLeave( Vector2 mousePos )
        {
            if ( IsVisible && IsEnabled )
                myButtonSprite.SubrectLeft = 32.0f;
        }

        protected override void OnDisable()
        {
            myButtonSprite.SubrectLeft = 32.0f;
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            myButtonSprite.Position = renderPosition;
            myButtonSprite.Colour = ( IsEnabled ? Colour : DisabledColour );
            myButtonSprite.Render( shader );
        }
    }
}
