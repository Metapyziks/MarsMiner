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

namespace MarsMiner.Saving.Util
{
    public class IntRangeList
    {
        //TODO: This can be made much faster with a sorted set.

        private List<Tuple<int, int>> ranges = new List<Tuple<int, int>>();

        public void Add(int start, int end)
        {
            Add((Tuple<int, int>)new Tuple<int, int>(start, end));
        }

        public void Add(Tuple<int, int> range)
        {
            var newRanges = new List<Tuple<int, int>>();

            var newRangeStart = range.Item1;
            var newRangeEnd = range.Item2;

            foreach (var r in ranges)
            {
                if (r.Item2 < newRangeStart || newRangeEnd < r.Item1)
                {
                    newRanges.Add(r);
                }
                else
                {
                    newRangeStart = Math.Min(r.Item1, newRangeStart);
                    newRangeEnd = Math.Max(r.Item2, newRangeEnd);
                }
            }

            newRanges.Add(new Tuple<int, int>(newRangeStart, newRangeEnd));

            ranges = newRanges;
        }

        public void Subtract(Tuple<int, int> range)
        {
            var newRanges = new List<Tuple<int, int>>();

            foreach (var r in ranges)
            {
                if (r.Item2 < range.Item1 || range.Item2 < r.Item1)
                {
                    newRanges.Add(r);
                }
                else
                {
                    if (r.Item1 < range.Item1)
                    {
                        newRanges.Add(new Tuple<int, int>(r.Item1, range.Item1));
                    }
                    if (range.Item2 < r.Item2)
                    {
                        newRanges.Add(new Tuple<int, int>(range.Item2, r.Item2));
                    }
                }
            }

            ranges = newRanges;
        }

        public void Add(IntRangeList rangeList)
        {
            foreach (var range in rangeList.ranges)
            {
                Add(range);
            }
        }

        public void Subtract(IntRangeList rangeList)
        {
            foreach (var range in rangeList.ranges)
            {
                Subtract(range);
            }
        }

        public Tuple<int, int>[] Items
        {
            get
            {
                return ranges.ToArray();
            }
        }
    }
}
