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

        public IBlockStructure[] UnboundBlocks
        {
            get
            {
                if (Address != null)
                {
                    //Bound
                    return new IBlockStructure[0];
                }

                var blocks = octrees.Where(o => o.Address == null).ToList<IBlockStructure>();
                if (blockTypeTable.Address == null)
                {
                    blocks.Add(blockTypeTable);
                }
                return blocks.ToArray();
            }
        }

        private Tuple<int, uint> address;
        public Tuple<int, uint> Address
        {
            get
            {
                return address;
            }
            set
            {
                if (address != null)
                {
                    throw new InvalidOperationException("Address can't be reassigned!");
                }
                address = value;
            }
        }

        public Octree[] Octrees { get { return octrees.ToArray(); } }
        public BlockTypeTable BlockTypeTable { get { return blockTypeTable; } }

        public Chunk(BlockTypeTable blockTypeTable, Octree[] octrees)
        {
            this.blockTypeTable = blockTypeTable;
            this.octrees = octrees;

            Length = 4 // blockTypeTable
                + 1 // octreeCount
                + 4 * Octrees.Length; // octrees
        }

        private Chunk(BlockTypeTable blockTypeTable, Octree[] octrees, Tuple<int, uint> address)
            : this(blockTypeTable, octrees)
        {
            Address = address;
        }

        public int Length { get; private set; }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc, Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            var start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write(getBlockPointerFunc(this, BlockTypeTable));
            w.Write((byte)Octrees.Length);
            foreach (var octree in Octrees)
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

        public static Chunk Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc, Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc, ReadOptions readOptions)
        {
            var stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            var blockTypeTablePointer = r.ReadUInt32();
            var octreeCount = r.ReadByte();

            var octreePointers = new uint[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                octreePointers[i] = r.ReadUInt32();
            }

            var blockTypeTable = BlockTypeTable.Read(resolvePointerFunc(source.Item1, blockTypeTablePointer), resolvePointerFunc, resolveStringFunc, getStreamFunc, readOptions);
            var octrees = new Octree[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                octrees[i] = Octree.Read(resolvePointerFunc(source.Item1, octreePointers[i]), resolvePointerFunc, resolveStringFunc, getStreamFunc, readOptions);
            }

            Chunk chunk = new Chunk(blockTypeTable, octrees, source);

            if (readOptions.ChunkCallback != null)
            {
                readOptions.ChunkCallback(chunk);
            }

            return chunk;
        }

        public void Unload()
        {
            if (Address == null)
            {
                throw new InvalidOperationException("Can't unload unbound blocks!");
            }

            blockTypeTable = null;
            octrees = null;
        }
    }
}
