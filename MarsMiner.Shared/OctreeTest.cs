using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Shared
{
    public class OctreeTest : Octree<OctreeTestBlockType>
    {
        public TestChunk Chunk;

        public OctreeTest( int x, int y, int z, int size )
            : base( x, y, z, size )
        {

        }

        protected override OctreeNode<OctreeTestBlockType> FindExternalNode( int x, int y, int z, int size )
        {
            if( Chunk != null )
                return Chunk.FindOctree( x, y, z, size );

            return null;
        }
    }
}
