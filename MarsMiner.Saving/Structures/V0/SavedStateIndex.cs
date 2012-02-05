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

namespace MarsMiner.Saving.Structures.V0
{
    internal class SavedStateIndex : IBlockStructure
    {
        private long timestamp;
        private string saveName;
        private ChunkTable chunkTable;

        public IBlockStructure[] ReferencedBlocks
        {
            get
            {
                return new IBlockStructure[] { ChunkTable };
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

        public ChunkTable ChunkTable
        {
            get
            {
                return chunkTable;
            }
        }

        public SavedStateIndex(long timestamp, string saveName, ChunkTable chunkTable)
        {
            this.timestamp = timestamp;
            this.saveName = saveName;
            this.chunkTable = chunkTable;
        }

        private SavedStateIndex(long timestamp, string saveName, ChunkTable chunkTable, Tuple<int, uint> address)
            : this(timestamp, saveName, chunkTable)
        {
            Address = address;
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

            w.Write(timestamp);
            w.Write(getStringPointerFunc(saveName));
            uint getBlockPointerFunc1 = getBlockPointerFunc(this, ChunkTable);
            w.Write(getBlockPointerFunc1);
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in SavedStateIndex!");
            }
#endif
        }

        public static SavedStateIndex Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc, Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc)
        {
            var stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            var timestamp = r.ReadInt64();
            var saveNameAddress = r.ReadUInt32();
            var chunkTablePointer = r.ReadUInt32();

            return new SavedStateIndex(
                timestamp,
                resolveStringFunc(saveNameAddress),
                ChunkTable.Read(resolvePointerFunc(source.Item1, chunkTablePointer), resolvePointerFunc, resolveStringFunc, getStreamFunc),
                source);
        }
    }
}
