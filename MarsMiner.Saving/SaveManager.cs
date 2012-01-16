using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace MarsMiner.Saving
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SaveManager : IDisposable
    {
        bool disposed = false;
        bool closed = false;

        public void Close()
        {
            closed = true;
        }

        public void IDisposable.Dispose()
        {
            if (disposed) { return; }

            Close();

            disposed = true;
            GC.SuppressFinalize(this);
        }

        ~SaveManager()
        {
            if (!disposed)
            {
                // It's not good if an instance of this class isn't disposed properly.
                Debug.Fail("SaveManager not disposed!", "SaveManager must be closed to ensure that all data is written to the save file!");

                Console.Beep(); // Beeps only if the error message doesn't work.
            }
        }
    }
}
