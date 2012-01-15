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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Shared
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
            bool right = ( faces & Face.Right ) != 0;
            bool top = ( faces & Face.Top ) != 0;
            bool back = ( faces & Face.Back ) != 0;

            return XYZ[ right ? 1 : 0, top ? 1 : 0, back ? 1 : 0 ];
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
