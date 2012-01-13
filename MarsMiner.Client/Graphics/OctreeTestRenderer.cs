using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MarsMiner.Shared;

namespace MarsMiner.Client.Graphics
{
    public class OctreeTestRenderer
    {
        private static readonly Color4[] stBlockColours = new Color4[]
        {
            Color4.Black,
            Color4.White,
            Color4.Black,
            Color4.Red,
            Color4.Green,
            Color4.Blue,
        };

        private VertexBuffer myVertexBuffer;
        private bool myVertexDataChanged;

        public readonly TestChunk Chunk;

        public OctreeTestRenderer( TestChunk chunk )
        {
            Chunk = chunk;
            myVertexBuffer = new VertexBuffer( 7 );

            myVertexDataChanged = true;
        }

        public void UpdateVertices()
        {
            myVertexDataChanged = true;
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

                    Color4 colour = stBlockColours[ (int) octree.Value ];

                    float x0 = octree.X; float x1 = octree.X + octree.Size;
                    float y0 = octree.Y; float y1 = octree.Y + octree.Size;
                    float z0 = octree.Z; float z1 = octree.Z + octree.Size;

                    float r = ( octree.X % ( octree.Size * 2 ) ) / 2.0f + 0.5f;
                    float g = ( octree.Y % ( octree.Size * 2 ) ) / 2.0f + 0.5f;
                    float b = ( octree.Z % ( octree.Size * 2 ) ) / 2.0f + 0.5f;
                    float a = colour.A;

                    if ( ( octree.Exposed & Face.Front ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z0, r, g, b, a,
                            x1, y0, z0, r, g, b, a,
                            x1, y1, z0, r, g, b, a,
                            x0, y1, z0, r, g, b, a,
                        } );

                    if ( ( octree.Exposed & Face.Right ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x1, y0, z0, r, g, b, a,
                            x1, y0, z1, r, g, b, a,
                            x1, y1, z1, r, g, b, a,
                            x1, y1, z0, r, g, b, a,
                        } );

                    if ( ( octree.Exposed & Face.Back ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x1, y0, z1, r, g, b, a,
                            x0, y0, z1, r, g, b, a,
                            x0, y1, z1, r, g, b, a,
                            x1, y1, z1, r, g, b, a,
                        } );

                    if ( ( octree.Exposed & Face.Left ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z1, r, g, b, a,
                            x0, y0, z0, r, g, b, a,
                            x0, y1, z0, r, g, b, a,
                            x0, y1, z1, r, g, b, a,
                        } );

                    if ( ( octree.Exposed & Face.Bottom ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y0, z0, r, g, b, a,
                            x0, y0, z1, r, g, b, a,
                            x1, y0, z1, r, g, b, a,
                            x1, y0, z0, r, g, b, a,
                        } );

                    if ( ( octree.Exposed & Face.Top ) != 0 )
                        verts.AddRange( new float[]
                        {
                            x0, y1, z0, r, g, b, a,
                            x1, y1, z0, r, g, b, a,
                            x1, y1, z1, r, g, b, a,
                            x0, y1, z1, r, g, b, a,
                        } );
                }
            }

            return verts.ToArray();
        }

        public void Render( ShaderProgram shader )
        {
            if ( myVertexDataChanged )
            {
                myVertexBuffer.SetData( FindVertices() );
                myVertexDataChanged = false;
            }

            myVertexBuffer.Render( shader );
        }

        public void Dispose()
        {
            myVertexBuffer.Dispose();
        }
    }
}
