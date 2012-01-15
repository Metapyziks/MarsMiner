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

using System;

namespace MarsMiner.Shared
{
    public class TestChunk
    {
        public const int ChunkSize = 256;
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

        public OctreeNode<OctreeTestBlockType> FindOctree( int x, int y, int z, int size )
        {
            if ( x < X || x >= X + ChunkSize || z < Z || z >= Z + ChunkSize )
                return World.FindOctree( x, y, z, size );

            if ( y < 0 || y >= ChunkHeight || !Loaded )
                return null;

            return Octrees[ y / ChunkSize ].FindNode( x, y, z, size );
        }
    }
}
