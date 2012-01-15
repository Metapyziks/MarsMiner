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
