using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MarsMiner.Shared;
using System.Threading;

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
            myVertexBuffer = new VertexBuffer( 4 );
            myChunkChanged = true;
        }

        public void UpdateVertices()
        {
            myChunkChanged = true;
        }

        public float[] FindVertices()
        {
            List<float> verts = new List<float>();

            foreach ( OctreeTest chunkOctree in Chunk.Octrees )
            {
                foreach ( OctreeTest octree in chunkOctree )
                {
                    if ( octree.Value == OctreeTestBlockType.Empty )
                        continue;

                    float x0 = octree.X; float x1 = octree.X + octree.Size;
                    float y0 = octree.Y; float y1 = octree.Y + octree.Size;
                    float z0 = octree.Z; float z1 = octree.Z + octree.Size;

                    float r0 = octree.Bottom / 256.0f;
                    float r1 = octree.Top / 256.0f;

                    octree.UpdateFace( Face.All );

                    if ( ( octree.Exposed & Face.Front ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z0, r0,
                            x1, y0, z0, r0,
                            x1, y1, z0, r1,
                            x0, y1, z0, r1,
                        } );

                    if ( ( octree.Exposed & Face.Right ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x1, y0, z0, r0,
                            x1, y0, z1, r0, 
                            x1, y1, z1, r1,
                            x1, y1, z0, r1,
                        } );

                    if ( ( octree.Exposed & Face.Back ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x1, y0, z1, r0,
                            x0, y0, z1, r0,
                            x0, y1, z1, r1,
                            x1, y1, z1, r1,
                        } );

                    if ( ( octree.Exposed & Face.Left ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z1, r0,
                            x0, y0, z0, r0,
                            x0, y1, z0, r1,
                            x0, y1, z1, r1,
                        } );

                    if ( ( octree.Exposed & Face.Bottom ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z0, r0,
                            x0, y0, z1, r0,
                            x1, y0, z1, r0,
                            x1, y0, z0, r0,
                        } );

                    if ( ( octree.Exposed & Face.Top ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y1, z0, r1,
                            x1, y1, z0, r1,
                            x1, y1, z1, r1,
                            x0, y1, z1, r1,
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
