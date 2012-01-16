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
    public struct Octant
    {
        private static readonly Face[] stFaces = new Face[]
        {
            Face.Left | Face.Bottom | Face.Front,
            Face.Left | Face.Bottom | Face.Back,
            Face.Left | Face.Top | Face.Front,
            Face.Left | Face.Top | Face.Back,
            Face.Right | Face.Bottom | Face.Front,
            Face.Right | Face.Bottom | Face.Back,
            Face.Right | Face.Top | Face.Front,
            Face.Right | Face.Top | Face.Back,
        };

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
            return XYZ[ faces.HasRight ? 1 : 0, faces.HasTop ? 1 : 0, faces.HasBack ? 1 : 0 ];
        }

        public int X
        {
            get { return Index >> 2; }
        }
        public int Y
        {
            get { return ( Index >> 1 ) % 2; }
        }
        public int Z
        {
            get { return Index % 2; }
        }

        public readonly int Index;

        public Octant Next
        {
            get { return All[ ( Index + 1 ) % 8 ]; }
        }

        public Face Faces
        {
            get { return stFaces[ Index ]; }
        }

        private Octant( int x, int y, int z )
        {
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
