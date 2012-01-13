using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                { All[ 0 ], All[ 1 ] },
                { All[ 2 ], All[ 3 ] }
            },
            {
                { All[ 4 ], All[ 5 ] },
                { All[ 6 ], All[ 7 ] }
            }
        };

        public static Octant FromFaces( Face faces )
        {
            bool right = ( faces & Face.Right ) != 0;
            bool top = ( faces & Face.Top ) != 0;
            bool back = ( faces & Face.Back ) != 0;

            return XYZ[ right ? 1 : 0, top ? 1 : 0, back ? 1 : 0 ];
        }

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
}
