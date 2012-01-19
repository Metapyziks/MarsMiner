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
    public class OctreeNode<T> : IOctreeContainer<T>
    {        
        public readonly OctreeBranch<T> Parent;

        public OctreeNode()
        {

        }

        internal OctreeNode( OctreeBranch<T> parent )
        {
            Parent = parent;
        }

        public bool IsFaceExposed( Face face, FindSolidFacesDelegate<T> solidCheck )
        {
            OctreeNode<T> n = FindNeighbour( face );

            return n == null || !n.IsFaceSolid( face.Opposite, solidCheck );
        }

        public virtual bool IsFaceSolid( Face face, FindSolidFacesDelegate<T> solidCheck )
        {
            throw new NotImplementedException();
        }

        internal virtual void SetCuboid( int size, Cuboid cuboid, T value )
        {
            throw new NotImplementedException();
        }

        public virtual OctreeNode<T> FindNode( int x, int y, int z, int size )
        {
            return Parent.FindNode( x, y, z, size );
        }

        internal virtual OctreeNode<T> FindNode( int mSize, int oX, int oY, int oZ, int oSize )
        {
            if ( mSize == oSize && oX == 0 && oY == 0 && oZ == 0 )
                return this;

            if ( oX < 0 || oY < 0 || oZ < 0 || oX >= mSize
                || oY >= mSize || oZ >= mSize )
                return Parent.FindNode( this, mSize, oX, oY, oZ, oSize );

            return FindInnerNode( mSize, oX, oY, oZ, oSize );
        }

        internal virtual OctreeNode<T> FindInnerNode( int mSize, int oX, int oY, int oZ, int oSize )
        {
            throw new NotImplementedException();
        }

        public virtual OctreeNode<T> FindNeighbour( Face face )
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
            return "Octree Node";
        }
    }
}
