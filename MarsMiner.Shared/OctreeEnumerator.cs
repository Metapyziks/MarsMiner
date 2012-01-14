using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Shared
{
    public class OctreeEnumerator<T> : IEnumerator<Octree<T>>
    {
        private OctreeEnumerator<T> myChild;
        private Octant myCurOctant;
        private byte myPassed;

        public readonly Octree<T> Octree;
        public readonly Face Face;

        public OctreeEnumerator( Octree<T> octree )
            : this( octree, Face.None )
        {

        }

        public OctreeEnumerator( Octree<T> octree, Face face )
        {
            Octree = octree;
            Face = face;

            Reset();
        }

        public Octree<T> Current
        {
            get
            {
                if ( Octree.HasChildren )
                    return myChild.Current;

                return Octree;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if ( Octree.HasChildren )
            {
                if ( myChild == null || !myChild.MoveNext() )
                {
                    if ( myPassed++ == ( Face == Face.None ? 8 : 4 ) )
                        return false;

                    myChild = new OctreeEnumerator<T>( Octree[ myCurOctant ], Face );
                    myChild.MoveNext();

                    do
                        myCurOctant = myCurOctant.Next;
                    while ( ( myCurOctant.Faces & Face ) != Face );
                }

                return true;
            }

            return myPassed++ == 0;
        }

        public void Reset()
        {
            myChild = null;
            myCurOctant = Octant.All[ 0 ];
            myPassed = 0;

            while ( ( myCurOctant.Faces & Face ) != Face )
                myCurOctant = myCurOctant.Next;
        }

        public void Dispose()
        {
            if ( myChild != null )
                myChild.Dispose();
        }
    }
}
