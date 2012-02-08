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

using System.Collections.Generic;

namespace MarsMiner.Saving.Util
{
    internal static class Extensions
    {
        public static void Add(this Dictionary<int, IntRangeList> rangeLists0, Dictionary<int, IntRangeList> rangeLists1)
        {
            foreach (var k in rangeLists1.Keys)
            {
                if (!rangeLists0.ContainsKey(k))
                {
                    rangeLists0[k] = new IntRangeList();
                }
                rangeLists0[k].Add(rangeLists1[k]);
            }
        }
    }
}