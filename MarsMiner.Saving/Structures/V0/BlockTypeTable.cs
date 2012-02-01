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
    internal class BlockTypeTable : IBlockStructure
    {
        private string[] blockTypeNames;

        public string this[int index]
        {
            get { return blockTypeNames[index]; }
        }

        public BlockTypeTable(string[] blockTypeNames)
        {
            this.blockTypeNames = blockTypeNames;
        }

        #region IBlockStructure
        public int Length
        {
            get
            {
                return 2 // block type count
                    + 4 * blockTypeNames.Length; // block type names
            }
        }

        public void Write(Stream stream, Func<object, uint> getPointerFunc)
        {
            var w = new BinaryWriter(stream);

            w.Write((ushort)blockTypeNames.Length);
            foreach (var blockTypeName in blockTypeNames)
            {
                w.Write(getPointerFunc(blockTypeName));
            }
        }
        #endregion

        public static BlockTypeTable Read(Tuple<Stream, int> source, Func<Stream, uint, Tuple<Stream, int>> resolvePointerFunc, Func<uint, string> resolveStringFunc)
        {
            source.Item1.Seek(source.Item2, SeekOrigin.Begin);
            var r = new BinaryReader(source.Item1);

            var blockTypeNameCount = r.ReadUInt16();
            var blockTypeNameAddresses = new uint[blockTypeNameCount];
            for (int i = 0; i < blockTypeNameCount; i++)
            {
                blockTypeNameAddresses[i] = r.ReadUInt32();
            }

            var blockTypeNames = new string[blockTypeNameCount];

            for (int i = 0; i < blockTypeNameCount; i++)
            {
                blockTypeNames[i] = resolveStringFunc(blockTypeNameAddresses[i]);
            }

            return new BlockTypeTable(blockTypeNames);
        }
    }
}
