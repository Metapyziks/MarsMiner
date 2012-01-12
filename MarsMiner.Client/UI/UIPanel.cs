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
