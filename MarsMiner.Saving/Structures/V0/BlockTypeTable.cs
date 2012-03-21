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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarsMiner.Saving.Common;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class BlockTypeTable : UniqueBlockStructure<BlockTypeTable>, IEnumerable<Tuple<StringBlock, int>>
    {
        private int[] _blockSubTypes;
        private StringBlock[] _blockTypeNames;

        private BlockTypeTable(GameSave gameSave, Tuple<int, uint> address)
            : base(gameSave, address)
        {
        }

        internal static BlockTypeTable FromSave(GameSave gameSave, Tuple<int, uint> address)
        {
            BlockTypeTable blockTypeTable;
            if (!gameSave.TryGetFromBlockStructureCache(address, out blockTypeTable))
            {
                blockTypeTable = new BlockTypeTable(gameSave, address);
                gameSave.AddToBlockStructureCache(address, blockTypeTable);
            }
            return blockTypeTable;
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
            get
            {
                Load();
                return _blockSubTypes.Length;
            }
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get
            {
                Load();
                return _blockTypeNames.ToArray<BlockStructure>();
            }
        }

        protected override IEqualityComparer<UniqueBlockStructure<BlockTypeTable>> ValueComparer
        {
            get { return new BlockTypeTableComparer(); }
        }

        #region IEnumerable<Tuple<StringBlock,int>> Members

        public IEnumerator<Tuple<StringBlock, int>> GetEnumerator()
        {
            Load();
            for (int i = 0; i < _blockSubTypes.Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

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
                _blockTypeNames[i] = StringBlock.FromSave(GameSave, ReadAddress(reader));
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

        #region Nested type: BlockTypeTableComparer

        private class BlockTypeTableComparer : IEqualityComparer<UniqueBlockStructure<BlockTypeTable>>
        {
            #region IEqualityComparer<UniqueBlockStructure<BlockTypeTable>> Members

            public bool Equals(UniqueBlockStructure<BlockTypeTable> x, UniqueBlockStructure<BlockTypeTable> y)
            {
                var tableX = (BlockTypeTable) x;
                var tableY = (BlockTypeTable) y;

                if (tableX.Count != tableY.Count)
                {
                    return false;
                }

                int count = tableX.Count;
                var stringBlockComparer = new StringBlock.StringBlockEqualityComparer();
                for (int i = 0; i < count; i++)
                {
                    Tuple<StringBlock, int> bX = tableX[i];
                    Tuple<StringBlock, int> bY = tableY[i];
                    if (!(stringBlockComparer.Equals(bX.Item1, bY.Item1) && bX.Item2 == bY.Item2))
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(UniqueBlockStructure<BlockTypeTable> obj)
            {
                //TODO: This hash function is not very good, see http://stackoverflow.com/a/1079419/410020

                var table = (BlockTypeTable) obj;

                int result = 17;
                var stringBlockComparer = new StringBlock.StringBlockEqualityComparer();
                unchecked
                {
                    foreach (var item in table)
                    {
                        result = result * 31 + stringBlockComparer.GetHashCode(item.Item1);
                        result = result * 31 + item.Item2;
                    }
                }
                return result;
            }

            #endregion
        }

        #endregion
    }
}