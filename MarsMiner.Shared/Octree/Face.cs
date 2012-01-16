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
    public struct Face
    {
        public const int LeftIndex      = 0;
        public const int BottomIndex    = 1;
        public const int FrontIndex     = 2;
        public const int RightIndex     = 3;
        public const int TopIndex       = 4;
        public const int BackIndex      = 5;

        public static readonly Face None    = new Face( 0 );
        public static readonly Face Left    = new Face( 1 << LeftIndex );
        public static readonly Face Bottom  = new Face( 1 << BottomIndex );
        public static readonly Face Front   = new Face( 1 << FrontIndex );
        public static readonly Face Right   = new Face( 1 << RightIndex );
        public static readonly Face Top     = new Face( 1 << TopIndex );
        public static readonly Face Back    = new Face( 1 << BackIndex );
        public static readonly Face All     = new Face( 63 );

        public static Face operator &( Face f0, Face f1 )
        {
            return new Face( (byte)( f0.myBitmap & f1.myBitmap ) );
        }
        public static Face operator |( Face f0, Face f1 )
        {
            return new Face( (byte) ( f0.myBitmap | f1.myBitmap ) );
        }
        public static Face operator ^( Face f0, Face f1 )
        {
            return new Face( (byte) ( f0.myBitmap ^ f1.myBitmap ) );
        }

        private byte myBitmap;

        public bool HasNone
        {
            get { return myBitmap == 0; }
        }
        public bool HasLeft
        {
            get { return ( myBitmap & 1 ) != 0; }
        }
        public bool HasBottom
        {
            get { return ( myBitmap & 2 ) != 0; }
        }
        public bool HasFront
        {
            get { return ( myBitmap & 4 ) != 0; }
        }
        public bool HasRight
        {
            get { return ( myBitmap & 8 ) != 0; }
        }
        public bool HasTop
        {
            get { return ( myBitmap & 16 ) != 0; }
        }
        public bool HasBack
        {
            get { return ( myBitmap & 32 ) != 0; }
        }
        public bool HasAll
        {
            get { return myBitmap == 63; }
        }

        public int Index
        {
            get { return Tools.QuickLog2( myBitmap ); }
        }
        public Face Opposite
        {
            get { return new Face( (byte)( 1 << ( ( Index + 3 ) % 6 ) ) ); }
        }

        private Face( byte bitmap )
        {
            myBitmap = bitmap;
        }

        public bool HasFace( Face face )
        {
            return ( myBitmap & face.myBitmap ) == face.myBitmap;
        }
    }

    public delegate Face FindSolidFacesDelegate<T>( T value );
}
