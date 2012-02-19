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
using System.IO;
using System.Linq;
using MarsMiner.Saving.Common;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class Chunk : BlockStructure
    {
        private BlockTypeTable _blockTypeTable;
        private Octree[] _octrees;

        internal Chunk(GameSave gameSave, Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        public Chunk(GameSave gameSave, BlockTypeTable blockTypeTable, Octree[] octrees) : base(gameSave)
        {
            _blockTypeTable = blockTypeTable;
            _octrees = octrees;

            UpdateLength();
        }

        public BlockTypeTable BlockTypeTable
        {
            get
            {
                Load();
                return _blockTypeTable;
            }
        }

        public Octree[] Octrees
        {
            get
            {
                Load();
                return _octrees.ToArray();
            }
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get
            {
                var referencedBlocks = new BlockStructure[Octrees.Length + 1];
                referencedBlocks[0] = BlockTypeTable;
                Octrees.CopyTo(referencedBlocks, 1);
                return referencedBlocks;
            }
        }

        protected override void ReadData(BinaryReader reader)
        {
            uint blockTypeTablePointer = reader.ReadUInt32();
            byte octreeCount = reader.ReadByte();

            var octreePointers = new uint[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                octreePointers[i] = reader.ReadUInt32();
            }

            _blockTypeTable = new BlockTypeTable(GameSave, GameSave.ResolvePointer(Address.Item1, blockTypeTablePointer));
            _octrees = new Octree[octreeCount];

            for (int i = 0; i < octreeCount; i++)
            {
                _octrees[i] = new Octree(GameSave, GameSave.ResolvePointer(Address.Item1, octreePointers[i]));
            }
        }

        protected override void ForgetData()
        {
            _blockTypeTable = null;
            _octrees = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(GameSave.FindBlockPointer(this, BlockTypeTable));
            writer.Write((byte) Octrees.Length);
            foreach (Octree octree in Octrees)
            {
                writer.Write(GameSave.FindBlockPointer(this, octree));
            }
        }

        protected override void UpdateLength()
        {
            Length = 4 // blockTypeTable
                     + 1 // octreeCount
                     + 4 * Octrees.Length; // octrees
        }
    }
}