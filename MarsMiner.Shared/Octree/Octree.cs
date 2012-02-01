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
    public class Octree<T> : OctreeBranch<T>, IEnumerable<OctreeNode<T>>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public readonly int Size;

        public IOctreeContainer<T> Container;

        public bool HasContainer
        {
            get { return Container != null; }
        }

        public Octree( int x, int y, int z, int size, IOctreeContainer<T> container = null )
        {
            X = x;
            Y = y;
            Z = z;

            Size = size;
        }

        public void SetCuboid( Cuboid cuboid, T value )
        {
            cuboid.X -= X;
            cuboid.Y -= Y;
            cuboid.Z -= Z;

            SetCuboid( Size, cuboid, value );
        }

        public void SetCuboid( int x, int y, int z, int width, int height, int depth, T value )
        {
            SetCuboid( Size, new Cuboid( x - X, y - Y, z - Z, width, height, depth ), value );
        }

        public override bool ShouldMerge()
        {
            return false;
        }

        public override OctreeNode<T> FindNode( int x, int y, int z, int size )
        {
            if ( x < X || y < Y || z < Z || x >= X + Size || y >= Y + Size || z >= Z + Size )
            {
                if ( HasContainer )
                    return Container.FindNode( x, y, z, size );

                return null;
            }

            return FindNode( Size, x - X, y - Y, z - Z, size );
        }

        internal override OctreeNode<T> FindNode( int mSize, int oX, int oY, int oZ, int oSize )
        {
            if ( oX < 0 || oY < 0 || oZ < 0 || oX >= mSize || oY >= mSize || oZ >= mSize )
            {
                if ( HasContainer )
                {
                    int scale = Size / mSize;
                    return Container.FindNode( X + oX * scale, Y + oY * scale, Z + oZ * scale, oSize * scale );
                }

                return null;
            }

            return base.FindNode( mSize, oX, oY, oZ, oSize );
        }

        protected override Cuboid FindDimensionsOfChild( OctreeNode<T> child )
        {
            Octant oct = FindOctantOfChild( child );

            int size = Size >> 1;

            return new Cuboid( X + oct.X * size, Y + oct.Y * size, Z + oct.Z * size, size );
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
            return "Octree Root";
        }
    }
}
