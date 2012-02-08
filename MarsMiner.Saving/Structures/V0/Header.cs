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
    public class Header : IHeader
    {
        public const int Version = 0;
        private Dictionary<int, IntRangeList> _recursiveUsedSpace;

        public Header(SavedStateIndex saveIndex)
        {
            SaveIndex = saveIndex;
        }

        public SavedStateIndex SaveIndex { get; private set; }

        #region IHeader Members

        public IBlockStructure[] UnboundBlocks
        {
            get { return SaveIndex.Address == null ? new IBlockStructure[] { SaveIndex } : new IBlockStructure[0]; }
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
                if (_recursiveUsedSpace == null)
                {
                    CalculateRecursiveUsedSpace();
                }
                return _recursiveUsedSpace;
            }
        }

        public int Length
        {
            get { return 8; }
        }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc,
                          Func<string, uint> getStringPointerFunc)
        {
#if AssertBlockLength
            long start = stream.Position;
#endif
            var w = new BinaryWriter(stream);

            w.Write(Version);
            w.Write(getBlockPointerFunc(this, SaveIndex));
#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch in Header!");
            }
#endif
        }

        public void Unload()
        {
            if (Address == null || (SaveIndex != null && SaveIndex.Address == null))
            {
                throw new InvalidOperationException("Can't unload unbound blocks!");
            }

            CalculateRecursiveUsedSpace();
            SaveIndex = null;
        }

        #endregion

        public static Header Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc,
                                  Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc,
                                  ReadOptions readOptions)
        {
#if DebugVerboseBlocks
            Console.WriteLine("Reading {0} from {1}", "Header", source);
#endif

            Stream stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            int version = r.ReadInt32();
            if (version != Version)
            {
                throw new InvalidDataException("Expected file version " + Version + ", was " + version + ".");
            }

            uint mainIndexPointer = r.ReadUInt32();

#if DebugVerboseBlocks || AssertBlockLength
            long end = stream.Position;
#endif

            SavedStateIndex mainIndex = SavedStateIndex.Read(resolvePointerFunc(source.Item1, mainIndexPointer),
                                                             resolvePointerFunc, resolveStringFunc, getStreamFunc,
                                                             readOptions);

            var newHeader = new Header(mainIndex);

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", "Header", newHeader.Address, newHeader.Address.Item2 + newHeader.Length, end);
#endif
#if AssertBlockLength
            if (newHeader.Address.Item2 + newHeader.Length != end)
            {
                throw new Exception("Length mismatch in Header!");
            }
#endif

            return newHeader;
        }


        public void CalculateRecursiveUsedSpace()
        {
            if (_recursiveUsedSpace != null) return;

            _recursiveUsedSpace = new Dictionary<int, IntRangeList>();
            _recursiveUsedSpace.Add(SaveIndex.RecursiveUsedSpace);

            if (!_recursiveUsedSpace.ContainsKey(Address.Item1))
            {
                _recursiveUsedSpace[Address.Item1] = new IntRangeList();
            }
            _recursiveUsedSpace[Address.Item1] += new Tuple<int, int>((int)Address.Item2, (int)Address.Item2 + Length);
        }
    }
}