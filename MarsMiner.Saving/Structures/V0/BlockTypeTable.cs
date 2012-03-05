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
    public sealed class BlockTypeTable : BlockStructure
    {
        private int[] _blockSubTypes;
        private StringBlock[] _blockTypeNames;

        internal BlockTypeTable(GameSave gameSave, Tuple<int, uint> address)
            : base(gameSave, address)
        {
        }

        public BlockTypeTable(GameSave gameSave, StringBlock[] blockTypeNames, int[] blockSubTypes)
            : base(gameSave)
        {
            if (blockTypeNames.Length != blockSubTypes.Length)
            {
                throw new ArgumentException("blockTypeNames and blockSubTypes must have the same length.");
            }

            _blockTypeNames = blockTypeNames;
            _blockSubTypes = blockSubTypes;

            UpdateLength();
        }

        public BlockTypeTable(GameSave gameSave, Tuple<StringBlock, int>[] blockTypes)
            : this(gameSave,
                   blockTypes.Select(x => x.Item1).ToArray(),
                   blockTypes.Select(x => x.Item2).ToArray())
        {
        }

        public Tuple<StringBlock, int> this[int index]
        {
            get
            {
                Load();
                return new Tuple<StringBlock, int>(_blockTypeNames[index], _blockSubTypes[index]);
            }
        }

        public int Count
        {
            get { return _blockSubTypes.Length; }
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get
            {
                Load();
                return _blockTypeNames.ToArray<BlockStructure>();
            }
        }

        protected override void UpdateLength()
        {
            Length = 2 // block type count
                     + _blockTypeNames.Length *
                     (8 // block type name
                      + 4); // subtype
        }

        protected override void ReadData(BinaryReader reader)
        {
            ushort blockTypeNameCount = reader.ReadUInt16();
            _blockSubTypes = new int[blockTypeNameCount];

            _blockTypeNames = new StringBlock[blockTypeNameCount];

            for (int i = 0; i < blockTypeNameCount; i++)
            {
                _blockTypeNames[i] = new StringBlock(GameSave, ReadAddress(reader));
                _blockSubTypes[i] = reader.ReadInt32();
            }
        }

        protected override void ForgetData()
        {
            _blockTypeNames = null;
            _blockSubTypes = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write((ushort) _blockTypeNames.Length);
            for (int i = 0; i < _blockTypeNames.Length; i++)
            {
                WriteAddress(writer, _blockTypeNames[i].Address);
                writer.Write(_blockSubTypes[i]);
            }
        }
    }
}