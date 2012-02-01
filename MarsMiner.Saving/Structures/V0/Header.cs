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
    internal class Header : IBlockStructure
    {
        public const int Version = 0;
        private SavedStateIndex mainIndex;

        public Header(SavedStateIndex mainIndex)
        {
            this.mainIndex = mainIndex;
        }

        public SavedStateIndex MainVersion { get { return mainIndex; } }

        #region IBlockStructure
        public int Length
        {
            get { return 8; }
        }

        public void Write(Stream stream, Func<object, uint> getPointerFunc)
        {
            var w = new BinaryWriter(stream);

            w.Write(Version);
            w.Write(getPointerFunc(mainIndex));
        }
        #endregion

        public static Header Read(Tuple<Stream, int> source, Func<uint, Tuple<Stream, int>> resolvePointerFunc, Func<uint, string> resolveStringFunc)
        {
            source.Item1.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(source.Item1);

            var version = r.ReadInt32();
            if (version != Version)
            {
                throw new InvalidDataException("Expected file version " + Version + ", was " + version + ".");
            }

            var mainIndexPointer = r.ReadUInt32();

            var mainIndex = SavedStateIndex.Read(resolvePointerFunc(mainIndexPointer), resolvePointerFunc, resolveStringFunc);

            return new Header(mainIndex);
        }
    }
}
