﻿using System.Collections.Generic;

using OpenTK.Graphics;

using MarsMiner.Shared;

namespace MarsMiner.Client.Graphics
{
    public enum OctreeTestBlockType : byte
    {
        Empty = 0,
        White = 1,
        Black = 2,
        Red = 3,
        Green = 4,
        Blue = 5,
    }

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

        public readonly Octree<OctreeTestBlockType> Octree;

        public OctreeTestRenderer( Octree<OctreeTestBlockType> octree, ShaderProgram shader )
        {
            Octree = octree;
            myVertexBuffer = new VertexBuffer( shader );
        }

        public void UpdateVertices()
        {
            myVertexBuffer.SetData( FindVertices() );
        }

        public float[] FindVertices()
        {
            List<float> verts = new List<float>();

            foreach ( Octree<OctreeTestBlockType> octree in Octree )
            {
                if ( octree.Value == OctreeTestBlockType.Empty )
                    continue;

                Color4 colour = stBlockColours[ (int) octree.Value ];

                float x0 = octree.X; float x1 = octree.X + octree.Size;
                float y0 = -octree.Y; float y1 = -octree.Y - octree.Size;
                float z0 = octree.Z; float z1 = octree.Z + octree.Size;

                float r = ( octree.X % ( octree.Size * 2 ) ) / 2.0f + 0.5f;
                float g = ( octree.Y % ( octree.Size * 2 ) ) / 2.0f + 0.5f;
                float b = ( octree.Z % ( octree.Size * 2 ) ) / 2.0f + 0.5f;
                float a = colour.A;

                verts.AddRange( new float[]
                {
                    x0, y0, z0, r, g, b, a,
                    x1, y0, z0, r, g, b, a,
                    x1, y1, z0, r, g, b, a,
                    x0, y1, z0, r, g, b, a,
                    
                    x1, y0, z0, r, g, b, a,
                    x1, y0, z1, r, g, b, a,
                    x1, y1, z1, r, g, b, a,
                    x1, y1, z0, r, g, b, a,
                    
                    x1, y0, z1, r, g, b, a,
                    x0, y0, z1, r, g, b, a,
                    x0, y1, z1, r, g, b, a,
                    x1, y1, z1, r, g, b, a,
                    
                    x0, y0, z1, r, g, b, a,
                    x0, y0, z0, r, g, b, a,
                    x0, y1, z0, r, g, b, a,
                    x0, y1, z1, r, g, b, a,

                    x0, y0, z0, r, g, b, a,
                    x0, y0, z1, r, g, b, a,
                    x1, y0, z1, r, g, b, a,
                    x1, y0, z0, r, g, b, a,

                    x0, y1, z0, r, g, b, a,
                    x1, y1, z0, r, g, b, a,
                    x1, y1, z1, r, g, b, a,
                    x0, y1, z1, r, g, b, a,
                } );
            }

            return verts.ToArray();
        }

        public void Render()
        {
            myVertexBuffer.Render();
        }
    }
}