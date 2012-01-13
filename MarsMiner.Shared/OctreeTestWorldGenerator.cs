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
        public int WorldSize = 256;
        public int GroundLevel = 127;

        public OctreeTest Generate()
        {
            int halfSize = WorldSize / 2;
            OctreeTest octree = new OctreeTest( -halfSize, -GroundLevel, -halfSize, WorldSize );
            Cuboid ground = new Cuboid
            {
                Left = -halfSize, Front = -halfSize, Bottom = -GroundLevel,
                Right = halfSize, Back = halfSize, Top = 0
            };
            octree.SetCuboid( ground, OctreeTestBlockType.White );

            return octree;
        }
    }
}
