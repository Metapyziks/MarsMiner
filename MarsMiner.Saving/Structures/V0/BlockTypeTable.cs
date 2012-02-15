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
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class BlockTypeTable : BlockStructure
    {
        private int[] _blockSubTypes;
        private string[] _blockTypeNames;

        private Dictionary<int, IntRangeList> _recursiveUsedSpace;

        public BlockTypeTable(GameSave gameSave, string[] blockTypeNames, int[] blockSubTypes) : base(gameSave)
        {
            if (blockTypeNames.Length != blockSubTypes.Length)
            {
                throw new ArgumentException("blockTypeNames and blockSubTypes must have the same length.");
            }

            _blockTypeNames = blockTypeNames;
            _blockSubTypes = blockSubTypes;

            UpdateLength();
        }

        protected override void UpdateLength()
        {
            Length = 2 // block type count
                     + (4 + 4) * _blockTypeNames.Length; // block type names and subtypes
        }

        public BlockTypeTable(GameSave gameSave, Tuple<string, int>[] blockTypes)
            : this(gameSave,
                   blockTypes.Select(x => x.Item1).ToArray(),
                   blockTypes.Select(x => x.Item2).ToArray())
        {
        }

        public Tuple<string, int> this[int index]
        {
            get { return new Tuple<string, int>(_blockTypeNames[index], _blockSubTypes[index]); }
        }

        //TODO: Split and move into BlockStructure
        private void CalculateRecursiveUsedSpace()
        {
            if (_recursiveUsedSpace != null) return;

            _recursiveUsedSpace = new Dictionary<int, IntRangeList>();

            if (!_recursiveUsedSpace.ContainsKey(Address.Item1))
            {
                _recursiveUsedSpace[Address.Item1] = new IntRangeList();
            }
            _recursiveUsedSpace[Address.Item1] += new Tuple<int, int>((int) Address.Item2, (int) Address.Item2 + Length);
        }

        protected override void ReadData(BinaryReader reader)
        {
            ushort blockTypeNameCount = reader.ReadUInt16();
            var blockTypeNameAddresses = new uint[blockTypeNameCount];
            _blockSubTypes = new int[blockTypeNameCount];
            for (int i = 0; i < blockTypeNameCount; i++)
            {
                blockTypeNameAddresses[i] = reader.ReadUInt32();
                _blockSubTypes[i] = reader.ReadInt32();
            }

            _blockTypeNames = new string[blockTypeNameCount];

            for (int i = 0; i < blockTypeNameCount; i++)
            {
                _blockTypeNames[i] = _gameSave.ResolveString(blockTypeNameAddresses[i]);
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
                writer.Write(_gameSave.FindStringAddress(_blockTypeNames[i]));
                writer.Write(_blockSubTypes[i]);
            }
        }
    }
}