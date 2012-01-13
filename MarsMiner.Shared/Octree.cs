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
        private Face mySolidity;

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public readonly int Size;

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
            mySolidity = Face.None;
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

        protected virtual Face FindSolidFaces( T value )
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
                }
            }
        }

        public void SetCuboid( int x, int y, int z, int width, int height, int depth, T value )
        {
            SetCuboid( new Cuboid( x, y, z, width, height, depth ), value );
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
