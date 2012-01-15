/**
 * Copyright (c) 2012 James King
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 * 
 * James King [metapyziks@gmail.com]
 */

using System.Collections.Generic;

using MarsMiner.Shared;

namespace MarsMiner.Client.Graphics
{
    public class OctreeTestRenderer
    {
        private VertexBuffer myVertexBuffer;
        private bool myChunkChanged;

        public readonly TestChunk Chunk;

        public OctreeTestRenderer( TestChunk chunk )
        {
            Chunk = chunk;
            myVertexBuffer = new VertexBuffer( 3 );
            myChunkChanged = true;
        }

        public void UpdateVertices()
        {
            myChunkChanged = true;
        }

        public float[] FindVertices()
        {
            List<float> verts = new List<float>();
            FindSolidFacesDelegate<OctreeTestBlockType> solidCheck =
                ( x => ( x == OctreeTestBlockType.Empty ? Face.None : Face.All ) );

            foreach ( OctreeTest chunkOctree in Chunk.Octrees )
            {
                foreach ( OctreeNode<OctreeTestBlockType> octree in chunkOctree )
                {
                    if ( octree.Value == OctreeTestBlockType.Empty )
                        continue;

                    float x0 = octree.X; float x1 = x0 + octree.Size;
                    float y0 = octree.Y; float y1 = y0 + octree.Size;
                    float z0 = octree.Z; float z1 = z0 + octree.Size;

                    if ( octree.IsFaceExposed( Face.Front, solidCheck ) )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z0,
                            x1, y0, z0,
                            x1, y1, z0,
                            x0, y1, z0,
                        } );

                    if ( octree.IsFaceExposed( Face.Right, solidCheck ) )
                        verts.AddRange( new float[]
                        {
                            x1, y0, z0,
                            x1, y0, z1,
                            x1, y1, z1,
                            x1, y1, z0,
                        } );

                    if ( octree.IsFaceExposed( Face.Back, solidCheck ) )
                        verts.AddRange( new float[]
                        {
                            x1, y0, z1,
                            x0, y0, z1,
                            x0, y1, z1,
                            x1, y1, z1,
                        } );

                    if ( octree.IsFaceExposed( Face.Left, solidCheck ) )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z1,
                            x0, y0, z0,
                            x0, y1, z0,
                            x0, y1, z1,
                        } );

                    if ( octree.IsFaceExposed( Face.Bottom, solidCheck ) )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z0,
                            x0, y0, z1,
                            x1, y0, z1,
                            x1, y0, z0,
                        } );

                    if ( octree.IsFaceExposed( Face.Top, solidCheck ) )
                        verts.AddRange( new float[]
                        {
                            x0, y1, z0,
                            x1, y1, z0,
                            x1, y1, z1,
                            x0, y1, z1,
                        } );
                }
            }

            return verts.ToArray();
        }

        public void Render( ShaderProgram shader )
        {
            if ( myChunkChanged )
            {
                myChunkChanged = false;
                myVertexBuffer.SetData( FindVertices() );
            }

            myVertexBuffer.Render( shader );
        }

        public void Dispose()
        {
            myVertexBuffer.Dispose();
        }
    }
}
