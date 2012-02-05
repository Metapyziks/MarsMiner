using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
