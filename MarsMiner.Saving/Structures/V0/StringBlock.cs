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
using System.Text;
using MarsMiner.Saving.Common;
using System.IO;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class StringBlock : BlockStructure
    {
        private const int NullMarker = -1;

        private string _value;

        internal StringBlock(GameSave gameSave, Tuple<int, uint> address)
            : base(gameSave, address)
        {
        }

        public StringBlock(GameSave gameSave, string value)
            : base(gameSave)
        {
            _value = value;
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get { return new BlockStructure[0]; }
        }

        public string Value
        {
            get { return _value; }
        }

        protected override void ReadData(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length == -1)
            {
                _value = null;
                return;
            }

            var utf8Data = reader.ReadBytes(length);
            _value = Encoding.UTF8.GetString(utf8Data);
        }

        protected override void ForgetData()
        {
            _value = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            if (Value == null)
            {
                writer.Write(NullMarker);
                return;
            }

            var utf8Data = Encoding.UTF8.GetBytes(Value);
            writer.Write(utf8Data.Length);
            writer.Write(utf8Data);
        }

        protected override void UpdateLength()
        {
            if (Value == null)
            {
                Length = 4; // NullMarker
                return;
            }

            Length = 4 // Length
                     + Encoding.UTF8.GetByteCount(Value);
        }
    }
}
