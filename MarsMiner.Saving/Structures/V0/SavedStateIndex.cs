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
using MarsMiner.Saving.Interface.V0;
using MarsMiner.Saving.Interfaces;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Structures.V0
{
    public class SavedStateIndex : IBlockStructure
    {
        private Tuple<int, uint> _address;
        private Dictionary<int, IntRangeList> _recursiveUsedSpace;

        public SavedStateIndex(long timestamp, string saveName, ChunkTable chunkTable)
        {
            Timestamp = timestamp;
            SaveName = saveName;
            ChunkTable = chunkTable;
        }

        private SavedStateIndex(long timestamp, string saveName, ChunkTable chunkTable, Tuple<int, uint> address)
            : this(timestamp, saveName, chunkTable)
        {
            Address = address;
            CalculateRecursiveUsedSpace();
        }

        public long Timestamp { get; private set; }
        public string SaveName { get; private set; }
        public ChunkTable ChunkTable { get; private set; }

        #region IBlockStructure Members

        public IBlockStructure[] UnboundBlocks
        {
            get
            {
                if (ChunkTable == null)
                {
                    //Unloaded
                    return new IBlockStructure[0];
                }

                return ChunkTable.Address == null ? new IBlockStructure[] {ChunkTable} : new IBlockStructure[0];
            }
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
        }

        public int Length
        {
            get
            {
                return 8 // timestamp
                       + 4 // saveName
                       + 4; // chunkTable
            }
        }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc,
                          Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            long start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write(Timestamp);
            w.Write(getStringPointerFunc(SaveName));
            w.Write(getBlockPointerFunc(this, ChunkTable));
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in SavedStateIndex!");
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
            ChunkTable = null;
        }

        #endregion

        private void CalculateRecursiveUsedSpace()
        {
            if (_recursiveUsedSpace != null) return;

            _recursiveUsedSpace = new Dictionary<int, IntRangeList>();
            _recursiveUsedSpace.Add(ChunkTable.RecursiveUsedSpace);

            if (!_recursiveUsedSpace.ContainsKey(Address.Item1))
            {
                _recursiveUsedSpace[Address.Item1] = new IntRangeList();
            }
            _recursiveUsedSpace[Address.Item1].Add(new Tuple<int, int>((int) Address.Item2, (int) Address.Item2 + Length));
        }

        public static SavedStateIndex Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc,
                                           Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc,
                                           ReadOptions readOptions)
        {
#if DebugVerboseBlocks
            Console.WriteLine("Reading {0} from {1}", "SavedStateIndex", source);
#endif

            Stream stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            long timestamp = r.ReadInt64();
            uint saveNameAddress = r.ReadUInt32();
            uint chunkTablePointer = r.ReadUInt32();

#if DebugVerboseBlocks || AssertBlockLength
            long end = stream.Position;
#endif

            var newSavedStateIndex = new SavedStateIndex(
                timestamp,
                resolveStringFunc(saveNameAddress),
                ChunkTable.Read(resolvePointerFunc(source.Item1, chunkTablePointer), resolvePointerFunc,
                                resolveStringFunc, getStreamFunc, readOptions),
                source);

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", "SavedStateIndex", newSavedStateIndex.Address, newSavedStateIndex.Address.Item2 + newSavedStateIndex.Length, end);
#endif
#if AssertBlockLength
            if (newSavedStateIndex.Address.Item2 + newSavedStateIndex.Length != end)
            {
                throw new Exception("Length mismatch in SavedStateIndex!");
            }
#endif

            return newSavedStateIndex;
        }
    }
}