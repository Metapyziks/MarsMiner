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
    internal class Header
    {
        private const int version = 0;
        private Pointer<SavedStateIndex> mainIndex;

        public Header(Pointer<SavedStateIndex> mainIndex)
        {
            this.mainIndex = mainIndex;
        }

        public int Version { get { return version; } }
        public Pointer<SavedStateIndex> MainVersion { get { return mainIndex; } }

        //TODO: serializing
    }
}
