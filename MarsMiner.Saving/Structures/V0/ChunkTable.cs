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
using MarsMiner.Saving.Interfaces;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class ChunkTable : BlockStructure
    {
        private Chunk[] _chunks;

        private int[] _xLocations;
        private int[] _zLocations;

        public ChunkTable(GameSave gameSave, Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        public ChunkTable(GameSave gameSave, int[] xLocations, int[] zLocations, Chunk[] chunks) : base(gameSave)
        {
            if (xLocations.Length != zLocations.Length || zLocations.Length != chunks.Length)
            {
                throw new ArgumentException("Argument arrays must have the same length!");
            }
            _xLocations = xLocations;
            _zLocations = zLocations;
            _chunks = chunks;

            UpdateLength();
        }

        public ChunkTable(GameSave gameSave, Tuple<int, int, Chunk>[] chunks)
            : this(
                gameSave,
                chunks.Select(x => x.Item1).ToArray(),
                chunks.Select(x => x.Item2).ToArray(),
                chunks.Select(x => x.Item3).ToArray())
        {
        }

        public IEnumerable<Tuple<int, int, Chunk>> GetChunks()
        {
            return _chunks.Select((t, i) => new Tuple<int, int, Chunk>(_xLocations[i], _zLocations[i], t));
        }

        protected override void ReadData(BinaryReader reader)
        {
            uint chunkCount = reader.ReadUInt32();

            _xLocations = new int[chunkCount];
            _zLocations = new int[chunkCount];
            var chunkPointers = new uint[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                _xLocations[i] = reader.ReadInt32();
                _zLocations[i] = reader.ReadInt32();
                chunkPointers[i] = reader.ReadUInt32();
            }

            _chunks = new Chunk[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                _chunks[i] = new Chunk(GameSave, GameSave.ResolvePointer(Address.Item1, chunkPointers[i]));
            }
        }

        protected override void ForgetData()
        {
            _xLocations = null;
            _zLocations = null;
            _chunks = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write((uint) _chunks.LongLength);
            for (long i = 0; i < _chunks.LongLength; i++)
            {
                writer.Write(_xLocations[i]);
                writer.Write(_zLocations[i]);
                uint chunkPointer = GameSave.FindBlockPointer(this, _chunks[i]);
                writer.Write(chunkPointer);
            }
        }

        protected override void UpdateLength()
        {
            Length = 4 //chunk count
                     + _chunks.Length *
                     (4 // xLocation
                      + 4 // yLocation
                      + 4); // chunk
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get { return _chunks.ToArray<BlockStructure>(); }
        }
    }
}