using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

using MarsMiner.Client.Graphics;
using MarsMiner.Client.UI;

namespace MarsMiner
{
    class MarsMinerWindow : GameWindow
    {
        private SpriteShader mySpriteShader;
        private UIObject myUIRoot;

        public MarsMinerWindow()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 0 ), 32, 0, 4 ), "MarsMiner" )
        {
            WindowBorder = WindowBorder.Fixed;
            VSync = VSyncMode.Off;
        }

        protected override void OnLoad( System.EventArgs e )
        {
            mySpriteShader = new SpriteShader( Width, Height );
            myUIRoot = new UIPanel( new Vector2( Width, Height ) ) { Colour = Color4.Black };

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonEvent;
            Mouse.ButtonDown += OnMouseButtonEvent;

            var window = new UIWindow( new Vector2( 320, 240 ) )
            {
                Title = "Hello World!"
            };
            myUIRoot.AddChild( window );
            window.Centre();
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            mySpriteShader.Begin();
            myUIRoot.Render( mySpriteShader );
            mySpriteShader.End();

            SwapBuffers();
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            myUIRoot.SendKeyPressEvent( e );
        }

        private void OnMouseMove( object sender, MouseMoveEventArgs e )
        {
            myUIRoot.SendMouseMoveEvent( new Vector2( Mouse.X, Mouse.Y ), e );
        }

        private void OnMouseButtonEvent( object sender, MouseButtonEventArgs e )
        {
            myUIRoot.SendMouseButtonEvent( new Vector2( Mouse.X, Mouse.Y ), e );
        }
    }
}
