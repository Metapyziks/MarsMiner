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
