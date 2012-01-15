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

        public void UpdateNeighbours()
        {
            for ( int i = 1; i < 16; i <<= 1 )
            {
                Face face = (Face) i;
                OctreeTest n = (OctreeTest) FindNeighbour( face );
                if ( n != null )
                {
                    Face opp = Tools.Opposite( face );
                    var enumerator = n.GetEnumerator( Tools.Opposite( opp ) );
                    while ( enumerator.MoveNext() )
                        enumerator.Current.UpdateFace( opp );
                }
            }
        }

        protected override Face FindSolidFaces()
        {
            if ( Value == OctreeTestBlockType.Empty )
                return Face.None;

            return Face.All;
        }

        protected override OctreeNode<OctreeTestBlockType> FindExternalNode( int x, int y, int z, int size )
        {
            if( Chunk != null )
                return Chunk.FindOctree( x, y, z, size );

            return null;
        }
    }
}
