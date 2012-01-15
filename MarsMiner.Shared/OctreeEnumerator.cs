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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Shared
{
    public class OctreeEnumerator<T> : IEnumerator<OctreeNode<T>>
    {
        private OctreeEnumerator<T> myChild;
        private Octant myCurOctant;
        private byte myPassed;

        public readonly OctreeNode<T> Octree;
        public readonly Face Face;

        public OctreeEnumerator( OctreeNode<T> octree )
            : this( octree, Face.None )
        {

        }

        public OctreeEnumerator( OctreeNode<T> octree, Face face )
        {
            Octree = octree;
            Face = face;

            Reset();
        }

        public OctreeNode<T> Current
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
