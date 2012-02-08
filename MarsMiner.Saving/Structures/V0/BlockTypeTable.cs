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
using MarsMiner.Saving.Interface.V0;
using MarsMiner.Saving.Interfaces;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Structures.V0
{
    public class BlockTypeTable : IBlockStructure
    {
        private Tuple<int, uint> _address;
        private int[] _blockSubTypes;
        private string[] _blockTypeNames;

        private Dictionary<int, IntRangeList> _recursiveUsedSpace;

        public BlockTypeTable(string[] blockTypeNames, int[] blockSubTypes)
        {
            if (blockTypeNames.Length != blockSubTypes.Length)
            {
                throw new ArgumentException("blockTypeNames and blockSubTypes must have the same length.");
            }

            _blockTypeNames = blockTypeNames;
            _blockSubTypes = blockSubTypes;

            Length = 2 // block type count
                     + (4 + 4) * blockTypeNames.Length; // block type names and subtypes
        }

        private BlockTypeTable(string[] blockTypeNames, int[] blockSubTypes, Tuple<int, uint> address)
            : this(blockTypeNames, blockSubTypes)
        {
            Address = address;
            CalculateRecursiveUsedSpace();
        }

        public BlockTypeTable(Tuple<string, int>[] blockTypes)
            : this(
                blockTypes.Select(x => x.Item1).ToArray(),
                blockTypes.Select(x => x.Item2).ToArray())
        {
        }

        public Tuple<string, int> this[int index]
        {
            get { return new Tuple<string, int>(_blockTypeNames[index], _blockSubTypes[index]); }
        }

        #region IBlockStructure Members

        public IBlockStructure[] UnboundBlocks
        {
            get { return new IBlockStructure[0]; }
        }

        public Tuple<int, uint> Address
        {
            get { return _address; }
            set
            {
                if (_address != null)
                {
                    throw new InvalidOperationException("Address can't be reassigned!");
                }
                _address = value;
            }
        }

        public Dictionary<int, IntRangeList> RecursiveUsedSpace
        {
            get
            {
                if (Address == null)
                {
                    throw new InvalidOperationException("Can't get used space from unbound block!");
                }
                if (_recursiveUsedSpace == null)
                {
                    CalculateRecursiveUsedSpace();
                }
                return _recursiveUsedSpace;
            }
            set { _recursiveUsedSpace = value; }
        }

        public int Length { get; private set; }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc,
                          Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            long start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write((ushort)_blockTypeNames.Length);
            for (int i = 0; i < _blockTypeNames.Length; i++)
            {
                w.Write(getStringPointerFunc(_blockTypeNames[i]));
                w.Write(_blockSubTypes[i]);
            }
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in BlockTypeTable!");
            }
#endif
        }

        public void Unload()
        {
            if (Address == null)
            {
                throw new InvalidOperationException("Can't unload unbound blocks!");
            }

            CalculateRecursiveUsedSpace();
            _blockTypeNames = null;
            _blockSubTypes = null;
        }

        #endregion

        private void CalculateRecursiveUsedSpace()
        {
            if (_recursiveUsedSpace != null) return;

            _recursiveUsedSpace = new Dictionary<int, IntRangeList>();

            if (!_recursiveUsedSpace.ContainsKey(Address.Item1))
            {
                _recursiveUsedSpace[Address.Item1] = new IntRangeList();
            }
            _recursiveUsedSpace[Address.Item1] += new Tuple<int, int>((int)Address.Item2, (int)Address.Item2 + Length);
        }

        public static BlockTypeTable Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc,
                                          Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc,
                                          ReadOptions readOptions)
        {
#if DebugVerboseBlocks
            Console.WriteLine("Reading {0} from {1}", "BlockTypeTable", source);
#endif

            Stream stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            ushort blockTypeNameCount = r.ReadUInt16();
            var blockTypeNameAddresses = new uint[blockTypeNameCount];
            var blockSubtypes = new int[blockTypeNameCount];
            for (int i = 0; i < blockTypeNameCount; i++)
            {
                blockTypeNameAddresses[i] = r.ReadUInt32();
                blockSubtypes[i] = r.ReadInt32();
            }

#if DebugVerboseBlocks || AssertBlockLength
            long end = stream.Position;
#endif

            var blockTypeNames = new string[blockTypeNameCount];

            for (int i = 0; i < blockTypeNameCount; i++)
            {
                blockTypeNames[i] = resolveStringFunc(blockTypeNameAddresses[i]);
            }

            var newBlockTypeTable = new BlockTypeTable(blockTypeNames, blockSubtypes, source);

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", "BlockTypeTable", newBlockTypeTable.Address, newBlockTypeTable.Address.Item2 + newBlockTypeTable.Length, end);
#endif
#if AssertBlockLength
            if (newBlockTypeTable.Address.Item2 + newBlockTypeTable.Length != end)
            {
                throw new Exception("Length mismatch in BlockTypeTable!");
            }
#endif

            return newBlockTypeTable;
        }
    }
}