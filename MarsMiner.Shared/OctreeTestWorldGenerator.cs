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

        public Octree<OctreeTestBlockType> Generate()
        {
            int halfSize = WorldSize / 2;
            Octree<OctreeTestBlockType> octree = new Octree<OctreeTestBlockType>( -halfSize, -GroundLevel, -halfSize, WorldSize );
            octree.SetCuboid( -halfSize, -GroundLevel, -halfSize, WorldSize, GroundLevel, WorldSize, OctreeTestBlockType.White );

            return octree;
        }
    }
}
