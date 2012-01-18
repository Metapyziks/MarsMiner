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
using System.Collections.Generic;
using System.Threading;

using MarsMiner.Shared;
using MarsMiner.Shared.Geometry;
using MarsMiner.Shared.Octree;

namespace MarsMiner.Client.Graphics
{
    public class ChunkRenderer
    {
        private VertexBuffer myVertexBuffer;

        private float[] myVertices;

        public readonly Chunk Chunk;

        public ChunkRenderer( Chunk chunk )
        {
            Chunk = chunk;
            myVertexBuffer = new VertexBuffer( 5 );
        }

        public void UpdateVertices( GeometryShader shader )
        {
            Monitor.Enter( this );
            myVertices = FindVertices( shader );
            Monitor.Exit( this );
        }

        private float[] FindVertices( GeometryShader shader )
        {
            List<float> verts = new List<float>();
            FindSolidFacesDelegate<UInt16> solidCheck =
                ( x => BlockManager.Get( x ).SolidFaces );

            foreach ( Octree<UInt16> octree in Chunk.Octrees )
            {
                var iter = (OctreeEnumerator<UInt16>) octree.GetEnumerator();
                while( iter.MoveNext() )
                {
                    OctreeNode<UInt16> node = iter.Current;

                    BlockType type = BlockManager.Get( node.Value );

                    if ( !type.IsVisible )
                        continue;

                    int size = iter.Size;

                    float x0 = iter.X; float x1 = x0 + size;
                    float y0 = iter.Y; float y1 = y0 + size;
                    float z0 = iter.Z; float z1 = z0 + size;

                    bool[] exposed = new bool[ 6 ];

                    for ( int i = 0; i < 6; ++i )
                        exposed[ i ] = node.IsFaceExposed( Face.FromIndex( i ), solidCheck );

                    if ( node.IsFaceExposed( Face.Front, solidCheck ) )
                    {
                        int t = shader.GetFaceTileIndex( node.Value, Face.Front );
                        int f = ( size * 6 + Face.Front.Index ) * 4;

                        verts.AddRange( new float[]
                        {
                            x0, y0, z0, 0 + f, t,
                            x1, y0, z0, 1 + f, t,
                            x1, y1, z0, 2 + f, t,
                            x0, y1, z0, 3 + f, t,
                        } );
                    }

                    if ( node.IsFaceExposed( Face.Right, solidCheck ) )
                    {
                        int t = shader.GetFaceTileIndex( node.Value, Face.Right );
                        int f = ( size * 6 + Face.Right.Index ) * 4;

                        verts.AddRange( new float[]
                        {
                            x1, y0, z0, 0 + f, t,
                            x1, y0, z1, 1 + f, t,
                            x1, y1, z1, 2 + f, t,
                            x1, y1, z0, 3 + f, t,
                        } );
                    }

                    if ( node.IsFaceExposed( Face.Back, solidCheck ) )
                    {
                        int t = shader.GetFaceTileIndex( node.Value, Face.Back );
                        int f = ( size * 6 + Face.Back.Index ) * 4;

                        verts.AddRange( new float[]
                        {
                            x1, y0, z1, 0 + f, t,
                            x0, y0, z1, 1 + f, t,
                            x0, y1, z1, 2 + f, t,
                            x1, y1, z1, 3 + f, t,
                        } );
                    }

                    if ( node.IsFaceExposed( Face.Left, solidCheck ) )
                    {
                        int t = shader.GetFaceTileIndex( node.Value, Face.Left );
                        int f = ( size * 6 + Face.Left.Index ) * 4;

                        verts.AddRange( new float[]
                        {
                            x0, y0, z1, 0 + f, t,
                            x0, y0, z0, 1 + f, t,
                            x0, y1, z0, 2 + f, t,
                            x0, y1, z1, 3 + f, t,
                        } );
                    }

                    if ( node.IsFaceExposed( Face.Bottom, solidCheck ) )
                    {
                        int t = shader.GetFaceTileIndex( node.Value, Face.Bottom );
                        int f = ( size * 6 + Face.Bottom.Index ) * 4;

                        verts.AddRange( new float[]
                        {
                            x0, y0, z0, 0 + f, t,
                            x0, y0, z1, 1 + f, t,
                            x1, y0, z1, 2 + f, t,
                            x1, y0, z0, 3 + f, t,
                        } );
                    }

                    if ( node.IsFaceExposed( Face.Top, solidCheck ) )
                    {
                        int t = shader.GetFaceTileIndex( node.Value, Face.Top );
                        int f = ( size * 6 + Face.Top.Index ) * 4;

                        verts.AddRange( new float[]
                        {
                            x0, y1, z0, 0 + f, t,
                            x1, y1, z0, 1 + f, t,
                            x1, y1, z1, 2 + f, t,
                            x0, y1, z1, 3 + f, t,
                        } );
                    }
                }
            }

            return verts.ToArray();
        }

        public void Render( ShaderProgram shader )
        {
            if ( myVertices != null )
            {
                Monitor.Enter( this );
                myVertexBuffer.SetData( myVertices );
                myVertices = null;
                Monitor.Exit( this );
            }

            myVertexBuffer.Render( shader );
        }

        public void Dispose()
        {
            myVertexBuffer.Dispose();
        }
    }
}
