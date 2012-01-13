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
        private Perlin myHillyNoise;
        private Perlin myPlainsNoise;
        private Perlin myTransNoise;

        private int myMinHilly;
        private int myMaxHilly;

        private int myMinPlains;
        private int myMaxPlains;

        public OctreeTestWorldGenerator()
        {
            myHillyNoise = new Perlin
            {
                OctaveCount = 6,
                Frequency = 1.0,
                Lacunarity = 2.0,
                Persistence = 0.5
            };

            myPlainsNoise = new Perlin
            {
                OctaveCount = 6,
                Frequency = 8.0,
                Lacunarity = 2.0,
                Persistence = 1.0
            };

            myTransNoise = new Perlin
            {
                OctaveCount = 6,
                Frequency = 1.0 / 32.0,
                Lacunarity = 2.0,
                Persistence = 1.0
            };

            myMinHilly = 64;
            myMaxHilly = 240;

            myMinPlains = 64;
            myMaxPlains = 72;
        }

        public OctreeTest Generate( int x, int y, int z, int size, int resolution = 1 )
        {
            OctreeTest octree = new OctreeTest( x, y, z, size );

            int min = System.Math.Min( myMinHilly, myMinPlains );

            octree.SetCuboid( x, 0, z, size, System.Math.Min( myMinHilly, myMinPlains ), size, OctreeTestBlockType.White );

            if ( y + size > min )
            {
                int hillDiff = ( myMaxHilly - myMinHilly ) / 2;
                int hillMid = myMinHilly + hillDiff;
                int plainDiff = ( myMinPlains - myMaxPlains ) / 2;
                int plainMid = myMaxPlains + plainDiff;

                Cuboid cuboid = new Cuboid( 0, 0, 0, resolution, 1, resolution );

                for ( int nx = 0; nx < size; nx += resolution )
                {
                    for ( int nz = 0; nz < size; nz += resolution )
                    {
                        double dx = ( x + nx ) / 256.0;
                        double dy = ( z + nz ) / 256.0;

                        double hillVal = myHillyNoise.GetValue( dx, dy, 0.5 ) * hillDiff + hillMid;
                        double plainVal = myHillyNoise.GetValue( dx, dy, 0.5 ) * plainDiff + plainMid;
                        double trans = Tools.Clamp( ( myTransNoise.GetValue( dx, dy, 0.5 ) + 1.0 ) / 2.0, 0.0, 1.0 );
                        trans *= trans;

                        int height = (int) System.Math.Round( ( trans * hillVal + ( 1 - trans ) * plainVal ) / resolution ) * resolution;

                        cuboid.X = x + nx;
                        cuboid.Z = z + nz;

                        cuboid.Height = height;
                        octree.SetCuboid( cuboid, OctreeTestBlockType.White );
                    }
                }
            }

            return octree;
        }
    }
}
