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
    public sealed class ByteArray : BlockStructure
    {
        private byte[] _data;

        private ByteArray(GameSave gameSave, Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        internal static ByteArray FromSave(GameSave gameSave, Tuple<int, uint> address)
        {
            ByteArray byteArray;
            if (!gameSave.TryGetFromBlockStructureCache(address, out byteArray))
            {
                byteArray = new ByteArray(gameSave, address);
                gameSave.AddToBlockStructureCache(address, byteArray);
            }
            return byteArray;
        }

        public ByteArray(GameSave gameSave, byte[] data) : base(gameSave)
        {
            _data = new byte[data.Length];
            data.CopyTo(_data, 0);
        }

        public byte[] Data
        {
            get
            {
                Load();
                var data = new byte[_data.Length];
                _data.CopyTo(data, 0);
                return data;
            }
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get { return new BlockStructure[0]; }
        }

        protected override void ReadData(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            _data = reader.ReadBytes(length);
        }

        protected override void ForgetData()
        {
            _data = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(_data.Length);
            writer.Write(_data);
        }

        protected override void UpdateLength()
        {
            Length = 4 // data length
                     + _data.Length; // data
        }
    }
}