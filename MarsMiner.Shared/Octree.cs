using System;
using System.Collections.Generic;

namespace MarsMiner.Shared
{
    public struct Octant
    {
        public static readonly Octant[] All = new Octant[]
        {
            new Octant( 0, 0, 0 ),
            new Octant( 0, 0, 1 ),
            new Octant( 0, 1, 0 ),
            new Octant( 0, 1, 1 ),
            new Octant( 1, 0, 0 ),
            new Octant( 1, 0, 1 ),
            new Octant( 1, 1, 0 ),
            new Octant( 1, 1, 1 ),
        };

        public static readonly Octant[,,] XYZ = new Octant[ , , ]
        {
            {
                {
                    All[ 0 ],
                    All[ 1 ],
                },
                {
                    All[ 2 ],
                    All[ 3 ],
                }
            },
            {
                {
                    All[ 4 ],
                    All[ 5 ],
                },
                {
                    All[ 6 ],
                    All[ 7 ],
                }
            }
        };

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public readonly int Index;

        public Octant Next
        {
            get { return All[ ( Index + 1 ) % 8 ]; }
        }

        private Octant( int x, int y, int z )
        {
            X = x;
            Y = y;
            Z = z;

            Index = x << 2 | y << 1 | z;
        }

        public override bool Equals( object obj )
        {
            return obj is Octant && ( (Octant) obj ).Index == Index;
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }

    public class Octree<T> : IEnumerable<Octree<T>>
    {
        private T myValue;
        private Octree<T>[] myChildren;

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public readonly int Size;

        public Cuboid Cube
        {
            get
            {
                return new Cuboid( X, Y, Z, Size, Size, Size );
            }
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

        public Octree( int size, int x, int y, int z )
        {
            Size = size;
        }

        private Octree( Octree<T> parent, Octant octant )
            : this( parent.Size / 2 )
        {
            Parent = parent;

            X = Parent.X + octant.X * Parent.Size;
            Y = Parent.Y + octant.Y * Parent.Size;
            Z = Parent.Z + octant.Z * Parent.Size;
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
                    new Octree<T>( this, Octant.All[ 0 ] ),
                    new Octree<T>( this, Octant.All[ 1 ] ),
                    new Octree<T>( this, Octant.All[ 2 ] ),
                    new Octree<T>( this, Octant.All[ 3 ] ),
                    new Octree<T>( this, Octant.All[ 4 ] ),
                    new Octree<T>( this, Octant.All[ 5 ] ),
                    new Octree<T>( this, Octant.All[ 6 ] ),
                    new Octree<T>( this, Octant.All[ 7 ] )
                };

                myValue = default( T );
            }
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

        public void SetCuboid( Cuboid cuboid, T value )
        {
            Cuboid cube = Cube;
            if ( cube.IsIntersecting( cuboid ) )
            {
                Cuboid intersection = cube.FindIntersection( cuboid );
                if ( intersection.Equals( cuboid ) )
                    Merge( value );
                else if ( intersection.Volume != 0 )
                {
                    Partition();
                    foreach ( Octree<T> child in myChildren )
                        child.SetCuboid( intersection, value );
                }
            }
        }

        public IEnumerator<Octree<T>> GetEnumerator()
        {
            return new OctreeEnumerator<T>( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class OctreeEnumerator<T> : IEnumerator<Octree<T>>
    {
        private OctreeEnumerator<T> myChild;
        private Octant myCurOctant;
        private bool myFirst;

        public readonly Octree<T> Octree;

        public OctreeEnumerator( Octree<T> octree )
        {
            Octree = octree;

            myChild = null;
            myCurOctant = Octant.All[ 0 ];
            myFirst = true;
        }

        public Octree<T> Current
        {
            get
            {
                if ( Octree.HasChildren )
                    return myChild.Current;

                return Octree;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if ( Octree.HasChildren )
            {
                if ( myChild != null && myCurOctant.Index == 0 )
                    return false;

                myChild = new OctreeEnumerator<T>( Octree[ myCurOctant ] );
                myCurOctant = myCurOctant.Next;

                return true;
            }

            myFirst = !myFirst;
            return !myFirst;
        }

        public void Reset()
        {
            myChild = null;
            myCurOctant = Octant.All[ 0 ];
        }

        public void Dispose()
        {
            if ( myChild != null )
                myChild.Dispose();
        }
    }
}
