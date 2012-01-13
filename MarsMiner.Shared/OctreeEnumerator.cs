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
        private bool myFirst;

        public readonly Octree<T> Octree;

        public OctreeEnumerator( Octree<T> octree )
        {
            Octree = octree;

            myChild = null;
            myCurOctant = Octant.All[ 0 ];
            myFirst = true;
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
                    if ( myChild != null && myCurOctant.Index == 0 )
                        return false;

                    myChild = new OctreeEnumerator<T>( Octree[ myCurOctant ] );
                    myChild.MoveNext();
                    myCurOctant = myCurOctant.Next;
                }

                return true;
            }

            myFirst = !myFirst;
            return !myFirst;
        }

        public void Reset()
        {
            myChild = null;
            myCurOctant = Octant.All[ 0 ];
        }

        public void Dispose()
        {
            if ( myChild != null )
                myChild.Dispose();
        }
    }
}
