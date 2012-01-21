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
using OpenTK;

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
            FindSolidFacesDelegate<UInt16> solidCheck = ( x =>
            {
                var comp = BlockManager.Get( x ).GetComponant<VisibilityBComponant>();
                if ( comp != null )
                    return comp.SolidFaces;
                return Face.None;
            } );

            foreach ( Octree<UInt16> octree in Chunk.Octrees )
            {
                var iter = (OctreeEnumerator<UInt16>) octree.GetEnumerator();
                while( iter.MoveNext() )
                {
                    OctreeLeaf<UInt16> leaf = iter.Current;

                    BlockType type = BlockManager.Get( leaf.Value );
                    var vis = type.GetComponant<VisibilityBComponant>();

                    if ( vis == null || !vis.IsVisible )
                        continue;

                    var mdl = type.GetComponant<ModelBComponant>();

                    if ( mdl == null )
                        continue;

                    int size = iter.Size;

                    Vector3 offset = new Vector3( iter.X, iter.Y, iter.Z );

                    verts.AddRange( mdl.Model.GenerateVertexData( offset, size,
                        leaf.FindExposedFaces( solidCheck ) ) );
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
