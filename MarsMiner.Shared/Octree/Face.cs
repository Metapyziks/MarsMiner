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

using System.Collections.Generic;

namespace MarsMiner.Shared.Octree
{
    public struct Face : IEnumerable<Face>
    {
        public const int LeftIndex      = 0;
        public const int BottomIndex    = 1;
        public const int FrontIndex     = 2;
        public const int RightIndex     = 3;
        public const int TopIndex       = 4;
        public const int BackIndex      = 5;

        public static readonly Face None    = new Face( 0 );
        public static readonly Face Left    = FromIndex( LeftIndex );
        public static readonly Face Bottom  = FromIndex( BottomIndex );
        public static readonly Face Front   = FromIndex( FrontIndex );
        public static readonly Face Right   = FromIndex( RightIndex );
        public static readonly Face Top     = FromIndex( TopIndex );
        public static readonly Face Back    = FromIndex( BackIndex );
        public static readonly Face All     = new Face( 63 );

        public static Face operator &( Face f0, Face f1 )
        {
            return new Face( (byte) ( f0.Bitmap & f1.Bitmap ) );
        }
        public static Face operator |( Face f0, Face f1 )
        {
            return new Face( (byte) ( f0.Bitmap | f1.Bitmap ) );
        }
        public static Face operator ^( Face f0, Face f1 )
        {
            return new Face( (byte) ( f0.Bitmap ^ f1.Bitmap ) );
        }

        public static Face FromIndex( int index )
        {
            return new Face( (byte) ( 1 << index ) );
        }

        public readonly byte Bitmap;
        public readonly int Index;

        public bool HasNone
        {
            get { return Bitmap == 0; }
        }
        public bool HasLeft
        {
            get { return ( Bitmap & 1 ) != 0; }
        }
        public bool HasBottom
        {
            get { return ( Bitmap & 2 ) != 0; }
        }
        public bool HasFront
        {
            get { return ( Bitmap & 4 ) != 0; }
        }
        public bool HasRight
        {
            get { return ( Bitmap & 8 ) != 0; }
        }
        public bool HasTop
        {
            get { return ( Bitmap & 16 ) != 0; }
        }
        public bool HasBack
        {
            get { return ( Bitmap & 32 ) != 0; }
        }
        public bool HasAll
        {
            get { return Bitmap == 63; }
        }

        public Face HorzLeft
        {
            get
            {
                switch( Index )
                {
                    case LeftIndex:
                        return Back;
                    case BackIndex:
                        return Right;
                    case RightIndex:
                        return Front;
                    case FrontIndex:
                        return Left;
                    default:
                        return this;
                }
            }
        }
        public Face HorzRight
        {
            get
            {
                return HorzLeft.Opposite;
            }
        }
        public Face Opposite
        {
            get { return FromIndex( ( Index + 3 ) % 6 ); }
        }

        public Face( byte bitmap )
        {
            Bitmap = bitmap;
            Index = -1;

            bool found = false;
            for ( int i = 0; i < 6; ++i )
            {
                if ( ( bitmap & ( 1 << i ) ) != 0 )
                {
                    if ( !found )
                    {
                        Index = i;
                        found = true;
                    }
                    else
                    {
                        Index = -1;
                        break;
                    }
                }
            }
        }

        public bool HasFace( Face face )
        {
            return ( Bitmap & face.Bitmap ) == face.Bitmap;
        }

        public override bool Equals( object obj )
        {
            if ( obj is Face )
                return Bitmap == ( (Face) obj ).Bitmap;

            return false;
        }

        public override int GetHashCode()
        {
            return Bitmap;
        }

        public IEnumerator<Face> GetEnumerator()
        {
            return new FaceEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public delegate Face FindSolidFacesDelegate<T>( T value );
}
