using System;
using System.Collections.Generic;

namespace MarsMiner.Shared
{
    public enum Face : byte
    {
        None    = 0,
        All     = 63,

        Front   = 1,
        Right   = 2,
        Back    = 4,
        Left    = 8,
        Top     = 16,
        Bottom  = 32
    }

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

            return FindNode( X, Y, Z, Size, x, y, z, size );
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

    public delegate Face FindSolidFacesDelegate<T>( T value );

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
            for( int i = 0; i < 8; ++ i )
                if( child == myChildren[ i ] )
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

        protected OctreeNode<T> FindNode( int mX, int mY, int mZ, int mSize, int oX, int oY, int oZ, int oSize )
        {
            if ( mSize == oSize && mX == oX && mY == oY && mZ == oZ )
                return this;

            if ( oX < mX || oY < mY || oZ < mZ || oX >= mX + mSize
                || oY >= mY + mSize || oZ >= mZ + mSize )
            {
                mX ^= ( mX & mSize );
                mY ^= ( mY & mSize );
                mZ ^= ( mZ & mSize );
                mSize <<= 1;
                return Parent.FindNode( mX, mY, mZ, mSize, oX, oY, oZ, oSize );
            }

            if ( HasChildren )
            {
                int hs = mSize >> 1;
                int cX = ( oX >= mX + hs ? 1 : 0 );
                int cY = ( oY >= mY + hs ? 1 : 0 );
                int cZ = ( oZ >= mZ + hs ? 1 : 0 );
                int child = cX << 2 | cY << 1 | cZ;

                cX = mX + cX * hs;
                cY = mY + cY * hs;
                cZ = mZ + cZ * hs;

                return myChildren[ child ].FindNode( cX, cY, cZ, hs, oX, oY, oZ, oSize );
            }

            return this;
        }

        protected virtual OctreeNode<T> FindNeighbour( Face face )
        {
            Cuboid dims = Cube;

            int size = dims.Width;

            switch ( face )
            {
                case Face.Left:
                    dims.X -= size; break;
                case Face.Right:
                    dims.X += size; break;
                case Face.Bottom:
                    dims.Y -= size; break;
                case Face.Top:
                    dims.Y += size; break;
                case Face.Front:
                    dims.Z -= size; break;
                case Face.Back:
                    dims.Z += size; break;
            }

            return FindNode( dims.X, dims.Y, dims.Z, size );
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
