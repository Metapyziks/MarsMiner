using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarsMiner.Saving.Interfaces;

#if AssertCorrectSaving
using System.Diagnostics;
#endif

namespace MarsMiner.Saving.Cache
{
    internal class BlockWriteCache
    {
        public byte[] Data { get; private set; }

#if AssertCorrectSaving
        List<int> blanks;
#endif

        //TODO: public event Action<Pointer

        public BlockWriteCache(byte[] data, IEnumerable<KeyValuePair<int, IBlockStructure>> pointers)
        {

        }

        private void FillBlank(int offset, byte[] data)
        {
#if AssertCorrectSaving
            Debug.Assert(blanks.Contains(offset));
#endif
            data.CopyTo(Data, offset);
#if AssertCorrectSaving
            blanks.Remove(offset);
#endif
        }
    }
}
