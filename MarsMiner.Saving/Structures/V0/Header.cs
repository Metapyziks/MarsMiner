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
    internal class Header : IBlockStructure
    {
        public const int Version = 0;
        private SavedStateIndex mainIndex;

        public IBlockStructure[] ReferencedBlocks
        {
            get
            {
                return new IBlockStructure[]{mainIndex};
            }
        }

        public Tuple<int, uint> Address
        {
            get { return new Tuple<int, uint>(0, 0); }
            set { throw new InvalidOperationException("Can't set address on Header!"); }
        }

        public Header(SavedStateIndex mainIndex)
        {
            this.mainIndex = mainIndex;
        }

        public SavedStateIndex SaveIndex { get { return mainIndex; } }

        #region IBlockStructure
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
        #endregion

        public static Header Read(Tuple<int, uint> source, Func<int, uint, Tuple<int, uint>> resolvePointerFunc, Func<uint, string> resolveStringFunc, Func<int, Stream> getStreamFunc)
        {
            var stream = getStreamFunc(source.Item1);
            stream.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(stream);

            var version = r.ReadInt32();
            if (version != Version)
            {
                throw new InvalidDataException("Expected file version " + Version + ", was " + version + ".");
            }

            var mainIndexPointer = r.ReadUInt32();

            var mainIndex = SavedStateIndex.Read(resolvePointerFunc(source.Item1, mainIndexPointer), resolvePointerFunc, resolveStringFunc, getStreamFunc);

            return new Header(mainIndex);
        }

        public WriteTransaction GetTransaction()
        {
            var blocks = new List<IBlockStructure>();
            var blockStack = new Stack<IBlockStructure>();

            blockStack.Push(SaveIndex);

            while (blockStack.Count > 0)
            {
                var block = blockStack.Pop();

                blocks.Add(block);
                foreach (var b in block.ReferencedBlocks)
                {
                    blockStack.Push(b);
                }
            }

            return new WriteTransaction(this, blocks.ToArray());
        }
    }
}
