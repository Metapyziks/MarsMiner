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
using System.IO;

namespace MarsMiner.Saving.Structures.V0
{
    internal class PointerFileIndex
    {
        private FilePointer[] pointers;

        public FilePointer this[int index]
        {
            get { return pointers[index]; }
        }

        //TODO: constructor

        #region IBlockStructure
        public int Length
        {
            get
            {
                return 4 // file pointer count
                    + 8 * pointers.Length; // file pointers
            }
        }

        public void Write(Stream stream, Func<object, uint> getPointerFunc)
        {
            var w = new BinaryWriter(stream);

            w.Write(pointers.Length);
            foreach (var pointer in pointers)
            {
                w.Write(getPointerFunc(pointer.Filename));
                w.Write(pointer.Address);
            }
        }
        #endregion
    }
}
