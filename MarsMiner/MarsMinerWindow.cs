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
using MarsMiner.Shared.Geometry;
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

        private World myTestWorld;

        private GeometryShader myGeoShader;
        private List<ChunkRenderer> myGeoRenderers;

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
            Plugin.Register( "MarsMiner.Shared.CorePlugin", true, true );
            Plugin.Register( "MarsMiner.Shared.MarsMinerPlugin", true, true );

            mySpriteShader = new SpriteShader( Width, Height );
            myUIRoot = new UIObject( new Vector2( Width, Height ) );

            myFPSText = new UILabel( Font.Large, new Vector2( 4.0f, 4.0f ) );
            myFPSText.Text = "FT: ??ms FPS: ?? MEM: ??";
            myUIRoot.AddChild( myFPSText );

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonEvent;
            Mouse.ButtonDown += OnMouseButtonEvent;

            myTestWorld = new World();

            myGeoShader = new GeometryShader( Width, Height );
            myGeoShader.UpdateTileMap( 16 );
            
            myGeoRenderers = new List<ChunkRenderer>();

            myTestWorld.ChunkLoaded += OnChunkEvent;
            myTestWorld.ChunkUnloaded += OnChunkEvent;
            myTestWorld.ChunkChanged += OnChunkEvent;

            myTestWorld.Generate( 1024, 1024 );

            myGeoShader.CameraPosition = new Vector3( 0.0f, 1024.0f, 0.0f );

            GL.ClearColor( new Color4( 223, 186, 168, 255 ) );

            myFrameTimer.Start();
        }

        private void OnChunkEvent( object sender, ChunkEventArgs e )
        {
            if ( myClosing )
                return;

            if ( e.EventType == ChunkEventType.Loaded )
            {
                ChunkRenderer renderer = new ChunkRenderer( e.Chunk );
                renderer.UpdateVertices( myGeoShader );

                Monitor.Enter( myGeoRenderers );
                myGeoRenderers.Add( renderer );
                Monitor.Exit( myGeoRenderers );
            }
            else
            {
                Monitor.Enter( myGeoRenderers );
                ChunkRenderer renderer = myGeoRenderers.Find( x => x.Chunk == e.Chunk );
                if ( e.EventType == ChunkEventType.Unloaded )
                    myGeoRenderers.Remove( renderer );
                Monitor.Exit( myGeoRenderers );
                if ( e.EventType == ChunkEventType.Changed )
                    renderer.UpdateVertices( myGeoShader );
            }
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.Clear( ClearBufferMask.ColorBufferBit );
            GL.Clear( ClearBufferMask.DepthBufferBit );
            
            myGeoShader.StartBatch();
            Monitor.Enter( myGeoRenderers );
            foreach( ChunkRenderer renderer in myGeoRenderers )
                renderer.Render( myGeoShader );
            Monitor.Exit( myGeoRenderers );
            myGeoShader.EndBatch();

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
            float angleY = myGeoShader.CameraRotation.Y;
            float angleX = myGeoShader.CameraRotation.X;

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
                myGeoShader.CameraPosition = myGeoShader.CameraPosition + movement;
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
                    myGeoShader.LineMode = !myGeoShader.LineMode;
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
                Vector2 rot = myGeoShader.CameraRotation;

                rot.Y += e.XDelta / 180.0f;
                rot.X += e.YDelta / 180.0f;
                rot.X = Tools.Clamp( rot.X, (float) -Math.PI / 2.0f, (float) Math.PI / 2.0f );

                myGeoShader.CameraRotation = rot;

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

            Monitor.Enter( myGeoRenderers );
            foreach ( ChunkRenderer renderer in myGeoRenderers )
                renderer.Dispose();
            Monitor.Exit( myGeoRenderers );

            base.Dispose();
        }
    }
}
