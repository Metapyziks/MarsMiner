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

namespace MarsMiner.Shared.Octree
{
    public class OctreeLeaf<T> : OctreeNode<T>
    {
        private T myValue;

        public T Value
        {
            get { return myValue; }
            set
            {
                myValue = value;

                if ( Parent.ShouldMerge() )
                    Parent.Merge( value );
            }
        }

        internal OctreeLeaf( OctreeBranch<T> parent )
            : base( parent )
        {
            myValue = default(T);
        }

        internal OctreeLeaf( OctreeBranch<T> parent, T value )
            : base( parent )
        {
            myValue = value;
        }

        public OctreeBranch<T> Partition()
        {
            OctreeBranch<T> branch = new OctreeBranch<T>( Parent, Value );
            Parent.ReplaceChild( this, branch );
            return branch;
        }

        public override bool IsFaceSolid( Face face, FindSolidFacesDelegate<T> solidCheck )
        {
            return solidCheck( Value ).HasFace( face );
        }

        internal override void SetCuboid( int size, Cuboid cuboid, T value )
        {
            if( Value.Equals( value ) )
                return;

            if ( cuboid.IsIntersecting( size ) )
            {
                Cuboid i = cuboid.FindIntersection( size );
                if ( i.X == 0 && i.Y == 0 && i.Z == 0
                    && i.Width == i.Height && i.Height == i.Depth && i.Depth == size )
                    Value = value;
                else if ( i.Volume != 0 )
                    Partition().SetCuboid( size, cuboid, value );
            }
        }

        internal override OctreeNode<T> FindInnerNode( int mSize, int oX, int oY, int oZ, int oSize )
        {
            return this;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
