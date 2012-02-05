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
using System.Collections;
using MarsMiner.Saving.Interfaces;
using System.IO;

namespace MarsMiner.Saving.Structures.V0
{
    public class Octree : IBlockStructure
    {
        private BitArray octreeFlags;
        private byte[] octreeValues;

        public IBlockStructure[] UnboundBlocks
        {
            get
            {
                return new IBlockStructure[0];
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

        //TODO: accessors

        public Octree(BitArray octreeFlags, byte[] octreeValues)
        {
            this.octreeFlags = octreeFlags;
            this.octreeValues = octreeValues;

            Length = 4 // octreeFlags length
                    + 4 // octreeValueList length
                    + (octreeFlags.Length / 8) + (octreeFlags.Length % 8 == 0 ? 0 : 1) // octreeFlags
                    + octreeValues.Length; // octreeValues
        }

        private Octree(BitArray octreeFlags, byte[] octreeValues, Tuple<int, uint> address)
            : this(octreeFlags, octreeValues)
        {
            Address = address;
        }

        public int Length { get; private set; }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc, Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            var start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            int octreeFlagsLength = (octreeFlags.Length / 8) + (octreeFlags.Length % 8 == 0 ? 0 : 1);

            w.Write(octreeFlagsLength);

            w.Write(octreeValues.Length);

            var buffer = new byte[octreeFlagsLength];
            octreeFlags.CopyTo(buffer, 0);
            w.Write(buffer);

            w.Write(octreeValues);
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in Octree!");
            }
#endif
        }

        public static Octree Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc, Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc, ReadOptions readOptions)
        {
            var stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            var octreeFlagsLength = r.ReadInt32();
            var octreeValuesLength = r.ReadInt32();

            var octreeFlags = new BitArray(r.ReadBytes(octreeFlagsLength));
            var octreeValues = r.ReadBytes(octreeValuesLength);

            return new Octree(octreeFlags, octreeValues, source);
        }

        public void Unload()
        {
            if (Address == null)
            {
                throw new InvalidOperationException("Can't unload unbound blocks!");
            }

            octreeFlags = null;
            octreeValues = null;
        }
    }
}
