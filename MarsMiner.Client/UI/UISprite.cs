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
