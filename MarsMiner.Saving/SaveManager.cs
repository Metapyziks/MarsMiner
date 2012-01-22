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

        public void Flush()
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            if (disposed) { return; }

            Close();
            Flush();

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
