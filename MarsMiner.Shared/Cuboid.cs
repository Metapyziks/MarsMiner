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
            set { X = value; }
        }
        public int Top
        {
            get { return Y + Height; }
            set { Y = value; }
        }
        public int Back
        {
            get { return Z + Depth; }
            set { Z = value; }
        }

        public int Volume
        {
            get { return Width * Height * Depth; }
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
    }
}
