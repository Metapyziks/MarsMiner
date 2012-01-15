using System;
using System.Threading;
using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
