﻿/**
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

namespace MarsMiner.Saving.Structures.V0
{
    internal class BlockTypeTable : IBlockStructure
    {
        private string[] blockTypeNames;

        public string this[int index]
        {
            get { return blockTypeNames[index]; }
        }

        //TODO: constructor

        #region IBlockStructure
        public int Length
        {
            get { throw new NotImplementedException(); }
        }

        public IBlockStructure[] GetTargets()
        {
            return new IBlockStructure[0];
        }

        public void Write(System.IO.Stream stream, Func<object, uint> getPointerFunc)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
