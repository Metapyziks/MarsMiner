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

using System.Collections.Generic;

namespace MarsMiner.Shared.Octree
{
    public class OctreeEnumerator<T> : IEnumerator<OctreeNode<T>>
    {
        private Octree<T> myOriginal;
        private Octant[] myValidOctants;
        private Stack<int> myStack;
        private int myCurOctIndex;

        public readonly Face Face;

        public OctreeNode<T> Node { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public int Size { get; private set; }

        public OctreeNode<T> Current
        {
            get { return Node; }
        }

        public OctreeEnumerator( Octree<T> octree )
            : this( octree, Face.None )
        {

        }

        public OctreeEnumerator( Octree<T> octree, Face face )
        {
            myOriginal = octree;
            Face = face;

            if ( Face.HasNone )
                myValidOctants = Octant.All;
            else
            {
                for ( int i = 0, j = 0; i < 8; ++i )
                {
                    if ( Octant.All[ i ].Faces.HasFace( face ) )
                        myValidOctants[ j++ ] = Octant.All[ i ];
                }
            }

            Reset();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if ( Node == null )
                Node = myOriginal;
            else
            {
                while ( !Node.HasChildren || myCurOctIndex == myValidOctants.Length )
                {
                    if ( !Node.HasParent )
                        return false;

                    Node = Node.Parent;
                    myCurOctIndex = myStack.Pop() + 1;

                    X ^= ( X & Size );
                    Y ^= ( Y & Size );
                    Z ^= ( Z & Size );

                    Size <<= 1;
                }
            }

            while ( Node.HasChildren )
            {
                Octant oct = myValidOctants[ myCurOctIndex ];
                Node = Node[ oct ];
                myStack.Push( myCurOctIndex );
                myCurOctIndex = 0;

                Size >>= 1;

                X += oct.X * Size;
                Y += oct.Y * Size;
                Z += oct.Z * Size;
            }

            return true;
        }

        public void Reset()
        {
            Node = null;

            X = myOriginal.X;
            Y = myOriginal.Y;
            Z = myOriginal.Z;

            Size = myOriginal.Size;
            myStack = new Stack<int>();
            myCurOctIndex = 0;
        }

        public void Dispose()
        {
            return;
        }
    }
}
