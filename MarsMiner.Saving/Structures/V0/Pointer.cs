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

namespace MarsMiner.Saving.Structures.V0
{
    internal struct Pointer<T>
    {
        private const uint IsGlobalMask = 0x80000000;
        private const uint DestinationMask = 0x7FFFFFFF;

        private uint pointer;

        public Pointer(bool isGlobal, uint destination)
        {
            pointer = isGlobal ? IsGlobalMask : 0 |
                destination & DestinationMask;
        }

        public bool IsGlobal
        {
            get { return (pointer & IsGlobalMask) == IsGlobalMask; }
        }

        public uint Destination
        {
            get { return (pointer & DestinationMask); }
        }

        //TODO: serializing
    }
}
