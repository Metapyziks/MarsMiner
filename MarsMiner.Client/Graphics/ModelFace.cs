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
        private readonly float[] myTexCoordIndexes;
        private readonly float myNormalIndex;

        public readonly Vector3[] Vertices;
        public readonly Vector2[] TextureCoords;

        public readonly Vector3 Normal;

        public ModelFace( Vector3[] vertices, Vector2[] texCoords )
        {
            if ( vertices.Length < 3 )
                throw new Exception( "At least three vertices expected" );

            if ( texCoords.Length != vertices.Length )
                throw new Exception( "Mismatch between number of vertices and texture coordinates" );

            Vertices = vertices;
            TextureCoords = texCoords;

            myTexCoordIndexes = new float[ TextureCoords.Length ];
            for ( int i = 0; i < TextureCoords.Length; ++i )
                myTexCoordIndexes[ i ] = (float) Math.Round( TextureCoords[ i ].X * 16.0f )
                    + (float) Math.Round( TextureCoords[ i ].Y * 16.0f ) * 17.0f;

            Normal = Vector3.Cross( Vertices[ 1 ] - Vertices[ 0 ], Vertices[ 2 ] - Vertices[ 0 ] );
            float absX = Math.Abs( Normal.X );
            float absY = Math.Abs( Normal.Y );
            float absZ = Math.Abs( Normal.Z );

            if ( absX >= absY && absX >= absZ )
                myNormalIndex = Normal.X < 0 ? 0 : 3;
            else if ( absY >= absZ )
                myNormalIndex = Normal.Y < 0 ? 1 : 4;
            else
                myNormalIndex = Normal.Z < 0 ? 2 : 5;
        }

        public float[] GenerateEntityVertexData( Matrix4 transform )
        {
            throw new NotImplementedException();
        }

        public float[] GenerateGeometryVertexData( Vector3 offset, float size, int textureIndex )
        {
            int triCount = Vertices.Length - 2;

            float faceInfo = ( size * 6 + myNormalIndex ) * 289;

            float[] data = new float[ triCount * 3 * 5 ];
            float[] first = new float[]
            {
                Vertices[ 0 ].X + offset.X,
                Vertices[ 0 ].Y + offset.Y,
                Vertices[ 0 ].Z + offset.Z,
                faceInfo + myTexCoordIndexes[ 0 ],
                textureIndex
            };
            float[] last = new float[]
            {
                Vertices[ 1 ].X + offset.X,
                Vertices[ 1 ].Y + offset.Y,
                Vertices[ 1 ].Z + offset.Z,
                faceInfo + myTexCoordIndexes[ 1 ],
                textureIndex
            };

            for ( int i = 2; i < Vertices.Length; ++i )
            {
                first.CopyTo( data, ( i - 2 ) * 15 );
                last.CopyTo( data, ( i - 2 ) * 15 + 5 );
                last = new float[]
                {
                    Vertices[ 1 ].X + offset.X,
                    Vertices[ 1 ].Y + offset.Y,
                    Vertices[ 1 ].Z + offset.Z,
                    faceInfo + myTexCoordIndexes[ 1 ],
                    textureIndex
                };
                last.CopyTo( data, ( i - 2 ) * 15 + 10 );
            }

            return data;
        }
    }
}
