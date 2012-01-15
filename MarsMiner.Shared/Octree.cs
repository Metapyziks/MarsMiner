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

namespace MarsMiner.Shared
{
    public class Octree<T> : OctreeNode<T>
    {
        private readonly int myX;
        private readonly int myY;
        private readonly int myZ;

        private readonly int mySize;

        public override int X
        {
	        get { return myX; }
        }
        public override int Y
        {
	        get { return myY; }
        }
        public override int Z
        {
	        get { return myZ; }
        }
        
        public override int Size
        {
	        get { return mySize; }
        }

        public override Cuboid Cube
        {
            get { return new Cuboid( X, Y, Z, Size ); }
        }

        public Octree( int x, int y, int z, int size )
        {
            myX = x;
            myY = y;
            myZ = z;

            mySize = size;
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

        public override OctreeNode<T> FindNode( int x, int y, int z, int size )
        {
            if ( x < X || y < Y || z < Z || x >= X + Size || y >= Y + Size || z >= Z + Size )
                return FindExternalNode( x, y, z, size );

            return FindNode( Size, x - X, y - Y, z - Z, size );
        }

        protected override OctreeNode<T> FindNode( int mSize, int oX, int oY, int oZ, int oSize )
        {
            if ( oX < 0 || oY < 0 || oZ < 0 || oX >= mSize || oY >= mSize || oZ >= mSize )
            {
                int scale = Size / mSize;
                return FindExternalNode( X + oX * scale, Y + oY * scale, Z + oZ * scale, oSize * scale );
            }

            return base.FindNode( mSize, oX, oY, oZ, oSize );
        }

        protected virtual OctreeNode<T> FindExternalNode( int x, int y, int z, int size )
        {
            return null;
        }

        protected override Cuboid FindDimensionsOfChild( OctreeNode<T> child )
        {
            Octant oct = FindOctantOfChild( child );

            int size = Size >> 1;

            return new Cuboid( X + oct.X * size, Y + oct.Y * size, Z + oct.Z * size, size );
        }
    }
}
