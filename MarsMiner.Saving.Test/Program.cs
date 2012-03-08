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
using System.IO;
using System.Threading;
using MarsMiner.Saving.Structures.V0;

namespace MarsMiner.Saving.Test
{
    internal static class Program
    {
        private static void Main()
        {
            Console.BufferWidth = 1000;
            Console.BufferHeight = Int16.MaxValue - 1;

            string savePath = "." + Path.DirectorySeparatorChar + "TestSave";
            savePath = Path.GetFullPath(savePath);

            if (Directory.Exists(savePath))
            {
                Directory.Delete(savePath, true);
            }

            CreateWrite(savePath);
            Thread.Sleep(100);
            OpenRead(savePath);
            Thread.Sleep(100);
            for (int i = 0; i < 30; i++)
            {
                OpenReadUnloadModifyWrite(savePath);
                Thread.Sleep(100);
            }
            OpenReadRewrite(savePath);
        }

        private static void OpenReadRewrite(string savePath)
        {
            Console.WriteLine("Opening...");
            GameSave gameSave;
            Header header;

            GameSave.Open(savePath, out gameSave, out header);
            Console.WriteLine("...OK");

            Console.WriteLine("Rewriting...");
            header.Write(true);
            Console.WriteLine("...OK");

            Console.WriteLine("Closing...");
            gameSave.Close();
            Console.WriteLine("...OK");
        }

// ReSharper disable UnusedMember.Local
        private static void OpenReadMarkWrite(string savePath)
// ReSharper restore UnusedMember.Local
        {
            Console.WriteLine("Opening...");
            GameSave gameSave;
            Header header;
            GameSave.Open(savePath, out gameSave, out header);
            Console.WriteLine("...OK");

            Console.WriteLine("Marking and modifying...");
            Tests.TestMarkModify(gameSave, header);
            Console.WriteLine("...OK");

            Console.WriteLine("Closing...");
            gameSave.Close();
            Console.WriteLine("...OK");
        }

        private static void OpenReadUnloadModifyWrite(string savePath)
        {
            Console.WriteLine("Opening...");
            GameSave gameSave;
            Header header;
            GameSave.Open(savePath, out gameSave, out header);
            Console.WriteLine("...OK");

            Console.WriteLine("Unloading and modifying...");
            Tests.TestAddChunkToUnloadedChunks(gameSave, header);
            Console.WriteLine("...OK");

            Console.WriteLine("Closing...");
            gameSave.Close();
            Console.WriteLine("...OK");
        }

// ReSharper disable UnusedMember.Local
        private static void OpenReadWrite(string savePath)
// ReSharper restore UnusedMember.Local
        {
            Console.WriteLine("Opening...");
            GameSave gameSave;
            Header header;
            GameSave.Open(savePath, out gameSave, out header);
            Console.WriteLine("...OK");

            Console.WriteLine("Modifying...");
            Tests.TestModify(gameSave, header);
            Console.WriteLine("...OK");

            Console.WriteLine("Closing...");
            gameSave.Close();
            Console.WriteLine("...OK");
        }

        private static void CreateWrite(string savePath)
        {
            Console.WriteLine("Creating...");
            GameSave gameSave = GameSave.Create(savePath);
            Console.WriteLine("...OK");

            Console.WriteLine("Writing...");
            Tests.TestSaving(gameSave, "Test Save");
            Tests.TestSaving(gameSave, "Test Save2");
            Tests.TestSaving(gameSave, "Test Save");
            Tests.TestSaving(gameSave, "Test Save3");
            Console.WriteLine("...OK");

            Console.WriteLine("Closing...");
            gameSave.Close();
            Console.WriteLine("...OK");
        }

        private static void OpenRead(string savePath)
        {
            Console.WriteLine("Opening...");
            GameSave gameSave;
            Header header;
            GameSave.Open(savePath, out gameSave, out header);
            Console.WriteLine("...OK");

            Console.WriteLine("Reading...");
            Tests.TestReading(header);
            Console.WriteLine("...OK");

            Console.WriteLine("Closing...");
            gameSave.Close();
            Console.WriteLine("...OK");
        }
    }
}