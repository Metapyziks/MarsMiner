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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Shared
{
    public struct Cuboid
    {
        public int X;
        public int Y;
        public int Z;

        public int Width;
        public int Height;
        public int Depth;

        public int Left
        {
            get { return X; }
            set { X = value; }
        }
        public int Bottom
        {
            get { return Y; }
            set { Y = value; }
        }
        public int Front
        {
            get { return Z; }
            set { Z = value; }
        }

        public int Right
        {
            get { return X + Width; }
            set { Width = value - X; }
        }
        public int Top
        {
            get { return Y + Height; }
            set { Height = value - Y; }
        }
        public int Back
        {
            get { return Z + Depth; }
            set { Depth = value - Z; }
        }

        public int Volume
        {
            get { return Width * Height * Depth; }
        }

        public Cuboid( int x, int y, int z, int size )
        {
            X = x;
            Y = y;
            Z = z;

            Width = Height = Depth = size;
        }

        public Cuboid( int x, int y, int z, int width, int height, int depth )
        {
            X = x;
            Y = y;
            Z = z;

            Width = width;
            Height = height;
            Depth = depth;
        }

        public bool IsIntersecting( Cuboid cuboid )
        {
            return cuboid.Left <= Right && cuboid.Right >= Left
                && cuboid.Bottom <= Top && cuboid.Top >= Bottom
                && cuboid.Front <= Back && cuboid.Back >= Front;
        }

        public bool IsIntersecting( int size )
        {
            return 0 <= Right && size >= Left
                && 0 <= Top && size >= Bottom
                && 0 <= Back && size >= Front;
        }

        public bool IsIntersecting( int x, int y, int z, int size )
        {
            return x <= Right && x + size >= Left
                && y <= Top && y + size >= Bottom
                && z <= Back && z + size >= Front;
        }

        public bool IsIntersecting( int x, int y, int z, int width, int height, int depth )
        {
            return IsIntersecting( new Cuboid( x, y, z, width, height, depth ) );
        }

        public Cuboid FindIntersection( Cuboid cuboid )
        {
            if ( !IsIntersecting( cuboid ) )
                throw new InvalidOperationException();

            int left = Math.Max( Left, cuboid.Left );
            int bottom = Math.Max( Bottom, cuboid.Bottom );
            int front = Math.Max( Front, cuboid.Front );

            int right = Math.Min( Right, cuboid.Right );
            int top = Math.Min( Top, cuboid.Top );
            int back = Math.Min( Back, cuboid.Back );

            return new Cuboid( left, bottom, front, right - left, top - bottom, back - front );
        }

        public Cuboid FindIntersection( int size )
        {
            if ( !IsIntersecting( size ) )
                throw new InvalidOperationException();

            int left = Math.Max( Left, 0 );
            int bottom = Math.Max( Bottom, 0 );
            int front = Math.Max( Front, 0 );

            int right = Math.Min( Right, size );
            int top = Math.Min( Top, size );
            int back = Math.Min( Back, size );

            return new Cuboid( left, bottom, front, right - left, top - bottom, back - front );
        }

        public Cuboid FindIntersection( int x, int y, int z, int size )
        {
            if ( !IsIntersecting( x, y, z, size ) )
                throw new InvalidOperationException();

            int left = Math.Max( Left, x );
            int bottom = Math.Max( Bottom, y );
            int front = Math.Max( Front, z );

            int right = Math.Min( Right, x + size );
            int top = Math.Min( Top, y + size );
            int back = Math.Min( Back, z + size );

            return new Cuboid( left, bottom, front, right - left, top - bottom, back - front );
        }

        public Cuboid FindIntersection( int x, int y, int z, int width, int height, int depth )
        {
            return FindIntersection( new Cuboid( x, y, z, width, height, depth ) );
        }

        public override bool Equals( object obj )
        {
            if ( obj is Cuboid )
            {
                Cuboid cuboid = (Cuboid) obj;
                return X == cuboid.X && Y == cuboid.Y && Z == cuboid.Z
                    && Width == cuboid.Width && Height == cuboid.Height && Depth == cuboid.Depth;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ( X << 16 | Y << 8 | Z ) ^ ( Height << 16 | Depth << 8 | Width );
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + "," + Z + "),(" + Width + "," + Height + "," + Depth + ")";
        }
    }
}
