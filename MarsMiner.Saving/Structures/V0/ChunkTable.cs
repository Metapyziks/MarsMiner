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
    public class ChunkTable : IBlockStructure
    {
        private Tuple<int, uint> _address;
        private Chunk[] _chunks;

        private Dictionary<int, IntRangeList> _recursiveUsedSpace;
        private int[] _xLocations;
        private int[] _zLocations;

        public ChunkTable(Tuple<int, int, Chunk>[] chunks)
            : this(
                chunks.Select(x => x.Item1).ToArray(),
                chunks.Select(x => x.Item2).ToArray(),
                chunks.Select(x => x.Item3).ToArray())
        {
        }

        public ChunkTable(int[] xLocations, int[] zLocations, Chunk[] chunks)
        {
            if (xLocations.Length != zLocations.Length || zLocations.Length != chunks.Length)
            {
                throw new ArgumentException("Argument arrays must have the same length!");
            }
            _xLocations = xLocations;
            _zLocations = zLocations;
            _chunks = chunks;

            Length = 4 //chunk count
                     + chunks.Length*
                     (4 // xLocation
                      + 4 // yLocation
                      + 4); // chunk
        }

        private ChunkTable(int[] xLocations, int[] zLocations, Chunk[] chunks, Tuple<int, uint> address)
            : this(xLocations, zLocations, chunks)
        {
            Address = address;
            CalculateRecursiveUsedSpace();
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
                return _chunks.Where(c => c.Address == null).ToArray<IBlockStructure>();
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

            w.Write((uint) _chunks.LongLength);
            for (long i = 0; i < _chunks.LongLength; i++)
            {
                w.Write(_xLocations[i]);
                w.Write(_zLocations[i]);
                uint chunkPointer = getBlockPointerFunc(this, _chunks[i]);
                w.Write(chunkPointer);
            }
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in ChunkTable!");
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
            _xLocations = null;
            _zLocations = null;
            _chunks = null;
        }

        #endregion

        private void CalculateRecursiveUsedSpace()
        {
            if (_recursiveUsedSpace != null) return;

            _recursiveUsedSpace = new Dictionary<int, IntRangeList>();
            foreach (Chunk chunk in _chunks)
            {
                _recursiveUsedSpace.Add(chunk.RecursiveUsedSpace);
            }

            if (!_recursiveUsedSpace.ContainsKey(Address.Item1))
            {
                _recursiveUsedSpace[Address.Item1] = new IntRangeList();
            }
            _recursiveUsedSpace[Address.Item1].Add(new Tuple<int, int>((int) Address.Item2, (int) Address.Item2 + Length));
        }

        public IEnumerable<Tuple<int, int, Chunk>> GetChunks()
        {
            return _chunks.Select((t, i) => new Tuple<int, int, Chunk>(_xLocations[i], _zLocations[i], t));
        }

        public static ChunkTable Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc,
                                      Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc,
                                      ReadOptions readOptions)
        {
#if DebugVerboseBlocks
            Console.WriteLine("Reading {0} from {1}", "ChunkTable", source);
#endif

            Stream stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            uint chunkCount = r.ReadUInt32();

            var xLocations = new int[chunkCount];
            var yLocations = new int[chunkCount];
            var chunkPointers = new uint[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                xLocations[i] = r.ReadInt32();
                yLocations[i] = r.ReadInt32();
                chunkPointers[i] = r.ReadUInt32();
            }

#if DebugVerboseBlocks || AssertBlockLength
            long end = stream.Position;
#endif

            var chunks = new Chunk[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                chunks[i] = Chunk.Read(resolvePointerFunc(source.Item1, chunkPointers[i]), resolvePointerFunc,
                                       resolveStringFunc, getStreamFunc, readOptions);
            }

            var newChunkTable = new ChunkTable(xLocations, yLocations, chunks, source);

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", "ChunkTable", newChunkTable.Address, newChunkTable.Address.Item2 + newChunkTable.Length, end);
#endif
#if AssertBlockLength
            if (newChunkTable.Address.Item2 + newChunkTable.Length != end)
            {
                throw new Exception("Length mismatch in ChunkTable!");
            }
#endif

            return newChunkTable;
        }
    }
}