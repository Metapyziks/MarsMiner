/**
 * Copyright (c) 2012 Tamme Schichler
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 * 
 * Tamme Schichler [tammeschichler@googlemail.com]
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
