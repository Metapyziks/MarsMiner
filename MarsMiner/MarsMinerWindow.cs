/**
 * Copyright (c) 2012 James King [metapyziks@gmail.com]
 * Copyright (c) 2012 Tamme Schichler [tammeschichler@googlemail.com]
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
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using MarsMiner.Shared;
using MarsMiner.Client.Graphics;
using MarsMiner.Client.UI;

namespace MarsMiner
{
    class MarsMinerWindow : GameWindow
    {
        private SpriteShader mySpriteShader;
        private UIObject myUIRoot;

        private UILabel myFPSText;

        private long myTotalFrameTime;
        private int myFramesCompleted;

        private Stopwatch myFrameTimer;

        private TestWorld myTestWorld;

        private OctreeTestShader myTestShader;
        private List<OctreeTestRenderer> myTestRenderers;

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        private bool myClosing;

        public MarsMinerWindow()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 0 ), 8, 0, 4 ), "MarsMiner" )
        {
            WindowBorder = WindowBorder.Fixed;
            VSync = VSyncMode.Off;

            myCaptureMouse = true;
            myIgnoreMouse = false;
            myClosing = false;

            myTotalFrameTime = 0;
            myFramesCompleted = 0;
            myFrameTimer = new Stopwatch();
        }

        protected override void OnLoad( System.EventArgs e )
        {
            mySpriteShader = new SpriteShader( Width, Height );
            myUIRoot = new UIObject( new Vector2( Width, Height ) );

            myFPSText = new UILabel( Font.Large, new Vector2( 4.0f, 4.0f ) );
            myFPSText.Text = "FT: ??ms FPS: ?? MEM: ??";
            myUIRoot.AddChild( myFPSText );

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonEvent;
            Mouse.ButtonDown += OnMouseButtonEvent;

            myTestWorld = new TestWorld();

            myTestShader = new OctreeTestShader( Width, Height );
            myTestRenderers = new List<OctreeTestRenderer>();

            myTestWorld.ChunkLoaded += delegate( object sender, TestChunkLoadEventArgs ea )
            {
                if ( myClosing )
                    return;

                OctreeTestRenderer renderer = new OctreeTestRenderer( ea.Chunk );

                Monitor.Enter( myTestRenderers );
                myTestRenderers.Add( renderer );
                Monitor.Exit( myTestRenderers );
            };
            myTestWorld.ChunkUnloaded += delegate( object sender, TestChunkLoadEventArgs ea )
            {
                if ( myClosing )
                    return;

                Monitor.Enter( myTestRenderers );
                OctreeTestRenderer renderer = myTestRenderers.Find( x => x.Chunk == ea.Chunk );
                myTestRenderers.Remove( renderer );
                Monitor.Exit( myTestRenderers );
                renderer.Dispose();
            };
            myTestWorld.ChunkChanged += delegate( object sender, TestChunkLoadEventArgs ea )
            {
                if ( myClosing )
                    return;

                Monitor.Enter( myTestRenderers );
                OctreeTestRenderer renderer = myTestRenderers.Find( x => x.Chunk == ea.Chunk );
                Monitor.Exit( myTestRenderers );
                renderer.UpdateVertices();
            };

            myTestWorld.StartGenerator();

            myTestShader.CameraPosition = new Vector3( 0.0f, 256.0f, 0.0f );

            GL.ClearColor( Color4.CornflowerBlue );

            myFrameTimer.Start();
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.Clear( ClearBufferMask.ColorBufferBit );
            GL.Clear( ClearBufferMask.DepthBufferBit );
            
            myTestShader.StartBatch();
            Monitor.Enter( myTestRenderers );
            foreach( OctreeTestRenderer renderer in myTestRenderers )
                renderer.Render( myTestShader );
            Monitor.Exit( myTestRenderers );
            myTestShader.EndBatch();

            mySpriteShader.Begin();
            myUIRoot.Render( mySpriteShader );
            mySpriteShader.End();

            SwapBuffers();

            myTotalFrameTime += myFrameTimer.ElapsedTicks;
            ++myFramesCompleted;
            myFrameTimer.Restart();
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
            if ( myTotalFrameTime >= Stopwatch.Frequency )
            {
                double period = myTotalFrameTime / (Stopwatch.Frequency / 1000d) / myFramesCompleted;
                double freq = 1000 / period;

                myTotalFrameTime = 0;
                myFramesCompleted = 0;

                myFPSText.Text = string.Format("FT: {0:F}ms FPS: {1:F} MEM: {2:F}MB", period, freq, Process.GetCurrentProcess().PrivateMemorySize64 / ( 1024d * 1024d ) );
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
            switch( e.KeyChar )
            {
                case (char) 0x1B:
                    myCaptureMouse = !myCaptureMouse;
                    break;
                case 'l':
                case 'L':
                    myTestShader.LineMode = !myTestShader.LineMode;
                    break;
            }

            if( !myCaptureMouse )
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

        public override void Dispose()
        {
            myClosing = true;

            myTestWorld.StopGenerator();

            foreach ( OctreeTestRenderer renderer in myTestRenderers )
                renderer.Dispose();

            base.Dispose();
        }
    }
}
