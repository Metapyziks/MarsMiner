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
        private int mySeed;

        private Perlin myHillyNoise;
        private Perlin myPlainsNoise;
        private Perlin myTransNoise;

        private int myMinHilly;
        private int myMaxHilly;

        private int myMinPlains;
        private int myMaxPlains;

        public OctreeTestWorldGenerator( int seed = 0 )
        {
            if ( seed == 0 )
            {
                Random rand = new Random();
                seed = rand.Next( int.MaxValue );
            }

            mySeed = seed;

            myHillyNoise = new Perlin
            {
                Seed = seed,
                OctaveCount = 6,
                Frequency = 1.0,
                Lacunarity = 2.0,
                Persistence = 0.5
            };

            myPlainsNoise = new Perlin
            {
                Seed = seed,
                OctaveCount = 6,
                Frequency = 8.0,
                Lacunarity = 2.0,
                Persistence = 1.0
            };

            myTransNoise = new Perlin
            {
                Seed = seed,
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

            /*if( y < 128 )
                octree.SetCuboid( octree.Cube, OctreeTestBlockType.White );
            else if ( y == 128 )
            {
                int blk = 16;
                octree.SetCuboid( new Cuboid( x, y, z, blk, blk, blk ), OctreeTestBlockType.White );
                octree.SetCuboid( new Cuboid( x + size - blk, y, z, blk, blk, blk ), OctreeTestBlockType.White );
                octree.SetCuboid( new Cuboid( x, y, z + size - blk, blk, blk, blk ), OctreeTestBlockType.White );
                octree.SetCuboid( new Cuboid( x + size - blk, y, z + size - blk, blk, blk, blk ), OctreeTestBlockType.White );
            }

            return octree;*/

            int min = System.Math.Min( myMinHilly, myMinPlains );

            octree.SetCuboid( x, 0, z, size, System.Math.Min( myMinHilly, myMinPlains ), size, OctreeTestBlockType.White );

            if ( y + size > min )
            {
                int hillDiff = ( myMaxHilly - myMinHilly ) / 2;
                int hillMid = myMinHilly + hillDiff;
                int plainDiff = ( myMinPlains - myMaxPlains ) / 2;
                int plainMid = myMaxPlains + plainDiff;
                
                int maxCount = size / resolution;
                double hres = resolution / 2.0;

                int[,] map = new int[ maxCount, maxCount ];

                for( int i = 0; i < maxCount; ++ i )
                {
                    double dx = ( x + i * resolution + hres ) / 256.0;
                    for( int j = 0; j < maxCount; ++ j )
                    {
                        double dy = ( z + j * resolution + hres ) / 256.0;
                        double hillVal = Tools.Clamp( myHillyNoise.GetValue( dx, dy, 0.5 ) * hillDiff + hillMid, myMinHilly, myMaxHilly );
                        double plainVal = Tools.Clamp( myHillyNoise.GetValue( dx, dy, 0.5 ) * plainDiff + plainMid, myMinPlains, myMaxPlains );
                        double trans = Tools.Clamp( ( myTransNoise.GetValue( dx, dy, 0.5 ) + 1.0 ) / 2.0, 0.0, 1.0 );
                        trans *= trans;

                        map[ i, j ] = (int) System.Math.Round( ( trans * hillVal + ( 1 - trans ) * plainVal ) / resolution ) * resolution;
                    }
                }

                Cuboid cuboid = new Cuboid( 0, 0, 0, resolution, 1, resolution );

                int[,] prev = null;

                for ( int count = 1; count <= maxCount; count <<= 1 )
                {
                    int res = size / count;
                    hres = res / 2.0;
                    int[,] cur = new int[ count, count ];

                    int sca = res / resolution;

                    cuboid.Width = res;
                    cuboid.Depth = res;

                    for ( int nx = 0; nx < count; ++nx )
                    {
                        int rx = x + nx * res;
                        int px = nx >> 1;

                        for ( int nz = 0; nz < count; ++nz )
                        {
                            int rz = z + nz * res;
                            int pz = nz >> 1;
                            
                            int height = 256;
                            
                            for ( int ix = nx * sca; ix < ( nx + 1 ) * sca; ++ix )
                                for ( int iz = nz * sca; iz < ( nz + 1 ) * sca; ++iz )
                                    if ( map[ ix, iz ] < height )
                                        height = map[ ix, iz ];


                            //int height = map[ nx * sca, nz * sca ] / res * res;

                            cur[ nx, nz ] = height;

                            int prevHeight = ( count == 1 ? 0 : prev[ px, pz ] );

                            cuboid.X = rx;
                            cuboid.Z = rz;

                            cuboid.Bottom = System.Math.Min( height, prevHeight );
                            cuboid.Top    = System.Math.Max( height, prevHeight );

                            if( height > prevHeight )
                                octree.SetCuboid( cuboid, OctreeTestBlockType.White );
                            //else
                            //    octree.SetCuboid( cuboid, OctreeTestBlockType.Empty );

                        }
                    }

                    prev = cur;
                }
            }

            return octree;
        }
    }
}
