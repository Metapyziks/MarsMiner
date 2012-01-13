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

        public int WorldSize = 64;
        public int GroundLevel = 32;

        public int X = 0;
        public int Z = 0;

        public OctreeTestWorldGenerator()
        {
            myNoise = new Perlin();

            OctaveCount = 6;
            Frequency = 0.125;
            Lacunarity = 2.0;
            Persistence = 0.5;

            WorldSize = 64;
            GroundLevel = 32;

            X = 0;
            Z = 0;
        }

        public OctreeTest Generate()
        {
            OctreeTest octree = new OctreeTest( X, 0, Z, WorldSize );

            Cuboid cuboid = new Cuboid( 0, 0, 0, 1, 1, 1 );

            for ( int x = 0; x < WorldSize; ++x )
            {
                for ( int z = 0; z < WorldSize; ++z )
                {
                    double val = myNoise.GetValue( (double) x / WorldSize, (double) z / WorldSize, 0.5 );

                    cuboid.X = X + x;
                    cuboid.Z = Z + z;
                    cuboid.Height = GroundLevel + (int) ( val * WorldSize * 0.5 );

                    octree.SetCuboid( cuboid, OctreeTestBlockType.White );
                }
            }

            return octree;
        }
    }
}
