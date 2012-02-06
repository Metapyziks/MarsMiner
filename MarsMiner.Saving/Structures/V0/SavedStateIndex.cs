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
using System.Linq;
using System.Text;
using MarsMiner.Saving.Interfaces;
using System.IO;
using MarsMiner.Saving.Cache;
using MarsMiner.Saving.Interface.V0;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Structures.V0
{
    public class SavedStateIndex : IBlockStructure
    {
        public long Timestamp { get; private set; }
        public string SaveName { get; private set; }
        public ChunkTable ChunkTable { get; private set; }

        public IBlockStructure[] UnboundBlocks
        {
            get
            {
                if (ChunkTable == null)
                {
                    //Unloaded
                    return new IBlockStructure[0];
                }

                return ChunkTable.Address == null ? new IBlockStructure[] { ChunkTable } : new IBlockStructure[0];
            }
        }

        private Tuple<int, uint> address;
        public Tuple<int, uint> Address
        {
            get
            {
                return address;
            }
            set
            {
                if (address != null)
                {
                    throw new InvalidOperationException("Address can't be reassigned!");
                }
                address = value;
            }
        }

        public void CalculateRecursiveUsedSpace()
        {
            recursiveUsedSpace = new Dictionary<int, IntRangeList>();
            recursiveUsedSpace.Add(ChunkTable.RecursiveUsedSpace);

            if (!recursiveUsedSpace.ContainsKey(Address.Item1))
            {
                recursiveUsedSpace[Address.Item1] = new IntRangeList();
            }
            recursiveUsedSpace[Address.Item1].Add(new Tuple<int, int>((int)Address.Item2, (int)Address.Item2 + Length));
        }

        private Dictionary<int, IntRangeList> recursiveUsedSpace;
        public Dictionary<int, IntRangeList> RecursiveUsedSpace
        {
            get
            {
                if (Address == null)
                {
                    throw new InvalidOperationException("Can't get used space from unbound block!");
                }
                if (recursiveUsedSpace == null)
                {
                    CalculateRecursiveUsedSpace();
                }
                return recursiveUsedSpace;
            }
            private set
            {
                recursiveUsedSpace = value;
            }
        }

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

        public int Length
        {
            get
            {
                return 8 // timestamp
                    + 4 // saveName
                    + 4; // chunkTable
            }
        }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc, Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            var start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write(Timestamp);
            w.Write(getStringPointerFunc(SaveName));
            w.Write((uint)getBlockPointerFunc(this, ChunkTable));
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in SavedStateIndex!");
            }
#endif
        }

        public static SavedStateIndex Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc, Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc, ReadOptions readOptions)
        {
            Console.WriteLine("Reading {0} from {1}", "SavedStateIndex", source);

            var stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            var timestamp = r.ReadInt64();
            var saveNameAddress = r.ReadUInt32();
            var chunkTablePointer = r.ReadUInt32();

            var end = stream.Position;

            SavedStateIndex newSavedStateIndex = new SavedStateIndex(
                            timestamp,
                            resolveStringFunc(saveNameAddress),
                            ChunkTable.Read(resolvePointerFunc(source.Item1, chunkTablePointer), resolvePointerFunc, resolveStringFunc, getStreamFunc, readOptions),
                            source);

            Console.WriteLine("Read {0} from {1} to {2} == {3}", "SavedStateIndex", newSavedStateIndex.Address, newSavedStateIndex.Address.Item2 + newSavedStateIndex.Length, end);

            return newSavedStateIndex;
        }

        public void Unload()
        {
            if (Address == null)
            {
                throw new InvalidOperationException("Can't unload unbound blocks!");
            }

            ChunkTable = null;
        }
    }
}
