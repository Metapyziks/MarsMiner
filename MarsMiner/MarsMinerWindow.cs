using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

using MarsMiner.Shared;
using MarsMiner.Client.Graphics;
using MarsMiner.Client.UI;
using OpenTK.Graphics.OpenGL;

namespace MarsMiner
{
    class MarsMinerWindow : GameWindow
    {
        private SpriteShader mySpriteShader;
        private UIObject myUIRoot;

        private UILabel myFPSText;

        private double myTotalFrameTime;
        private int myFramesCompleted;

        private OctreeTest myTestOctree;
        private OctreeTestShader myTestShader;
        private OctreeTestRenderer myTestRenderer;

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        public MarsMinerWindow()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 0 ), 8, 0, 4 ), "MarsMiner" )
        {
            WindowBorder = WindowBorder.Fixed;
            VSync = VSyncMode.Off;

            myCaptureMouse = true;
            myIgnoreMouse = false;

            myTotalFrameTime = 0.0;
            myFramesCompleted = 0;
        }

        protected override void OnLoad( System.EventArgs e )
        {
            mySpriteShader = new SpriteShader( Width, Height );
            myUIRoot = new UIObject( new Vector2( Width, Height ) );

            myFPSText = new UILabel( Font.Large, new Vector2( 4.0f, 4.0f ) );
            myFPSText.Text = "FT: ??ms FPS: ??";
            myUIRoot.AddChild( myFPSText );

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonEvent;
            Mouse.ButtonDown += OnMouseButtonEvent;

            var generator = new OctreeTestWorldGenerator();

            myTestOctree = generator.Generate();
            myTestShader = new OctreeTestShader( Width, Height );
            myTestRenderer = new OctreeTestRenderer( myTestOctree );
            myTestRenderer.UpdateVertices();

            myTestShader.CameraPosition = new Vector3( 0.0f, 8.0f, -64.0f );

            GL.ClearColor( Color4.CornflowerBlue );
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            DateTime start = DateTime.Now;

            GL.Clear( ClearBufferMask.ColorBufferBit );
            GL.Clear( ClearBufferMask.DepthBufferBit );
            
            myTestShader.StartBatch();
            myTestRenderer.Render( myTestShader );
            myTestShader.EndBatch();

            mySpriteShader.Begin();
            myUIRoot.Render( mySpriteShader );
            mySpriteShader.End();

            SwapBuffers();

            DateTime end = DateTime.Now;

            myTotalFrameTime += ( end - start ).TotalMilliseconds;
            ++myFramesCompleted;
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
            if ( myTotalFrameTime >= 1000.0 )
            {
                double period = myTotalFrameTime / myFramesCompleted;
                double freq = 1000.0 / period;

                myTotalFrameTime = 0.0;
                myFramesCompleted = 0;

                myFPSText.Text = "FT: " + period.ToString( "F" ) + "ms FPS: " + freq.ToString( "F" );
            }

            Vector3 movement = new Vector3( 0.0f, 0.0f, 0.0f );
            float angleY = myTestShader.CameraRotation.Y;
            float angleX = myTestShader.CameraRotation.X;

            if ( Keyboard[ Key.D ] )
            {
                movement.X += (float) Math.Cos( angleY );
                movement.Z += (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.A ] )
            {
                movement.X -= (float) Math.Cos( angleY );
                movement.Z -= (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.S ] )
            {
                movement.Z += (float) Math.Cos( angleY ) * (float) Math.Cos( angleX );
                movement.Y += (float) Math.Sin( angleX );
                movement.X -= (float) Math.Sin( angleY ) * (float) Math.Cos( angleX );
            }
            if ( Keyboard[ Key.W ] )
            {
                movement.Z -= (float) Math.Cos( angleY ) * (float) Math.Cos( angleX );
                movement.Y -= (float) Math.Sin( angleX );
                movement.X += (float) Math.Sin( angleY ) * (float) Math.Cos( angleX );
            }

            if ( movement.Length != 0 )
            {
                movement.Normalize();
                myTestShader.CameraPosition = myTestShader.CameraPosition + movement;
            }
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            if ( myCaptureMouse )
            {
                Vector3 movement = new Vector3( 0.0f, 0.0f, 0.0f );

                switch( e.KeyChar )
                {
                    case (char) 0x1B:
                        myCaptureMouse = false;
                        break;
                    case 'l':
                        myTestShader.LineMode = !myTestShader.LineMode;
                        break;
                }
            }
            else
            {
                myUIRoot.SendKeyPressEvent( e );
            }
        }

        private void OnMouseMove( object sender, MouseMoveEventArgs e )
        {
            if ( myIgnoreMouse )
            {
                myIgnoreMouse = false;
                return;
            }

            if ( myCaptureMouse )
            {
                Vector2 rot = myTestShader.CameraRotation;

                rot.Y += e.XDelta / 180.0f;
                rot.X += e.YDelta / 180.0f;
                rot.X = Tools.Clamp( rot.X, (float) -Math.PI / 2.0f, (float) Math.PI / 2.0f );

                myTestShader.CameraRotation = rot;

                myIgnoreMouse = true;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point( Bounds.Left + Width / 2, Bounds.Top + Height / 2 );
            }
            else
            {
                myUIRoot.SendMouseMoveEvent( new Vector2( Mouse.X, Mouse.Y ), e );
            }
        }

        private void OnMouseButtonEvent( object sender, MouseButtonEventArgs e )
        {
            myUIRoot.SendMouseButtonEvent( new Vector2( Mouse.X, Mouse.Y ), e );
        }
    }
}
