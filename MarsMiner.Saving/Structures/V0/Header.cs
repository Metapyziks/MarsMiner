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
using MarsMiner.Saving.Common;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class Header : BlockStructure, IHeader
    {
        public const int Version = 0;
        private SavedStateIndex _saveIndex;

// Invoked via reflection
// ReSharper disable UnusedMember.Global
        internal Header(GameSave gameSave)
// ReSharper restore UnusedMember.Global
            : base(gameSave, new Tuple<int, uint>(0, 0))
        {
        }

        public Header(GameSave gameSave, SavedStateIndex saveIndex)
            : base(gameSave)
        {
            Address = new Tuple<int, uint>(0, 0);
            SaveIndex = saveIndex;

            UpdateLength();
        }

        public SavedStateIndex SaveIndex
        {
            get
            {
                Load();
                return _saveIndex;
            }
            private set { _saveIndex = value; }
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get
            {
                Load();
                return new BlockStructure[] { SaveIndex };
            }
        }

        protected override void ReadData(BinaryReader reader)
        {
            int version = reader.ReadInt32();
            if (version != Version)
            {
                throw new InvalidDataException("Expected file version " + Version + ", was " + version + ".");
            }

            SaveIndex = SavedStateIndex.FromSave(GameSave, ReadAddress(reader));
        }

        protected override void ForgetData()
        {
            SaveIndex = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(Version);
            WriteAddress(writer, SaveIndex.Address);
        }

        protected override void UpdateLength()
        {
            Length = 4 // Version
                     + 8; // SaveIndex;
        }
    }
}