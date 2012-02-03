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

namespace MarsMiner.Saving.Structures.V0
{
    internal class ChunkTable : IBlockStructure
    {
        private int[] xLocations;
        private int[] zLocations;
        private Chunk[] chunks;

        //TODO: accessors

        public ChunkTable(int[] xLocations, int[] zLocations, Chunk[] chunks)
        {
            if (xLocations.Length != zLocations.Length || zLocations.Length != chunks.Length)
            {
                throw new ArgumentException("Argument arrays must have the same length!");
            }
            this.xLocations = xLocations;
            this.zLocations = zLocations;
            this.chunks = chunks;
        }

        #region IBlockStructure
        public int Length
        {
            get
            {
                return 4 //chunk count

                    + chunks.Length *
                    (4 // xLocation
                    + 4 // yLocation
                    + 4); // chunk
            }
        }

        public void Write(Stream stream, Func<object, uint> getPointerFunc)
        {
#if AssertBlockLength
            var start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write((uint)chunks.LongLength);
            for (long i = 0; i < chunks.LongLength; i++)
            {
                w.Write(xLocations[i]);
                w.Write(zLocations[i]);
                var chunkPointer = getPointerFunc(chunks[i]);
                w.Write(chunkPointer);
            }
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in ChunkTable!");
            }
#endif
        }
        #endregion

        public static ChunkTable Read(Tuple<Stream, int> source, Func<Stream, uint, Tuple<Stream, int>> resolvePointerFunc, Func<uint, string> resolveStringFunc)
        {
            source.Item1.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(source.Item1);

            var chunkCount = r.ReadUInt32();

            var xLocations = new int[chunkCount];
            var yLocations = new int[chunkCount];
            var chunkPointers = new uint[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                xLocations[i] = r.ReadInt32();
                yLocations[i] = r.ReadInt32();
                chunkPointers[i] = r.ReadUInt32();
            }

            var chunks = new Chunk[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                chunks[i] = Chunk.Read(resolvePointerFunc(source.Item1, chunkPointers[i]), resolvePointerFunc, resolveStringFunc);
            }

            return new ChunkTable(xLocations, yLocations, chunks);
        }
    }
}
