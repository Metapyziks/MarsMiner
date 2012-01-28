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

        private Vector3 myCameraPosition;
        private Vector2 myCameraRotation;
        private Matrix4 myPerspectiveMatrix;

        private bool myViewChanged;

        private Texture2DArray myTileMap;

        private Dictionary<String,UInt16> myTileMapPointers;

        public readonly int TileSize;

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

        public GeometryShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec2, "in_tex" );
            vert.AddVarying( ShaderVarType.Float, "var_shade" );
            vert.AddVarying( ShaderVarType.Vec3, "var_tex" );
            vert.Logic = @"
                void main( void )
                {
                    int faceData = int( in_tex.x / 289 );
                    int texPosIndex = int( in_tex.x ) % 289;

                    float size = float( faceData / 6 );
                    int face = faceData % 6;

                    switch( face )
                    {
                        case 0:
                            var_shade = 0.9; break;
                        case 1:
                            var_shade = 1.0; break;
                        case 2:
                            var_shade = 0.8; break;
                        case 3:
                            var_shade = 0.9; break;
                        case 4:
                            var_shade = 0.7; break;
                        case 5:
                            var_shade = 0.8; break;
                    }

                    vec2 texPos = vec2( texPosIndex % 17, texPosIndex / 17 );

                    var_tex = vec3( texPos / 16.0 * size, in_tex.y );

                    gl_Position = view_matrix * vec4( in_position, 1.0 );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddUniform( ShaderVarType.Sampler2DArray, "tilemap" );
            frag.AddVarying( ShaderVarType.Float, "var_shade" );
            frag.AddVarying( ShaderVarType.Vec3, "var_tex" );
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = texture2DArray( tilemap, var_tex ) * vec4( var_shade, var_shade, var_shade, 1.0 );
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Triangles;

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
            AddAttribute( "in_tex", 2 );

            AddTexture( "tilemap", TextureUnit.Texture0 );
            
            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
        }

        public void UpdateTileMap( int size )
        {
            myTileMap = new Texture2DArray( size, size, GeometryModel.UsedTextures );
            SetTexture( "tilemap", myTileMap );

            foreach ( BlockType type in BlockManager.GetAll() )
            {
                var mdl = type.GetComponant<ModelBComponant>();
                if ( mdl != null )
                    mdl.Model.UpdateTextureIndexes( myTileMap );
            }
        }

        public int GetTileIndex( String textureName )
        {
            return myTileMapPointers[ textureName ];
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

            GL.CullFace( CullFaceMode.Back );

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
