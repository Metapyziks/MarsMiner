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
using MarsMiner.Saving.Interface.V0;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Structures.V0
{
    public class Header : IBlockStructure, IHeader
    {
        public const int Version = 0;
        private SavedStateIndex mainIndex;

        public IBlockStructure[] UnboundBlocks
        {
            get
            {
                return mainIndex.Address == null ? new IBlockStructure[] { mainIndex } : new IBlockStructure[0];
            }
        }

        public Tuple<int, uint> Address
        {
            get { return new Tuple<int, uint>(0, 0); }
            set { throw new InvalidOperationException("Can't set address on Header!"); }
        }

        public Dictionary<int, IntRangeList> RecursiveUsedSpace
        {
            get
            {
                var usedSpace = new Dictionary<int, IntRangeList>();

                usedSpace.Add(mainIndex.RecursiveUsedSpace);

                if (!usedSpace.ContainsKey(Address.Item1))
                {
                    usedSpace[Address.Item1] = new IntRangeList();
                }
                usedSpace[Address.Item1].Add((int)Address.Item2, (int)Address.Item2 + Length);
                return usedSpace;
            }
        }

        public Header(SavedStateIndex mainIndex)
        {
            this.mainIndex = mainIndex;
        }

        public SavedStateIndex SaveIndex { get { return mainIndex; } }

        public int Length
        {
            get { return 8; }
        }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc, Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            var start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write(Version);
            w.Write(getBlockPointerFunc(this, mainIndex));
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in Header!");
            }
#endif
        }

        public static Header Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc, Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc, ReadOptions readOptions)
        {
            Console.WriteLine("Reading {0} from {1}", "Header", source);

            var stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            var version = r.ReadInt32();
            if (version != Version)
            {
                throw new InvalidDataException("Expected file version " + Version + ", was " + version + ".");
            }

            var mainIndexPointer = r.ReadUInt32();

            var end = stream.Position;

            var mainIndex = SavedStateIndex.Read(resolvePointerFunc(source.Item1, mainIndexPointer), resolvePointerFunc, resolveStringFunc, getStreamFunc, readOptions);

            Header newHeader = new Header(mainIndex);

            Console.WriteLine("Read {0} from {1} to {2} == {3}", "Header", newHeader.Address, newHeader.Address.Item2 + newHeader.Length, end);

            return newHeader;
        }

        public void Unload()
        {
            throw new InvalidOperationException("Can't unload the header!");
            CalculateRecursiveUsedSpace();
        }


        public void CalculateRecursiveUsedSpace()
        {
            //Do nothing.
        }
    }
}
