using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Shared
{
    public class OctreeTest : Octree<OctreeTestBlockType>
    {
        public OctreeTest( int size )
            : base( size )
        {

        }

        public OctreeTest( int x, int y, int z, int size )
            : base( x, y, z, size )
        {

        }

        protected OctreeTest( OctreeTest parent, Octant octant )
            : base( parent, octant )
        {

        }

        protected override Octree<OctreeTestBlockType> CreateChild( Octant octant )
        {
            return new OctreeTest( this, octant );
        }

        protected override Face FindSolidFaces()
        {
            if ( Value == OctreeTestBlockType.Empty )
                return Face.None;

            return Face.All;
        }
    }
}
