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
using MarsMiner.Saving.Interfaces;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class SavedStateIndex : BlockStructure
    {
        public SavedStateIndex(GameSave gameSave, Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        public SavedStateIndex(GameSave gameSave, long timestamp, string saveName, ChunkTable chunkTable)
            : base(gameSave)
        {
            Timestamp = timestamp;
            SaveName = saveName;
            ChunkTable = chunkTable;

            UpdateLength();
        }

        public long Timestamp { get; private set; }
        public string SaveName { get; private set; }
        public ChunkTable ChunkTable { get; private set; }

        public override BlockStructure[] ReferencedBlocks
        {
            get { return new BlockStructure[] { ChunkTable }; }
        }

        protected override void ReadData(BinaryReader reader)
        {
            Timestamp = reader.ReadInt64();
            uint saveNameAddress = reader.ReadUInt32();
            uint chunkTablePointer = reader.ReadUInt32();

            SaveName = GameSave.ResolveString(saveNameAddress);
            ChunkTable = new ChunkTable(GameSave, GameSave.ResolvePointer(Address.Item1, chunkTablePointer));
        }

        protected override void ForgetData()
        {
            ChunkTable = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(Timestamp);
            writer.Write(GameSave.FindStringAddress(SaveName));
            writer.Write(GameSave.FindBlockPointer(this, ChunkTable));
        }

        protected override void UpdateLength()
        {
            Length = 8 // timestamp
                     + 4 // saveName
                     + 4; // chunkTable
        }
    }
}