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

        private Octree<OctreeTestBlockType> myTestOctree;
        private OctreeTestShader myTestShader;
        private OctreeTestRenderer myTestRenderer;

        private bool myLineMode;

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        public MarsMinerWindow()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 0 ), 8, 0, 4 ), "MarsMiner" )
        {
            WindowBorder = WindowBorder.Fixed;
            VSync = VSyncMode.Off;

            myLineMode = false;

            myCaptureMouse = true;
            myIgnoreMouse = false;
        }

        protected override void OnLoad( System.EventArgs e )
        {
            mySpriteShader = new SpriteShader( Width, Height );
            myUIRoot = new UIObject( new Vector2( Width, Height ) );

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonEvent;
            Mouse.ButtonDown += OnMouseButtonEvent;

            myTestOctree = new Octree<OctreeTestBlockType>( -128, -128, -128, 256 );
            myTestShader = new OctreeTestShader( Width, Height );
            myTestRenderer = new OctreeTestRenderer( myTestOctree );

            myTestOctree.SetCuboid( new Cuboid( -128, 0, -128, 256, 128, 256 ), OctreeTestBlockType.Red );
            myTestOctree.SetCuboid( new Cuboid( -8, -16, -8, 16, 32, 16 ), OctreeTestBlockType.Blue );
            myTestOctree.SetCuboid( new Cuboid( -7, -16, -6, 6, 3, 9 ), OctreeTestBlockType.Empty );
            myTestRenderer.UpdateVertices();

            myTestShader.CameraPosition = new Vector3( 0.0f, 8.0f, -64.0f );

            GL.ClearColor( Color4.CornflowerBlue );
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.Clear( ClearBufferMask.ColorBufferBit );
            GL.Clear( ClearBufferMask.DepthBufferBit );

            myTestShader.StartBatch();
            myTestRenderer.Render( myTestShader );
            myTestShader.EndBatch();

            //mySpriteShader.Begin();
            //myUIRoot.Render( mySpriteShader );
            //mySpriteShader.End();

            SwapBuffers();
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
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
                        myLineMode = !myLineMode;
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
