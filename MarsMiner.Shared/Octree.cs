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

    public class Octree<T> : IEnumerable<Octree<T>>
    {
        private T myValue;
        private Octree<T>[] myChildren;
        private Face myChangedFaces;
        private Face myExposed;

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public readonly int Size;

        public int Left
        {
            get { return X; }
        }
        public int Bottom
        {
            get { return Y; }
        }
        public int Front
        {
            get { return Z; }
        }
        public int Right
        {
            get { return X + Size; }
        }
        public int Top
        {
            get { return Y + Size; }
        }
        public int Back
        {
            get { return Z + Size; }
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

        public Cuboid Cube
        {
            get { return new Cuboid( X, Y, Z, Size, Size, Size ); }
        }

        public readonly Octree<T> Parent;

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

        public Octree( int size )
        {
            Size = size;

            Solidity = FindSolidFaces();

            myChangedFaces = Face.All;
        }

        public Octree( int x, int y, int z, int size )
            : this( size )
        {
            X = x;
            Y = y;
            Z = z;
        }

        protected Octree( Octree<T> parent, Octant octant )
            : this( parent.Size / 2 )
        {
            Parent = parent;

            X = Parent.X + octant.X * Size;
            Y = Parent.Y + octant.Y * Size;
            Z = Parent.Z + octant.Z * Size;

            myValue = parent.myValue;
        }

        public Octree<T> this[ Octant octant ]
        {
            get
            {
                if ( !HasChildren )
                    throw new InvalidOperationException();

                return myChildren[ octant.Index ];
            }
        }

        public void Partition()
        {
            if ( !HasChildren )
            {
                myChildren = new Octree<T>[]
                {
                    CreateChild( Octant.All[ 0 ] ),
                    CreateChild( Octant.All[ 1 ] ),
                    CreateChild( Octant.All[ 2 ] ),
                    CreateChild( Octant.All[ 3 ] ),
                    CreateChild( Octant.All[ 4 ] ),
                    CreateChild( Octant.All[ 5 ] ),
                    CreateChild( Octant.All[ 6 ] ),
                    CreateChild( Octant.All[ 7 ] )
                };

                myValue = default( T );
            }
        }

        protected virtual Octree<T> CreateChild( Octant octant )
        {
            return new Octree<T>( this, octant );
        }

        public void Merge( T value )
        {
            myValue = value;

            if ( HasChildren )
                myChildren = null;

            if ( HasParent && Parent.ShouldMerge() )
                Parent.Merge( Parent.FindMergeValue() );
            else
            {
                Face diff = Solidity;
                Solidity = FindSolidFaces();
                diff ^= Solidity;

                Octree<T> n;

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

        private void UpdateExposedness()
        {
            for ( int i = 1; i < 64; i <<= 1 )
            {
                Face face = (Face) i;
                if ( ( myChangedFaces & face ) != 0 )
                {
                    Octree<T> n = FindNeighbour( face );
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

        public void SetCuboid( Cuboid cuboid, T value )
        {
            if ( !HasChildren && Value.Equals( value ) )
                return;

            Cuboid cube = Cube;
            if ( cube.IsIntersecting( cuboid ) )
            {
                Cuboid intersection = cube.FindIntersection( cuboid );
                if ( intersection.Equals( cube ) )
                    Merge( value );
                else if ( intersection.Volume != 0 )
                {
                    if( !HasChildren )
                        Partition();

                    foreach ( Octree<T> child in myChildren )
                        child.SetCuboid( intersection, value );

                    UpdateSolidity();
                }
            }
        }

        public void SetCuboid( int x, int y, int z, int width, int height, int depth, T value )
        {
            SetCuboid( new Cuboid( x, y, z, width, height, depth ), value );
        }

        public Octree<T> FindOctree( int x, int y, int z, int size )
        {
            if ( size == Size && x == X && y == Y && z == Z )
                return this;

            if ( x < Left || y < Bottom || z < Front || x >= Right || y >= Top || z >= Back )
            {
                if ( HasParent )
                    return Parent.FindOctree( x, y, z, size );

                return FindExternalOctree( x, y, z, size );
            }

            if ( HasChildren )
            {
                int hs = Size >> 1;
                int child = ( x >= X + hs ? 4 : 0 ) | ( y >= Y + hs ? 2 : 0 ) | ( z >= Z + hs ? 1 : 0 );

                return myChildren[ child ].FindOctree( x, y, z, size );
            }

            return this;
        }

        public Octree<T> FindNeighbour( Face face )
        {
            int x = X, y = Y, z = Z, size = Size;

            switch ( face )
            {
                case Face.Left:
                    x -= Size; break;
                case Face.Right:
                    x += Size; break;
                case Face.Bottom:
                    y -= Size; break;
                case Face.Top:
                    y += Size; break;
                case Face.Front:
                    z -= Size; break;
                case Face.Back:
                    z += Size; break;
            }

            if ( HasParent )
                return Parent.FindOctree( x, y, z, size );

            return FindExternalOctree( x, y, z, size );
        }

        protected virtual Octree<T> FindExternalOctree( int x, int y, int z, int size )
        {
            return null;
        }

        public IEnumerator<Octree<T>> GetEnumerator()
        {
            return new OctreeEnumerator<T>( this );
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
