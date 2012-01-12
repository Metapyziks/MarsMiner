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
