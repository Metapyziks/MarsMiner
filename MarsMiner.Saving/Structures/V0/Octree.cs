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
    internal class Octree : IBlockStructure
    {
        private BitArray octreeFlags;
        private byte[] octreeValues;

        //TODO: accessors

        public Octree(BitArray octreeFlags, byte[] octreeValues)
        {
            this.octreeFlags = octreeFlags;
            this.octreeValues = octreeValues;
        }

        #region IBlockStructure
        public int Length
        {
            get
            {
                return 4 // octreeFlags length
                    + 4 // octreeValueList length
                    + (octreeFlags.Length / 8) + (octreeFlags.Length % 8 == 0 ? 0 : 1) // octreeFlags
                    + octreeValues.Length; // octreeValues
            }
        }

        public void Write(Stream stream, Func<object, uint> getPointerFunc)
        {
            {
                //DEBUG
                Console.WriteLine("Write octree at " + stream.Position);
            }
            var w = new BinaryWriter(stream);

            int octreeFlagsLength = (octreeFlags.Length / 8) + (octreeFlags.Length % 8 == 0 ? 0 : 1);

            w.Write(octreeFlagsLength);

            w.Write(octreeValues.Length);

            var buffer = new byte[octreeFlagsLength];
            octreeFlags.CopyTo(buffer, 0);
            w.Write(buffer);

            w.Write(octreeValues);
        }
        #endregion

        public static Octree Read(Tuple<Stream, int> source, Func<Stream, uint, Tuple<Stream, int>> resolvePointerFunc, Func<uint, string> resolveStringFunc)
        {
            {
                //DEBUG
                Console.WriteLine("Read octree at " + source.Item2);
            }
            source.Item1.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(source.Item1);

            var octreeFlagsLength = r.ReadInt32();
            var octreeValuesLength = r.ReadInt32();

            var octreeFlags = new BitArray(r.ReadBytes(octreeFlagsLength));
            var octreeValues = r.ReadBytes(octreeValuesLength);

            return new Octree(octreeFlags, octreeValues);
        }
    }
}
