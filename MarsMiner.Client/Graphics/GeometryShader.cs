/**
 * Copyright (c) 2012 James King [metapyziks@gmail.com]
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
using System.Drawing;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using ResourceLib;

using MarsMiner.Shared.Geometry;
using MarsMiner.Shared.Octree;

namespace MarsMiner.Client.Graphics
{
    public class GeometryShader : ShaderProgram
    {
        private Matrix4 myViewMatrix;
        private int myViewMatrixLoc;
        private int myTextureSizeLoc;
        private int myTilemapSizeLoc;

        private Vector3 myCameraPosition;
        private Vector2 myCameraRotation;
        private Matrix4 myPerspectiveMatrix;

        private bool myViewChanged;

        private int[,] myTileMapPointers;

        public readonly int TileSize;

        public Texture TileMap { get; private set; }

        public Vector3 CameraPosition
        {
            get { return myCameraPosition; }
            set
            {
                myCameraPosition = value;
                myViewChanged = true;
            }
        }
        public Vector2 CameraRotation
        {
            get { return myCameraRotation; }
            set
            {
                myCameraRotation = value;
                myViewChanged = true;
            }
        }
        public Matrix4 PerspectiveMatrix
        {
            get { return myPerspectiveMatrix; }
            set
            {
                myPerspectiveMatrix = value;
                myViewChanged = true;
            }
        }

        public bool LineMode { get; set; }

        public GeometryShader( int tileSize = 16 )
        {
            TileSize = tileSize;

            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Int, "tilemap_size" );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_tex" );
            vert.AddVarying( ShaderVarType.Float, "var_shade" );
            vert.AddVarying( ShaderVarType.Vec2, "var_tex_min" );
            vert.AddVarying( ShaderVarType.Vec2, "var_tex" );
            vert.Logic = @"
                void main( void )
                {
                    var_tex_min = vec2( float( int( in_tex.y ) % tilemap_size ) / float( tilemap_size ), floor( in_tex.y / float( tilemap_size ) ) / float( tilemap_size ) );

                    float size = in_tex.z / float( tilemap_size );
                    
                    int face = int( in_tex.x / 4 );
                    int vert = int( in_tex.x ) % 4;                    

                    switch( face )
                    {
                        case 0:
                            var_shade = 0.9; break;
                        case 1:
                            var_shade = 0.7; break;
                        case 2:
                            var_shade = 0.8; break;
                        case 3:
                            var_shade = 0.9; break;
                        case 4:
                            var_shade = 1.0; break;
                        case 5:
                            var_shade = 0.8; break;
                    }

                    switch( vert )
                    {
                        case 0:
                            var_tex = vec2( 0.0f, size ); break;
                        case 1:
                            var_tex = vec2( size, size ); break;
                        case 2:
                            var_tex = vec2( size, 0.0f ); break;
                        case 3:
                            var_tex = vec2( 0.0f, 0.0f ); break;
                    }

                    gl_Position = view_matrix * vec4( in_position, 1.0 );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddUniform( ShaderVarType.Float, "texture_size" );
            frag.AddUniform( ShaderVarType.Int, "tilemap_size" );
            frag.AddUniform( ShaderVarType.Sampler2D, "tilemap" );
            frag.AddVarying( ShaderVarType.Float, "var_shade" );
            frag.AddVarying( ShaderVarType.Vec2, "var_tex_min" );
            frag.AddVarying( ShaderVarType.Vec2, "var_tex" );
            frag.Logic = @"
                void main( void )
                {
                    int sixteenth = int( texture_size ) / tilemap_size;

                    vec2 pos = vec2( float( int( var_tex.x * texture_size ) % sixteenth ) / texture_size, float( int( var_tex.y * texture_size ) % sixteenth ) / texture_size ) + var_tex_min;

                    out_frag_colour = texture( tilemap, pos ) * vec4( var_shade, var_shade, var_shade, 1.0 );
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;

            CameraPosition = new Vector3();
            CameraRotation = new Vector2();
        }

        public GeometryShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        public void SetScreenSize( int width, int height )
        {
            PerspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(
                (float) Math.PI * ( 60.0f / 180.0f ), (float) width / (float) height, 0.125f, 2048.0f );
            UpdateViewMatrix();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_position", 3 );
            AddAttribute( "in_tex", 3 );

            AddTexture( "tilemap", TextureUnit.Texture0 );

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
            myTextureSizeLoc = GL.GetUniformLocation( Program, "texture_size" );
            myTilemapSizeLoc = GL.GetUniformLocation( Program, "tilemap_size" );
        }

        public void GenerateTileMap()
        {
            int count = BlockManager.TypeCount;

            List<String> allocated = new List<string>();
            myTileMapPointers = new int[ count, 6 ];

            for ( UInt16 i = 0; i < count; ++i )
            {
                BlockType type = BlockManager.Get( i );
                String[] faceImages = type.TileGraphics;

                if ( faceImages == null || faceImages.Length == 0 )
                    continue;

                int[] indexes = new int[ faceImages.Length ];

                for ( int j = 0; j < faceImages.Length; ++j )
                {
                    int index = allocated.IndexOf( faceImages[ j ] );
                    if ( index == -1 )
                    {
                        index = allocated.Count;
                        allocated.Add( faceImages[ j ] );
                    }
                    indexes[ j ] = index;
                }

                if ( indexes.Length == 1 )
                {
                    int img = indexes[ 0 ];
                    indexes = new int[]
                    {
                        img, img, img, img, img, img
                    };
                }
                else if ( indexes.Length == 2 )
                {
                    int floor = indexes[ 0 ];
                    int wall = indexes[ 1 ];
                    indexes = new int[]
                    {
                        wall, floor, wall, wall, floor, wall
                    };
                }
                else if ( indexes.Length == 3 )
                {
                    int floor = indexes[ 0 ];
                    int wall = indexes[ 1 ];
                    int ceil = indexes[ 2 ];
                    indexes = new int[]
                    {
                        wall, ceil, wall, wall, floor, wall
                    };
                }
                else if ( indexes.Length != 6 )
                    continue;

                for ( int f = 0; f < 6; ++f )
                    myTileMapPointers[ i, f ] = indexes[ f ];
            }

            int size = 1;
            while ( size * size < allocated.Count )
                size <<= 1;

            Bitmap bmp = new Bitmap( size * TileSize, size * TileSize );
            var g = System.Drawing.Graphics.FromImage( bmp );

            for ( int i = 0; i < allocated.Count; ++i )
            {
                Bitmap tile = Res.Get<Texture>( allocated[ i ] ).Bitmap;
                if ( tile.Width != tile.Height || tile.Height != TileSize )
                    tile = new Bitmap( tile, new Size( TileSize, TileSize ) );

                Point pos = new Point( ( i % size ) * TileSize, ( i / size ) * TileSize );

                g.DrawImage( tile, pos );
            }

            g.Dispose();

            TileMap = new Texture( bmp );

            SetTexture( "tilemap", TileMap );
            GL.Uniform1( myTextureSizeLoc, (float) ( size * TileSize ) );
            GL.Uniform1( myTilemapSizeLoc, size );
        }

        public int GetFaceTileIndex( UInt16 typeID, Face face )
        {
            return myTileMapPointers[ typeID, face.Index ];
        }

        private void UpdateViewMatrix()
        {
            Matrix4 yRot = Matrix4.CreateRotationY( CameraRotation.Y );
            Matrix4 xRot = Matrix4.CreateRotationX( CameraRotation.X );
            Matrix4 trns = Matrix4.CreateTranslation( -CameraPosition );

            myViewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot), xRot ), PerspectiveMatrix );

            GL.UniformMatrix4( myViewMatrixLoc, false, ref myViewMatrix );
        }

        protected override void OnStartBatch()
        {
            if ( myViewChanged )
                UpdateViewMatrix();

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.CullFace );

            GL.CullFace( CullFaceMode.Front );

            if ( LineMode )
                GL.PolygonMode( MaterialFace.Back, PolygonMode.Line );
        }

        protected override void OnEndBatch()
        {
            if ( LineMode )
                GL.PolygonMode( MaterialFace.Back, PolygonMode.Fill );

            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.CullFace );
        }
    }
}
