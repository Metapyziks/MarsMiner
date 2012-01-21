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

using LibNoise;

using MarsMiner.Shared.Octree;

namespace MarsMiner.Shared.Geometry
{
    public class PerlinGenerator : WorldGenerator
    {
        private Perlin myHillyNoise;
        private Perlin myPlainsNoise;
        private Perlin myTransNoise;

        private int myMinHilly;
        private int myMaxHilly;

        private int myMinPlains;
        private int myMaxPlains;

        public PerlinGenerator( int seed = 0 )
            : base( seed )
        {
            myHillyNoise = new Perlin
            {
                Seed = Seed,
                OctaveCount = 6,
                Frequency = 1.0,
                Lacunarity = 2.0,
                Persistence = 0.5
            };

            myPlainsNoise = new Perlin
            {
                Seed = Seed,
                OctaveCount = 6,
                Frequency = 8.0,
                Lacunarity = 2.0,
                Persistence = 1.0
            };

            myTransNoise = new Perlin
            {
                Seed = Seed,
                OctaveCount = 6,
                Frequency = 1.0 / 32.0,
                Lacunarity = 2.0,
                Persistence = 1.0
            };

            myMinHilly = 768;
            myMaxHilly = 896;

            myMinPlains = 768;
            myMaxPlains = 776;
        }

        public override Octree<UInt16> Generate( int x, int y, int z, int size, int resolution = 1 )
        {
            Octree<UInt16> octree = base.Generate( x, y, z, size, resolution );

            Random rand = new Random( Seed );

            UInt16 empty = BlockManager.GetID( "Core_Empty" );
            UInt16 sandCube = BlockManager.GetID( "MarsMiner_Sand", 0 );
            UInt16[] sandSlopes = new UInt16[]
            {
                BlockManager.GetID( "MarsMiner_Sand", 1 ),
                BlockManager.GetID( "MarsMiner_Sand", 2 ),
                BlockManager.GetID( "MarsMiner_Sand", 3 ),
                BlockManager.GetID( "MarsMiner_Sand", 4 )
            };
            Face[] slopeFaces = new Face[] { Face.Front, Face.Left, Face.Back, Face.Right };
            UInt16 rock = BlockManager.GetID( "MarsMiner_Rock" );
            UInt16 boulder = BlockManager.GetID( "MarsMiner_Boulder" );

            int min = System.Math.Min( myMinHilly, myMinPlains );
            int gradRange = 2;

            octree.SetCuboid( x, 0, z, size, System.Math.Min( myMinHilly, myMinPlains ), size, rock );

            if ( y + size >= min )
            {
                int hillDiff = ( myMaxHilly - myMinHilly ) / 2;
                int hillMid = myMinHilly + hillDiff;
                int plainDiff = ( myMinPlains - myMaxPlains ) / 2;
                int plainMid = myMaxPlains + plainDiff;
                
                int maxCount = size / resolution;
                double hres = resolution / 2.0;

                int[,] heightmap = new int[ maxCount + gradRange * 2, maxCount + gradRange * 2 ];
                double[,] gradmap = new double[ maxCount, maxCount ];

                for( int i = 0; i < maxCount + gradRange * 2; ++ i )
                {
                    double dx = ( x + ( i - gradRange ) * resolution + hres ) / 256.0;
                    for( int j = 0; j < maxCount + gradRange * 2; ++ j )
                    {
                        double dy = ( z + ( j - gradRange ) * resolution + hres ) / 256.0;
                        double hillVal = Tools.Clamp( myHillyNoise.GetValue( dx, dy, 0.5 ) * hillDiff + hillMid, myMinHilly, myMaxHilly );
                        double plainVal = Tools.Clamp( myHillyNoise.GetValue( dx, dy, 0.5 ) * plainDiff + plainMid, myMinPlains, myMaxPlains );
                        double trans = Tools.Clamp( ( myTransNoise.GetValue( dx, dy, 0.5 ) + 1.0 ) / 2.0, 0.0, 1.0 );
                        //trans *= trans;

                        int val = (int) System.Math.Floor( trans * hillVal + ( 1 - trans ) * plainVal );
                        
                        heightmap[ i, j ] = val / resolution * resolution;
                    }
                }

                for ( int i = 0; i < maxCount; ++i )
                {
                    for ( int j = 0; j < maxCount; ++j )
                    {
                        double grad = 0;

                        for ( int gx = -gradRange; gx <= gradRange; ++gx )
                        {
                            for ( int gy = -gradRange; gy <= gradRange; ++gy )
                            {
                                if( gx == 0 && gy == 0 )
                                    continue;

                                double dist = System.Math.Sqrt( gx * gx + gy * gy );

                                int diff = heightmap[ i + gradRange + gx, j + gradRange + gy ]
                                    - heightmap[ i + gradRange, j + gradRange ];

                                grad += System.Math.Abs( diff ) / dist;
                            }
                        }

                        gradmap[ i, j ] = grad;
                    }
                }

                Cuboid rcuboid = new Cuboid( 0, 0, 0, resolution, 1, resolution );
                Cuboid scuboid = new Cuboid( 0, 0, 0, 1, 1, 1 );

                int[,] prev = null;

                for ( int count = 1; count <= maxCount; count <<= 1 )
                {
                    int res = size / count;
                    hres = res / 2.0;
                    int[,] cur = new int[ count, count ];

                    int sca = res / resolution;

                    rcuboid.Width = res;
                    rcuboid.Depth = res;

                    scuboid.Width = res;
                    scuboid.Height = res;
                    scuboid.Depth = res;

                    for ( int nx = 0; nx < count; ++nx )
                    {
                        int rx = x + nx * res;
                        int px = nx >> 1;

                        for ( int nz = 0; nz < count; ++nz )
                        {
                            int rz = z + nz * res;
                            int pz = nz >> 1;

                            int realHeight = heightmap[ nx * sca + gradRange, nz * sca + gradRange ];
                            int height = realHeight / res * res;

                            cur[ nx, nz ] = height;

                            int prevHeight = ( count == 1 ? 0 : prev[ px, pz ] );

                            rcuboid.X = rx;
                            rcuboid.Z = rz;

                            rcuboid.Bottom = System.Math.Min( height, prevHeight );
                            rcuboid.Top = System.Math.Max( height, prevHeight );

                            if( height > prevHeight )
                                octree.SetCuboid( rcuboid, rock );
                            else
                                octree.SetCuboid( rcuboid, empty );

                            if ( ( res == 1 || ( resolution > 1 && height == realHeight ) )
                                && gradmap[ nx, nz ] <= 8.0 * res )
                            {
                                scuboid.X = rx;
                                scuboid.Y = height - res;
                                scuboid.Z = rz;

                                octree.SetCuboid( scuboid, sandCube );

                                if ( res == 1 && rand.NextDouble() < 1.0 / 256.0 )
                                {
                                    scuboid.Y += 1;
                                    octree.SetCuboid( scuboid, rock );
                                }
                            }
                        }
                    }

                    prev = cur;
                }

                if ( resolution == 1 )
                {
                    List<Tuple<int,int,int,int>> toSlope = new List<Tuple<int, int, int, int>>();

                    var enumer = new OctreeEnumerator<UInt16>( octree );
                    while( enumer.MoveNext() )
                    {
                        OctreeLeaf<UInt16> leaf = enumer.Current;
                        if ( enumer.Size == 1 && leaf.Value == sandCube )
                        {
                            int start = rand.Next( 4 );
                            for ( int i = start; i < start + 4; ++i )
                            {
                                Face face = slopeFaces[ i % 4 ];
                                var n = leaf.FindNeighbour( face ) as OctreeLeaf<UInt16>;
                                if ( n != null && n.Value == empty )
                                {
                                    toSlope.Add( new Tuple<int, int, int, int>
                                        ( enumer.X, enumer.Y, enumer.Z, i % 4 ) );
                                    break;
                                }
                            }
                        }
                    }

                    foreach ( var item in toSlope )
                        octree.SetCuboid( item.Item1, item.Item2, item.Item3,
                            1, 1, 1, sandSlopes[ item.Item4 ] );
                }
            }

            return octree;
        }
    }
}
