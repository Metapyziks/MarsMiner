/**
 * Copyright (c) 2012 Tamme Schichler [tammeschichler@googlemail.com]
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
using MarsMiner.Saving.Interfaces;
using System.IO;

namespace MarsMiner.Saving.Structures.V0
{
    public class Chunk : IBlockStructure
    {
        private BlockTypeTable blockTypeTable;
        private Octree[] octrees;

        public Tuple<int, uint> Address { get; set; }

        //TODO: accessors

        public Chunk(BlockTypeTable blockTypeTable, Octree[] octrees)
        {
            this.blockTypeTable = blockTypeTable;
            this.octrees = octrees;
        }

        public int Length
        {
            get
            {
                return 4 // blockTypeTable
                    + 1 // octreeCount
                    + 4 * octrees.Length; // octrees
            }
        }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc, Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            var start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write(getBlockPointerFunc(this, blockTypeTable));
            w.Write((byte)octrees.Length);
            foreach (var octree in octrees)
            {
                w.Write(getBlockPointerFunc(this, octree));
            }
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in Chunk!");
            }
#endif
        }

        public static Chunk Read(Tuple<Stream, int> source, Func<Stream, uint, Tuple<Stream, int>> resolvePointerFunc, Func<uint, string> resolveStringFunc)
        {
            source.Item1.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(source.Item1);

            var blockTypeTablePointer = r.ReadUInt32();
            var octreeCount = r.ReadByte();

            var octreePointers = new uint[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                octreePointers[i] = r.ReadUInt32();
            }

            var blockTypeTable = BlockTypeTable.Read(resolvePointerFunc(source.Item1, blockTypeTablePointer), resolvePointerFunc, resolveStringFunc);
            var octrees = new Octree[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                octrees[i] = Octree.Read(resolvePointerFunc(source.Item1, octreePointers[i]), resolvePointerFunc, resolveStringFunc);
            }

            return new Chunk(blockTypeTable, octrees);
        }
    }
}
