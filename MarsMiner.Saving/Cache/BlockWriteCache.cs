using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarsMiner.Saving.Interfaces;

#if AssertSaving
using System.Diagnostics;
#endif

namespace MarsMiner.Saving.Cache
{
    internal class BlockWriteCache
    {
        public byte[] Data { get; private set; }

#if AssertSaving
        List<int> blanks;
#endif

        //TODO: public event Action<Pointer

        public BlockWriteCache(byte[] data)
        {
            throw new NotImplementedException();
        }

        private void FillBlank(int offset, byte[] data)
        {
#if AssertSaving
            Debug.Assert(blanks.Contains(offset));
#endif
            data.CopyTo(Data, offset);
#if AssertSaving
            blanks.Remove(offset);
#endif
        }
    }
}
