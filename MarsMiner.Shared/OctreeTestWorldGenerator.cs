using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LibNoise;

namespace MarsMiner.Shared
{
    public enum OctreeTestBlockType : byte
    {
        Empty = 0,
        White = 1,
        Black = 2,
        Red = 3,
        Green = 4,
        Blue = 5,
    }

    public class OctreeTestWorldGenerator
    {
        private Perlin myNoise;

        public int Seed
        {
            get { return myNoise.Seed; }
            set { myNoise.Seed = value; }
        }
        public int OctaveCount
        {
            get { return myNoise.OctaveCount; }
            set { myNoise.OctaveCount = value; }
        }
        public double Frequency
        {
            get { return myNoise.Frequency; }
            set { myNoise.Frequency = value; }
        }
        public double Lacunarity
        {
            get { return myNoise.Lacunarity; }
            set { myNoise.Lacunarity = value; }
        }
        public double Persistence
        {
            get { return myNoise.Persistence; }
            set { myNoise.Persistence = value; }
        }
        public NoiseQuality NoiseQuality
        {
            get { return myNoise.NoiseQuality; }
            set { myNoise.NoiseQuality = value; }
        }

        public int MinAltitude;
        public int MaxAltitude;

        public OctreeTestWorldGenerator()
        {
            myNoise = new Perlin();

            OctaveCount = 6;
            Frequency = 0.25;
            Lacunarity = 2.0;
            Persistence = 0.5;

            MinAltitude = 192;
            MaxAltitude = 240;
        }

        public OctreeTest Generate( int x, int y, int z, int size )
        {
            OctreeTest octree = new OctreeTest( x, y, z, size );

            int altDiff = ( MaxAltitude - MinAltitude ) / 2;
            int altMid = MinAltitude + altDiff;

            octree.SetCuboid( x, 0, z, size, altMid, size, OctreeTestBlockType.White );

            if ( y + size > MinAltitude )
            {
                Cuboid cuboid = new Cuboid( 0, 0, 0, 1, 1, 1 );

                for ( int nx = 0; nx < size; ++nx )
                {
                    for ( int nz = 0; nz < size; ++nz )
                    {
                        double val = myNoise.GetValue( (double) ( x + nx ) / size, (double) ( z + nz ) / size, 0.5 );
                        int height = (int) ( val * altDiff );

                        cuboid.X = x + nx;
                        cuboid.Z = z + nz;

                        if ( height > 0 )
                        {
                            cuboid.Y = altMid;
                            cuboid.Height = height;
                            octree.SetCuboid( cuboid, OctreeTestBlockType.White );
                        }
                        else if( height < 0 )
                        {
                            cuboid.Y = altMid + height;
                            cuboid.Height = -height;
                            octree.SetCuboid( cuboid, OctreeTestBlockType.Empty );
                        }
                    }
                }
            }

            return octree;
        }
    }
}
