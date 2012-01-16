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

namespace MarsMiner.Shared.Octree
{
    public class OctreeNode<T> : IOctreeContainer<T>
    {
        private T myValue;
        private OctreeNode<T>[] myChildren;
        
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

            return n == null || !n.IsFaceSolid( face.Opposite, solidCheck );
        }

        public bool IsFaceSolid( Face face, FindSolidFacesDelegate<T> solidCheck )
        {
            if ( !HasChildren )
                return solidCheck( Value ).HasFace( face );

            switch ( face.Index )
            {
                case Face.LeftIndex:
                    return
                        myChildren[ 0 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 1 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 2 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 3 ].IsFaceSolid( face, solidCheck );
                case Face.RightIndex:
                    return
                        myChildren[ 4 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 5 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 6 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 7 ].IsFaceSolid( face, solidCheck );
                case Face.BottomIndex:
                    return
                        myChildren[ 0 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 1 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 4 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 5 ].IsFaceSolid( face, solidCheck );
                case Face.TopIndex:
                    return
                        myChildren[ 2 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 3 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 6 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 7 ].IsFaceSolid( face, solidCheck );
                case Face.FrontIndex:
                    return
                        myChildren[ 0 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 2 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 4 ].IsFaceSolid( face, solidCheck ) &&
                        myChildren[ 6 ].IsFaceSolid( face, solidCheck );
                case Face.BackIndex:
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
            switch ( face.Index )
            {
                case Face.LeftIndex:
                    return FindNode( 1, -1, 0, 0, 1 );
                case Face.RightIndex:
                    return FindNode( 1, 1, 0, 0, 1 );
                case Face.BottomIndex:
                    return FindNode( 1, 0, -1, 0, 1 );
                case Face.TopIndex:
                    return FindNode( 1, 0, 1, 0, 1 );
                case Face.FrontIndex:
                    return FindNode( 1, 0, 0, -1, 1 );
                case Face.BackIndex:
                    return FindNode( 1, 0, 0, 1, 1 );
            }

            return null;
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
