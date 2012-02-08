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

namespace MarsMiner.Saving.Util
{
    public class IntRangeList
    {
        //TODO: This can be made much faster with a sorted set.
        //TODO: Make immutable.

        private List<Tuple<int, int>> _ranges;

        public IntRangeList()
        {
            _ranges = new List<Tuple<int, int>>();
        }

        private IntRangeList(IntRangeList list)
        {
            _ranges = new List<Tuple<int, int>>(list._ranges);
        }

        public Tuple<int, int>[] Items
        {
            get { return _ranges.ToArray(); }
        }

        private void Add(int start, int end)
        {
            Add(new Tuple<int, int>(start, end));
        }

        private void Add(Tuple<int, int> range)
        {
            var newRanges = new List<Tuple<int, int>>();

            int newRangeStart = range.Item1;
            int newRangeEnd = range.Item2;

            foreach (var r in _ranges)
            {
                if (r.Item2 < newRangeStart || newRangeEnd < r.Item1)
                {
                    // No collision
                    newRanges.Add(r);
                }
                else
                {
                    // Collision, merge ranges
                    newRangeStart = Math.Min(r.Item1, newRangeStart);
                    newRangeEnd = Math.Max(r.Item2, newRangeEnd);
                }
            }

            newRanges.Add(new Tuple<int, int>(newRangeStart, newRangeEnd));

            _ranges = newRanges;
        }

        private void Add(IntRangeList rangeList)
        {
            foreach (var range in rangeList._ranges)
            {
                Add(range);
            }
        }

        private void Subtract(Tuple<int, int> range)
        {
            var newRanges = new List<Tuple<int, int>>();

            foreach (var r in _ranges)
            {
                if (r.Item2 < range.Item1 || range.Item2 < r.Item1)
                {
                    // No collision
                    newRanges.Add(r);
                }
                else
                {
                    if (r.Item1 < range.Item1)
                    {
                        // Start of r remains
                        newRanges.Add(new Tuple<int, int>(r.Item1, range.Item1));
                    }
                    if (range.Item2 < r.Item2)
                    {
                        // End of r remains
                        newRanges.Add(new Tuple<int, int>(range.Item2, r.Item2));
                    }
                }
            }

            _ranges = newRanges;
        }

        private void Subtract(IntRangeList rangeList)
        {
            foreach (var range in rangeList._ranges)
            {
                Subtract(range);
            }
        }

        public static IntRangeList operator +(IntRangeList list1, IntRangeList list2)
        {
            var newList = new IntRangeList(list1);
            newList.Add(list2);
            return newList;
        }

        public static IntRangeList operator +(IntRangeList list, Tuple<int, int> range)
        {
            var newList = new IntRangeList(list);
            newList.Add(range);
            return newList;
        }

        public static IntRangeList operator -(IntRangeList list1, IntRangeList list2)
        {
            var newList = new IntRangeList(list1);
            newList.Subtract(list2);
            return newList;
        }

        public static IntRangeList operator -(IntRangeList list, Tuple<int, int> range)
        {
            var newList = new IntRangeList(list);
            newList.Subtract(range);
            return newList;
        }

        public bool Contains(int integer)
        {
            return _ranges.Any(r => r.Item1 <= integer && r.Item2 > integer);
        }
    }
}