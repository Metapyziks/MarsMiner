﻿/**
 * Copyright (c) 2012 James King [metapyziks@gmail.com]
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

namespace MarsMiner.Shared.Octree
{
    public class FaceEnumerator : IEnumerator<Face>
    {
        private byte myBitmap;
        private int myShift;

        public Face Current
        {
            get { return Face.FromIndex( myShift ); }
        }

        object System.Collections.IEnumerator.Current
        {
            get { throw new NotImplementedException(); }
        }

        public FaceEnumerator( Face face )
        {
            myBitmap = (byte) face.GetHashCode();

            Reset();
        }

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            while ( ( myBitmap & ( 1 << ++myShift ) ) == 0 )
                if ( myShift >= 8 )
                    return false;

            return true;
        }

        public void Reset()
        {
            myShift = -1;
        }
    }
}
