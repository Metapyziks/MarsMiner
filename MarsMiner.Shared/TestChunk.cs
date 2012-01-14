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

            int dist = Math.Max( Math.Abs( CenterX ), Math.Abs( CenterZ ) );

            int res = 
                dist < 128 ? 1 :
                dist < 256 ? 2 :
                dist < 512 ? 4 : 8 ;

            for ( int i = 0; i < octrees; ++i )
            {
                OctreeTest newTree = World.Generator.Generate( X, i * ChunkSize, Z, ChunkSize, res );
                newTree.Chunk = this;
                Octrees[ i ] = newTree;
            }

            Loaded = true;
        }

        public void UpdateNeighbours()
        {
            for ( int i = 0; i < Octrees.Length; ++i )
                Octrees[ i ].UpdateNeighbours();
        }

        public OctreeTest FindOctree( int x, int y, int z, int size )
        {
            if ( x < X || x >= X + ChunkSize || z < Z || z >= Z + ChunkSize )
                return World.FindOctree( x, y, z, size );

            if ( y < 0 || y >= ChunkHeight || !Loaded )
                return null;

            return (OctreeTest) Octrees[ y / ChunkSize ].FindOctree( x, y, z, size );
        }
    }
}
