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
using System.IO;
using System.Linq;
using MarsMiner.Saving.Interface.V0;
using MarsMiner.Saving.Interfaces;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Structures.V0
{
    public class Chunk : IBlockStructure
    {
        private Tuple<int, uint> _address;
        private BlockTypeTable _blockTypeTable;
        private Octree[] _octrees;

        private Dictionary<int, IntRangeList> _recursiveUsedSpace;

        public Chunk(BlockTypeTable blockTypeTable, Octree[] octrees)
        {
            _blockTypeTable = blockTypeTable;
            _octrees = octrees;

            Length = 4 // blockTypeTable
                     + 1 // octreeCount
                     + 4*Octrees.Length; // octrees
        }

        private Chunk(BlockTypeTable blockTypeTable, Octree[] octrees, Tuple<int, uint> address)
            : this(blockTypeTable, octrees)
        {
            Address = address;
            CalculateRecursiveUsedSpace();
        }

        public Octree[] Octrees
        {
            get { return _octrees.ToArray(); }
        }

        public BlockTypeTable BlockTypeTable
        {
            get { return _blockTypeTable; }
        }

        #region IBlockStructure Members

        public IBlockStructure[] UnboundBlocks
        {
            get
            {
                if (Address != null)
                {
                    //Bound
                    return new IBlockStructure[0];
                }

                List<IBlockStructure> blocks = _octrees.Where(o => o.Address == null).ToList<IBlockStructure>();
                if (_blockTypeTable.Address == null)
                {
                    blocks.Add(_blockTypeTable);
                }
                return blocks.ToArray();
            }
        }

        public Tuple<int, uint> Address
        {
            get { return _address; }
            set
            {
                if (_address != null)
                {
                    throw new InvalidOperationException("Address can't be reassigned!");
                }
                _address = value;
            }
        }

        public Dictionary<int, IntRangeList> RecursiveUsedSpace
        {
            get
            {
                if (Address == null)
                {
                    throw new InvalidOperationException("Can't get used space from unbound block!");
                }
                if (_recursiveUsedSpace == null)
                {
                    CalculateRecursiveUsedSpace();
                }
                return _recursiveUsedSpace;
            }
        }

        public int Length { get; private set; }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc,
                          Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            long start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write(getBlockPointerFunc(this, BlockTypeTable));
            w.Write((byte) Octrees.Length);
            foreach (Octree octree in Octrees)
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

        public void Unload()
        {
            if (Address == null)
            {
                throw new InvalidOperationException("Can't unload unbound blocks!");
            }

            CalculateRecursiveUsedSpace();
            _blockTypeTable = null;
            _octrees = null;
        }

        #endregion

        private void CalculateRecursiveUsedSpace()
        {
            if (_recursiveUsedSpace != null) return;

            _recursiveUsedSpace = new Dictionary<int, IntRangeList>();
            _recursiveUsedSpace.Add(_blockTypeTable.RecursiveUsedSpace);
            foreach (Octree octree in _octrees)
            {
                _recursiveUsedSpace.Add(octree.RecursiveUsedSpace);
            }

            if (!_recursiveUsedSpace.ContainsKey(Address.Item1))
            {
                _recursiveUsedSpace[Address.Item1] = new IntRangeList();
            }
            _recursiveUsedSpace[Address.Item1].Add(new Tuple<int, int>((int) Address.Item2, (int) Address.Item2 + Length));
        }

        public static Chunk Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc,
                                 Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc,
                                 ReadOptions readOptions)
        {
#if DebugVerboseBlocks
            Console.WriteLine("Reading {0} from {1}", "Chunk", source);
#endif

            Stream stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            uint blockTypeTablePointer = r.ReadUInt32();
            byte octreeCount = r.ReadByte();

            var octreePointers = new uint[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                octreePointers[i] = r.ReadUInt32();
            }

#if DebugVerboseBlocks || AssertBlockLength
            long end = stream.Position;
#endif

            BlockTypeTable blockTypeTable = BlockTypeTable.Read(
                resolvePointerFunc(source.Item1, blockTypeTablePointer), resolvePointerFunc, resolveStringFunc,
                getStreamFunc, readOptions);
            var octrees = new Octree[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                octrees[i] = Octree.Read(resolvePointerFunc(source.Item1, octreePointers[i]), resolvePointerFunc,
                                         resolveStringFunc, getStreamFunc, readOptions);
            }

            var chunk = new Chunk(blockTypeTable, octrees, source);

            if (readOptions.ChunkCallback != null)
            {
                readOptions.ChunkCallback(chunk);
            }

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", "Chunk", chunk.Address, chunk.Address.Item2 + chunk.Length, end);
#endif
#if AssertBlockLength
            if (chunk.Address.Item2 + chunk.Length != end)
            {
                throw new Exception("Length mismatch in Chunk!");
            }
#endif

            return chunk;
        }
    }
}