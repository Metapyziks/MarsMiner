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

        public Octree( int x, int y, int z, int size )
        {
            myX = x;
            myY = y;
            myZ = z;

            mySize = size;
        }

        public void SetCuboid( Cuboid cuboid, T value )
        {
            SetCuboid( X, Y, Z, Size, cuboid, value );
        }

        public void SetCuboid( int x, int y, int z, int width, int height, int depth, T value )
        {
            SetCuboid( X, Y, Z, Size, new Cuboid( x, y, z, width, height, depth ), value );
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

        protected override OctreeNode<T> FindNeighbour( Face face )
        {
            return null;
        }
    }

    public class OctreeNode<T> : IEnumerable<OctreeNode<T>>
    {
        private T myValue;
        private OctreeNode<T>[] myChildren;
        private Face myChangedFaces;
        private Face myExposed;

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

        public Face Solidity { get; private set; }
        public Face Exposed
        {
            get
            {
                if ( myChangedFaces != Face.None )
                    UpdateExposedness();

                return myExposed;
            }
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
            Solidity = FindSolidFaces();
            myChangedFaces = Face.All;
        }

        protected OctreeNode( OctreeNode<T> parent, Octant octant )
            : this()
        {
            Parent = parent;
            myValue = parent.myValue;
            Solidity = parent.Solidity;
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
            else
            {
                Face diff = Solidity;
                Solidity = FindSolidFaces();
                diff ^= Solidity;

                OctreeNode<T> n;

                for ( int i = 1; i < 64; i <<= 1 )
                {
                    Face face = (Face) i;
                    if ( ( diff & face ) != 0 )
                    {
                        n = FindNeighbour( face );
                        if ( n != null )
                            n.myChangedFaces |= Tools.Opposite( face );
                    }
                }
            }
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

        private void UpdateSolidity()
        {
            Face lbf = myChildren[ 0 ].Solidity;
            Face lbb = myChildren[ 1 ].Solidity;
            Face ltf = myChildren[ 2 ].Solidity;
            Face ltb = myChildren[ 3 ].Solidity;
            Face rbf = myChildren[ 4 ].Solidity;
            Face rbb = myChildren[ 5 ].Solidity;
            Face rtf = myChildren[ 6 ].Solidity;
            Face rtb = myChildren[ 7 ].Solidity;

            Solidity =
                ( Face.Left   & lbf & lbb & ltf & ltb ) |
                ( Face.Right  & rbf & rbb & rtf & rtb ) |
                ( Face.Bottom & lbf & lbb & rbf & rbb ) |
                ( Face.Top    & ltf & ltb & rtf & rtb ) |
                ( Face.Front  & lbf & ltf & rbf & rtf ) |
                ( Face.Back   & lbb & ltb & rbb & rtb );
        }

        public void UpdateFace( Face face )
        {
            myChangedFaces |= face & Solidity;
        }

        private void UpdateExposedness()
        {
            for ( int i = 1; i < 64; i <<= 1 )
            {
                Face face = (Face) i;
                if ( ( myChangedFaces & face ) != 0 )
                {
                    OctreeNode<T> n = FindNeighbour( face );
                    bool exposed = ( n == null )
                        || ( n.Solidity & Tools.Opposite( face ) ) == 0;

                    if ( ( ( myExposed & face ) != 0 ) != exposed )
                        myExposed ^= face;
                }
            }

            myChangedFaces = Face.None;
        }

        protected virtual Face FindSolidFaces()
        {
            return Face.All;
        }

        protected void SetCuboid( int x, int y, int z, int size, Cuboid cuboid, T value )
        {
            if ( !HasChildren && Value.Equals( value ) )
                return;

            if ( cuboid.IsIntersecting( x, y, z, size ) )
            {
                Cuboid i = cuboid.FindIntersection( x, y, z, size );
                if ( i.X == x && i.Y == y && i.Z == z
                    && i.Width == i.Height && i.Height == i.Depth && i.Depth == size )
                    Merge( value );
                else if ( i.Volume != 0 )
                {
                    if ( !HasChildren )
                        Partition();

                    int h = size >> 1;

                    foreach ( Octant oct in Octant.All )
                        this[ oct ].SetCuboid( x + oct.X * h, y + oct.Y * h, z + oct.Z * h, h, i, value );

                    if ( HasChildren )
                        UpdateSolidity();
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
            Cuboid dims = Parent.FindDimensionsOfChild( this );

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

            return Parent.FindNode( dims.X, dims.Y, dims.Z, size );
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
