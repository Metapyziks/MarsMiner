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

using OpenTK;

namespace MarsMiner.Client.Graphics
{
    public class ModelFace
    {
        private float[] myTexCoordIndexes;
        private float myNormalIndex;

        private float myTextureIndex;

        public Vector3[] Vertices { get; private set; }
        public Vector2[] TextureCoords { get; private set; }

        public Vector3 Normal { get; private set; }

        public String TextureName { get; private set; }

        public int EntityDataLength { get; private set; }
        public int GeometryDataLength { get; private set; }

        public ModelFace( String textureName, Vector3[] vertices, Vector2[] texCoords )
        {
            if ( vertices.Length < 3 )
                throw new Exception( "At least three vertices expected" );

            if ( texCoords.Length != vertices.Length )
                throw new Exception( "Mismatch between number of vertices and texture coordinates" );

            Create( textureName, vertices, texCoords );
        }

        public ModelFace( String textureName, float[] vertices, float[] texCoords )
        {
            if ( vertices.Length < 9 || vertices.Length % 3 != 0 || texCoords.Length % 2 != 0 )
                throw new Exception( "At least three vertices expected" );

            if ( texCoords.Length / 3 != vertices.Length / 2 )
                throw new Exception( "Mismatch between number of vertices and texture coordinates" );

            Vector3[] verts = new Vector3[ vertices.Length / 3 ];
            Vector2[] txCos = new Vector2[ vertices.Length / 3 ];

            for ( int i = 0; i < verts.Length; ++i )
            {
                verts[ i ] = new Vector3(
                    vertices[ i * 3 + 0 ],
                    vertices[ i * 3 + 1 ],
                    vertices[ i * 3 + 2 ]
                );
                txCos[ i ] = new Vector2(
                    texCoords[ i * 2 + 0 ],
                    texCoords[ i * 2 + 1 ]
                );
            }

            Create( textureName, verts, txCos );
        }

        public ModelFace( String textureName, float[] data )
        {
            if ( data.Length < 15 || data.Length % 5 != 0 )
                throw new Exception( "At least three vertices expected" );

            Vector3[] verts = new Vector3[ data.Length / 5 ];
            Vector2[] txCos = new Vector2[ data.Length / 5 ];

            for ( int i = 0; i < verts.Length; ++i )
            {
                verts[ i ] = new Vector3(
                    data[ i * 5 + 0 ],
                    data[ i * 5 + 1 ],
                    data[ i * 5 + 2 ]
                );
                txCos[ i ] = new Vector2(
                    data[ i * 5 + 3 ],
                    data[ i * 5 + 4 ]
                );
            }

            Create( textureName, verts, txCos );
        }

        private void Create( String textureName, Vector3[] vertices, Vector2[] texCoords )
        {
            Vertices = vertices;
            TextureCoords = texCoords;

            myTexCoordIndexes = new float[ TextureCoords.Length ];
            for ( int i = 0; i < TextureCoords.Length; ++i )
                myTexCoordIndexes[ i ] = (float) Math.Round( TextureCoords[ i ].X * 16.0f )
                    + (float) Math.Round( TextureCoords[ i ].Y * 16.0f ) * 17.0f;

            Normal = Vector3.Cross( Vertices[ 2 ] - Vertices[ 0 ], Vertices[ 1 ] - Vertices[ 0 ] );

            float absX = Math.Abs( Normal.X );
            float absY = Math.Abs( Normal.Y );
            float absZ = Math.Abs( Normal.Z );

            if ( absX >= absY && absX >= absZ )
                myNormalIndex = Normal.X < 0 ? 0 : 3;
            else if ( absY >= absZ )
                myNormalIndex = Normal.Y < 0 ? 1 : 4;
            else
                myNormalIndex = Normal.Z < 0 ? 2 : 5;

            TextureName = textureName;
            myTextureIndex = 0.0f;

            GeometryDataLength = ( Vertices.Length - 2 ) * 3 * 5;
        }

        public void UpdateTextureIndex( Texture2DArray texArray )
        {
            myTextureIndex = texArray.GetTextureIndex( TextureName );
        }

        public float[] GenerateEntityVertexData( Matrix4 transform )
        {
            throw new NotImplementedException();
        }

        public float[] GenerateGeometryVertexData( Vector3 offset, float size )
        {
            float faceInfo = ( size * 6 + myNormalIndex ) * 289;

            float[] data = new float[ GeometryDataLength ];
            float[] first = new float[]
            {
                Vertices[ 0 ].X * size + offset.X,
                Vertices[ 0 ].Y * size + offset.Y,
                Vertices[ 0 ].Z * size + offset.Z,
                faceInfo + myTexCoordIndexes[ 0 ],
                myTextureIndex
            };
            float[] last = new float[]
            {
                Vertices[ 1 ].X * size + offset.X,
                Vertices[ 1 ].Y * size + offset.Y,
                Vertices[ 1 ].Z * size + offset.Z,
                faceInfo + myTexCoordIndexes[ 1 ],
                myTextureIndex
            };

            for ( int i = 2; i < Vertices.Length; ++i )
            {
                first.CopyTo( data, ( i - 2 ) * 15 );
                last.CopyTo( data, ( i - 2 ) * 15 + 5 );
                last = new float[]
                {
                    Vertices[ i ].X * size + offset.X,
                    Vertices[ i ].Y * size + offset.Y,
                    Vertices[ i ].Z * size + offset.Z,
                    faceInfo + myTexCoordIndexes[ i ],
                    myTextureIndex
                };
                last.CopyTo( data, ( i - 2 ) * 15 + 10 );
            }

            return data;
        }
    }
}
