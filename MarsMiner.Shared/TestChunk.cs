using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Shared
{
    public class TestChunk
    {
        public const int ChunkSize = 64;
        public const int ChunkHeight = 256;

        public OctreeTest[] Octrees { get; private set; }

        public readonly TestWorld World;
        public readonly int X;
        public readonly int Z;

        public int CenterX
        {
            get { return X + ChunkSize / 2; }
        }
        public int CenterZ
        {
            get { return Z + ChunkSize / 2; }
        }

        public bool Loaded { get; private set; }

        public TestChunk( TestWorld world, int x, int z )
        {
            World = world;

            X = x;
            Z = z;

            Loaded = false;
        }

        public void Generate()
        {
            Loaded = false;

            int octrees = ChunkHeight / ChunkSize;
            Octrees = new OctreeTest[ octrees ];

            int dist2 = CenterX * CenterX + CenterZ * CenterZ;

            int res = (
                dist2 < 128 * 128 ? 1 :
                dist2 < 256 * 256 ? 2 :
                dist2 < 512 * 512 ? 4 : 8 );

            for ( int i = 0; i < octrees; ++i )
                Octrees[ i ] = World.Generator.Generate( X, i * ChunkSize, Z, ChunkSize, res );

            Loaded = true;
        }
    }
}
