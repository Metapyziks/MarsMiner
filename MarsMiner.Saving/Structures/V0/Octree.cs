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
using MarsMiner.Saving.Interface.V0;
using MarsMiner.Saving.Interfaces;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Structures.V0
{
    public class Octree : IBlockStructure
    {
        private Tuple<int, uint> _address;
        private BitArray _octreeFlags;
        private byte[] _octreeValues;

        private Dictionary<int, IntRangeList> _recursiveUsedSpace;

        //TODO: accessors

        public Octree(BitArray octreeFlags, byte[] octreeValues)
        {
            _octreeFlags = octreeFlags;
            _octreeValues = octreeValues;

            Length = 4 // octreeFlags length
                     + 4 // octreeValueList length
                     + (octreeFlags.Length/8) + (octreeFlags.Length%8 == 0 ? 0 : 1) // octreeFlags
                     + octreeValues.Length; // octreeValues
        }

        private Octree(BitArray octreeFlags, byte[] octreeValues, Tuple<int, uint> address)
            : this(octreeFlags, octreeValues)
        {
            Address = address;
            CalculateRecursiveUsedSpace();
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
        }

        public int Length { get; private set; }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc,
                          Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            long start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            int octreeFlagsLength = (_octreeFlags.Length/8) + (_octreeFlags.Length%8 == 0 ? 0 : 1);

            w.Write(octreeFlagsLength);

            w.Write(_octreeValues.Length);

            var buffer = new byte[octreeFlagsLength];
            _octreeFlags.CopyTo(buffer, 0);
            w.Write(buffer);

            w.Write(_octreeValues);
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in Octree!");
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
            _octreeFlags = null;
            _octreeValues = null;
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
            _recursiveUsedSpace[Address.Item1].Add(new Tuple<int, int>((int) Address.Item2, (int) Address.Item2 + Length));
        }

        public static Octree Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc,
                                  Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc,
                                  ReadOptions readOptions)
        {
#if DebugVerboseBlocks
            Console.WriteLine("Reading {0} from {1}", "Octree", source);
#endif

            Stream stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            int octreeFlagsLength = r.ReadInt32();
            int octreeValuesLength = r.ReadInt32();

            var octreeFlags = new BitArray(r.ReadBytes(octreeFlagsLength));
            byte[] octreeValues = r.ReadBytes(octreeValuesLength);

#if DebugVerboseBlocks || AssertBlockLength
            long end = stream.Position;
#endif

            var newOctree = new Octree(octreeFlags, octreeValues, source);

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", "Octree", newOctree.Address, newOctree.Address.Item2 + newOctree.Length, end);
#endif
#if AssertBlockLength
            if (newOctree.Address.Item2 + newOctree.Length != end)
            {
                throw new Exception("Length mismatch in Octree!");
            }
#endif

            return newOctree;
        }
    }
}