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
    public sealed class Header : BlockStructure, IHeader
    {
        public const int Version = 0;

        public Header(GameSave gameSave, Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        public Header(GameSave gameSave, SavedStateIndex saveIndex) : base(gameSave)
        {
            SaveIndex = saveIndex;

            UpdateLength();
        }

        public SavedStateIndex SaveIndex { get; private set; }

        public override BlockStructure[] ReferencedBlocks
        {
            get { return new BlockStructure[] { SaveIndex }; }
        }

        protected override void ReadData(BinaryReader reader)
        {
            int version = reader.ReadInt32();
            if (version != Version)
            {
                throw new InvalidDataException("Expected file version " + Version + ", was " + version + ".");
            }

            uint saveIndexPointer = reader.ReadUInt32();

            SaveIndex = new SavedStateIndex(GameSave, GameSave.ResolvePointer(Address.Item1, saveIndexPointer));
        }

        protected override void ForgetData()
        {
            SaveIndex = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(GameSave.FindBlockPointer(this, SaveIndex));
        }

        protected override void UpdateLength()
        {
            Length = 4 // Version
                     + 4; // SaveIndex;
        }
    }
}