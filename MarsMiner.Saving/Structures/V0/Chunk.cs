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
    internal class Chunk : IBlockStructure
    {
        private BlockTypeTable blockTypeTable;
        private Octree[] octrees;

        //TODO: accessors

        public Chunk(BlockTypeTable blockTypeTable, Octree[] octrees)
        {
            this.blockTypeTable = blockTypeTable;
            this.octrees = octrees;
        }

        #region IBlockStructure
        public int Length
        {
            get
            {
                return 4 // blockTypeTable
                    + 1 // octreeCount
                    + 4 * octrees.Length; // octrees
            }
        }

        public void Write(Stream stream, Func<object, uint> getPointerFunc)
        {
            var w = new BinaryWriter(stream);

            w.Write(getPointerFunc(blockTypeTable));
            w.Write((byte)octrees.Length);
            foreach (var octree in octrees)
            {
                w.Write(getPointerFunc(octree));
            }
        }
        #endregion
    }
}
