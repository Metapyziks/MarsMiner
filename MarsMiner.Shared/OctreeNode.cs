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

namespace MarsMiner.Shared
{
    public class OctreeNode<T> : IEnumerable<OctreeNode<T>>
    {
        private T myValue;
        private OctreeNode<T>[] myChildren;

        public Octant Octant
        {
            get { return Parent.FindOctantOfChild( this ); }
        }

        public virtual int X
        {
            get { return Parent.X + Octant.X * Size; }
        }
        public virtual int Y
        {
            get { return Parent.Y + Octant.Y * Size; }
        }
        public virtual int Z
        {
            get { return Parent.Z + Octant.Z * Size; }
        }

        public virtual int Size
        {
            get { return Parent.Size >> 1; }
        }

        public virtual Cuboid Cube
        {
            get { return Parent.FindDimensionsOfChild( this ); }
        }

        public readonly OctreeNode<T> Parent;

        public T Value
        {
            get
            {
                if ( HasChildren )
                    throw new InvalidOperationException();

                return myValue;
            }
            set
            {
                Merge( value );
            }
        }

        public bool HasParent
        {
            get { return Parent != null; }
        }

        public bool HasChildren
        {
            get { return myChildren != null; }
        }

        public OctreeNode()
        {

        }

        protected OctreeNode( OctreeNode<T> parent, Octant octant )
        {
            Parent = parent;
            myValue = parent.myValue;
        }

        public OctreeNode<T> this[ Octant octant ]
        {
            get
            {
                if ( !HasChildren )
                    throw new InvalidOperationException();

                return myChildren[ octant.Index ];
            }
        }

        protected Octant FindOctantOfChild( OctreeNode<T> child )
        {
            for ( int i = 0; i < 8; ++i )
                if ( child == myChildren[ i ] )
                    return Octant.All[ i ];

            throw new IndexOutOfRangeException();
        }

        protected virtual Cuboid FindDimensionsOfChild( OctreeNode<T> child )
        {
            Octant oct = FindOctantOfChild( child );
            Cuboid dims = Parent.FindDimensionsOfChild( this );

            int size = dims.Width >> 1;

            return new Cuboid( dims.X + oct.X * size, dims.Y + oct.Y * size, dims.Z + oct.Z * size, size );
        }

        public void Partition()
        {
            if ( !HasChildren )
            {
                myChildren = new OctreeNode<T>[]
                {
                    new OctreeNode<T>( this, Octant.All[ 0 ] ),
                    new OctreeNode<T>( this, Octant.All[ 1 ] ),
                    new OctreeNode<T>( this, Octant.All[ 2 ] ),
                    new OctreeNode<T>( this, Octant.All[ 3 ] ),
                    new OctreeNode<T>( this, Octant.All[ 4 ] ),
                    new OctreeNode<T>( this, Octant.All[ 5 ] ),
                    new OctreeNode<T>( this, Octant.All[ 6 ] ),
                    new OctreeNode<T>( this, Octant.All[ 7 ] )
                };

                myValue = default( T );
            }
        }

        public void Merge( T value )
        {
            if ( HasChildren )
                myChildren = null;
            else if ( myValue.Equals( value ) )
                return;

            myValue = value;

            if ( HasParent && Parent.ShouldMerge() )
                Parent.Merge( Parent.FindMergeValue() );
        }

        protected virtual bool ShouldMerge()
        {
            if ( !HasChildren )
                return false;

            for ( int i = 0; i < 8; ++i )
            {
                if ( myChildren[ i ].HasChildren || ( i > 0
                    && !myChildren[ i ].Value.Equals( myChildren[ 0 ].Value ) ) )
                    return false;
            }

            return true;
        }

        protected virtual T FindMergeValue()
        {
            if ( !HasChildren )
                throw new InvalidOperationException();

            return myChildren[ 0 ].Value;
        }

        public bool IsFaceExposed( Face face, FindSolidFacesDelegate<T> solidCheck )
        {
            OctreeNode<T> n = FindNeighbour( face );

            return n == null || !n.IsFaceSolid( Tools.Opposite( face ), solidCheck );
        }

        public bool IsFaceSolid( Face face, FindSolidFacesDelegate<T> solidCheck )
        {
            if ( !HasChildren )
                return ( solidCheck( Value ) & face ) != 0;

            switch ( face )
            {
                case Face.Left:
                    return
                        myChildren[ 0 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 1 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 2 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 3 ].IsFaceSolid( face, solidCheck );
                case Face.Right:
                    return
                        myChildren[ 4 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 5 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 6 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 7 ].IsFaceSolid( face, solidCheck );
                case Face.Bottom:
                    return
                        myChildren[ 0 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 1 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 4 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 5 ].IsFaceSolid( face, solidCheck );
                case Face.Top:
                    return
                        myChildren[ 2 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 3 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 6 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 7 ].IsFaceSolid( face, solidCheck );
                case Face.Front:
                    return
                        myChildren[ 0 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 2 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 4 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 6 ].IsFaceSolid( face, solidCheck );
                case Face.Back:
                    return
                        myChildren[ 1 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 3 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 5 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 7 ].IsFaceSolid( face, solidCheck );
            }

            return false;
        }

        protected void SetCuboid( int size, Cuboid cuboid, T value )
        {
            if ( !HasChildren && Value.Equals( value ) )
                return;

            if ( cuboid.IsIntersecting( size ) )
            {
                Cuboid i = cuboid.FindIntersection( size );
                if ( i.X == 0 && i.Y == 0 && i.Z == 0
                    && i.Width == i.Height && i.Height == i.Depth && i.Depth == size )
                    Merge( value );
                else if ( i.Volume != 0 )
                {
                    if ( !HasChildren )
                        Partition();

                    int h = size >> 1;

                    foreach ( Octant oct in Octant.All )
                    {
                        Cuboid sub = new Cuboid( i.X - oct.X * h, i.Y - oct.Y * h, i.Z - oct.Z * h,
                            i.Width, i.Height, i.Depth );
                        this[ oct ].SetCuboid( h, sub, value );
                    }
                }
            }
        }

        public virtual OctreeNode<T> FindNode( int x, int y, int z, int size )
        {
            return Parent.FindNode( x, y, z, size );
        }

        protected virtual OctreeNode<T> FindNode( int mSize, int oX, int oY, int oZ, int oSize )
        {
            if ( mSize == oSize && oX == 0 && oY == 0 && oZ == 0 )
                return this;

            if ( oX < 0 || oY < 0 || oZ < 0 || oX >= mSize
                || oY >= mSize || oZ >= mSize )
                return Parent.FindNode( this, mSize, oX, oY, oZ, oSize );

            if ( HasChildren )
            {
                int hs = mSize >> 1;
                int cX = ( oX >= hs ? 1 : 0 );
                int cY = ( oY >= hs ? 1 : 0 );
                int cZ = ( oZ >= hs ? 1 : 0 );
                int child = cX << 2 | cY << 1 | cZ;

                return myChildren[ child ].FindNode( hs, oX - cX * hs, oY - cY * hs, oZ - cZ * hs, oSize );
            }

            return this;
        }

        protected OctreeNode<T> FindNode( OctreeNode<T> child, int mSize, int oX, int oY, int oZ, int oSize )
        {
            Octant oct = FindOctantOfChild( child );
            return FindNode( mSize << 1, oX + oct.X * mSize, oY + oct.Y * mSize, oZ + oct.Z * mSize, oSize );
        }

        protected virtual OctreeNode<T> FindNeighbour( Face face )
        {
            switch ( face )
            {
                case Face.Left:
                    return FindNode( 1, -1, 0, 0, 1 );
                case Face.Right:
                    return FindNode( 1, 1, 0, 0, 1 );
                case Face.Bottom:
                    return FindNode( 1, 0, -1, 0, 1 );
                case Face.Top:
                    return FindNode( 1, 0, 1, 0, 1 );
                case Face.Front:
                    return FindNode( 1, 0, 0, -1, 1 );
                case Face.Back:
                    return FindNode( 1, 0, 0, 1, 1 );
            }

            return null;
        }

        public IEnumerator<OctreeNode<T>> GetEnumerator()
        {
            return new OctreeEnumerator<T>( this );
        }

        public IEnumerator<OctreeNode<T>> GetEnumerator( Face face )
        {
            return new OctreeEnumerator<T>( this, face );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            if ( !HasChildren )
                return Value.ToString();
            else
                return "Octree branch";
        }
    }
}
