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
