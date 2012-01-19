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

namespace MarsMiner.Shared.Octree
{
    public class OctreeBranch<T> : OctreeNode<T>
    {
        private OctreeNode<T>[] myChildren;

        public OctreeBranch()
        {
            myChildren = new OctreeNode<T>[]
            {
                new OctreeLeaf<T>( this ),
                new OctreeLeaf<T>( this ),
                new OctreeLeaf<T>( this ),
                new OctreeLeaf<T>( this ),
                new OctreeLeaf<T>( this ),
                new OctreeLeaf<T>( this ),
                new OctreeLeaf<T>( this ),
                new OctreeLeaf<T>( this )
            };
        }

        internal OctreeBranch( OctreeBranch<T> parent, T oldValue )
            : base( parent )
        {
            myChildren = new OctreeNode<T>[]
            {
                new OctreeLeaf<T>( this, oldValue ),
                new OctreeLeaf<T>( this, oldValue ),
                new OctreeLeaf<T>( this, oldValue ),
                new OctreeLeaf<T>( this, oldValue ),
                new OctreeLeaf<T>( this, oldValue ),
                new OctreeLeaf<T>( this, oldValue ),
                new OctreeLeaf<T>( this, oldValue ),
                new OctreeLeaf<T>( this, oldValue )
            };
        }

        public OctreeNode<T> this[ Octant octant ]
        {
            get { return myChildren[ octant.Index ]; }
        }

        internal void ReplaceChild( OctreeNode<T> oldChild, OctreeNode<T> newChild )
        {
            for ( int i = 0; i < 8; ++i )
            {
                if ( oldChild == myChildren[ i ] )
                {
                    myChildren[ i ] = newChild;

                    if ( newChild is OctreeLeaf<T> && ShouldMerge() )
                        Merge( ( (OctreeLeaf<T>) newChild ).Value );
                    return;
                }
            }
        }

        public void Merge( T value )
        {
            OctreeLeaf<T> leaf = new OctreeLeaf<T>( Parent, value );
            Parent.ReplaceChild( this, leaf );
        }

        public virtual bool ShouldMerge()
        {
            T lastVal = default(T);

            for ( int i = 0; i < 8; ++i )
            {
                if ( myChildren[ i ] is OctreeBranch<T> )
                    return false;
                
                T value = ( (OctreeLeaf<T>) myChildren[ i ] ).Value;

                if ( i > 0 && !value.Equals( lastVal ) )
                    return false;

                lastVal = value;
            }

            return true;
        }

        public override bool IsFaceSolid( Face face, FindSolidFacesDelegate<T> solidCheck )
        {
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

        internal override void SetCuboid( int size, Cuboid cuboid, T value )
        {
            if ( cuboid.IsIntersecting( size ) )
            {
                Cuboid i = cuboid.FindIntersection( size );
                if ( i.X == 0 && i.Y == 0 && i.Z == 0
                    && i.Width == i.Height && i.Height == i.Depth && i.Depth == size && ShouldMerge() )
                    Merge( value );
                else if ( i.Volume != 0 )
                {
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

        internal override OctreeNode<T> FindInnerNode( int mSize, int oX, int oY, int oZ, int oSize )
        {
            int hs = mSize >> 1;
            int cX = ( oX >= hs ? 1 : 0 );
            int cY = ( oY >= hs ? 1 : 0 );
            int cZ = ( oZ >= hs ? 1 : 0 );
            int child = cX << 2 | cY << 1 | cZ;

            return myChildren[ child ].FindNode( hs, oX - cX * hs, oY - cY * hs, oZ - cZ * hs, oSize );
        }

        internal OctreeNode<T> FindNode( OctreeNode<T> child, int mSize, int oX, int oY, int oZ, int oSize )
        {
            Octant oct = FindOctantOfChild( child );
            return FindNode( mSize << 1, oX + oct.X * mSize, oY + oct.Y * mSize, oZ + oct.Z * mSize, oSize );
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

        public override string ToString()
        {
            return "Octree Branch";
        }
    }
}
