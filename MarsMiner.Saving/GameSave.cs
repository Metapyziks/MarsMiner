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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MarsMiner.Saving.Common;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving
{
    /// <summary>
    /// Each GameSave instance is a handle for a saved game on the disk.
    /// </summary>
    public class GameSave
    {
        private string _path;

        private bool _closed;

        private const int MaximumBlockStartAddress = 100000000;
        //const int MaximumBlockStartAddress = 100; // for testing blob creation

        private const int HeaderLength = 12;

        private FileStream[] _blobFiles;
        private IntRangeList[] _freeSpace;

        internal void FlushFiles()
        {
            foreach (FileStream blobFile in _blobFiles)
            {
                blobFile.Flush();
            }
        }

        internal FileStream GetBlobFile(int x)
        {
            return _blobFiles[x];
        }


        internal void MarkFreeSpace<T>(T header) where T : BlockStructure
        {
            if (header is IHeader == false)
            {
                throw new ArgumentException(header.GetType() + " doesn't implement IHeader", "header");
            }

#if DebugVerboseSpace
            PrintUsedSpace();
            Console.WriteLine("MarkFreeSpace called.");
#endif

            for (int i = 0; i < _freeSpace.Length; i++)
            {
                _freeSpace[i] = new IntRangeList();
                _freeSpace[i] += new Tuple<int, int>(HeaderLength, (int) _blobFiles[i].Length);
            }

            foreach (var kv in header.RecursiveUsedSpace)
            {
                _freeSpace[kv.Key] -= kv.Value;
            }

#if DebugVerboseSpace
            PrintUsedSpace();
#endif
        }

#if DebugVerboseSpace
        private void PrintUsedSpace()
        {
            for (int b = 0; b < _blobFiles.Length; b++)
            {
                Console.Write("Blob {0}: ", b);

                IntRangeList fs = _freeSpace[b];

                long max = _blobFiles[b].Length;

                int used = 0;

#if DebugDrawUsedSpace
                for (int i = 0; i < max; i++)
                {
                    if (Console.CursorLeft >= 100)
                    {
                        Console.WriteLine();
                    }

                    if (fs.Contains(i))
                    {
                        Console.Write("_");
                    }
                    else
                    {
                        Console.Write("#");
                        used++;
                    }
                }
                Console.WriteLine();
#else
                for (int i = 0; i < max; i++)
                {
                    if (!fs.Contains(i))
                    {
                        used++;
                    }
                }
#endif
                Console.WriteLine("Used ratio: {0}", (float) used / max);
            }
        }
#endif

        internal void BindBlock(BlockStructure blockStructure)
        {
            if (blockStructure.Address != null)
            {
                throw new ArgumentException("blockStructure.Address already set!");
            }

            int blockLength = blockStructure.Length;

            Tuple<int, Tuple<int, int>> bestMatch = null;

            for (int fileIndex = 0; fileIndex < _freeSpace.Length; fileIndex++)
            {
                foreach (var freeArea in _freeSpace[fileIndex].Items)
                {
                    int freeAreaLength = freeArea.Item2 - freeArea.Item1;

                    if (freeAreaLength < blockLength)
                    {
                        continue;
                    }

                    if (bestMatch != null && bestMatch.Item2.Item2 - bestMatch.Item2.Item1 < freeAreaLength)
                    {
                        continue;
                    }

                    bestMatch = new Tuple<int, Tuple<int, int>>(fileIndex, freeArea);
                }
            }
            if (bestMatch != null)
            {
                //Resize to fit block
                bestMatch = new Tuple<int, Tuple<int, int>>(bestMatch.Item1,
                                                            new Tuple<int, int>(bestMatch.Item2.Item1,
                                                                                bestMatch.Item2.Item1 + blockLength));
            }

            if (bestMatch == null)
            {
                for (int fileIndex = 0; fileIndex < _blobFiles.Length; fileIndex++)
                {
                    if (_blobFiles[fileIndex].Length < MaximumBlockStartAddress)
                    {
                        bestMatch = new Tuple<int, Tuple<int, int>>(
                            fileIndex,
                            new Tuple<int, int>(
                                (int) _blobFiles[fileIndex].Length,
                                (int) _blobFiles[fileIndex].Length + blockLength));

                        _blobFiles[fileIndex].SetLength(_blobFiles[fileIndex].Length + blockLength);

                        break;
                    }
                }
            }

            if (bestMatch == null)
            {
                int newBlobIndex = AddNewBlob();

                bestMatch = new Tuple<int, Tuple<int, int>>(newBlobIndex,
                                                            new Tuple<int, int>(
                                                                (int) _blobFiles[newBlobIndex].Length,
                                                                (int) _blobFiles[newBlobIndex].Length + blockLength));
            }

            if (bestMatch.Item2.Item2 - bestMatch.Item2.Item1 != blockLength)
            {
                throw new Exception("Area mismatch! Wanted" + blockLength + "bytes, but got " +
                                    (bestMatch.Item2.Item2 - bestMatch.Item2.Item1) + " bytes.");
            }

            AllocateSpace(bestMatch.Item1, bestMatch.Item2);

            blockStructure.Address = new Tuple<int, uint>(bestMatch.Item1, (uint) bestMatch.Item2.Item1);
        }

        private int AddNewBlob()
        {
            int newBlobIndex = _blobFiles.Length;

            var newBlobFiles = new FileStream[_blobFiles.Length + 1];
            _blobFiles.CopyTo(newBlobFiles, 0);

            var newFreeSpace = new IntRangeList[_freeSpace.Length + 1];
            _freeSpace.CopyTo(newFreeSpace, 0);

            newBlobFiles[newBlobIndex] = File.Open(Path.Combine(_path, "blob" + newBlobIndex), FileMode.CreateNew,
                                                   FileAccess.ReadWrite, FileShare.None);
            newFreeSpace[newBlobIndex] = new IntRangeList();

            newBlobFiles[newBlobIndex].Write(new byte[HeaderLength], 0, HeaderLength);

            _blobFiles = newBlobFiles;
            _freeSpace = newFreeSpace;
            return newBlobIndex;
        }

        private void AllocateSpace(int fileIndex, Tuple<int, int> spaceArea)
        {
            while (_blobFiles[fileIndex].Length < spaceArea.Item2)
            {
                long oldlength = _blobFiles[fileIndex].Length;
                _blobFiles[fileIndex].SetLength(oldlength + spaceArea.Item2);
                _freeSpace[fileIndex] += new Tuple<int, int>((int) oldlength, (int) _blobFiles[fileIndex].Length);
            }
            _freeSpace[fileIndex] -= spaceArea;
        }

        /// <summary>
        /// Creates a new empty save in a specified directory.
        /// </summary>
        /// <param name="path">The directory where the new save is created.<para />Must be empty or nonexistent.</param>
        /// <returns>A new GameSave instance managing the new save.</returns>
        public static GameSave Create(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.GetFiles(path).Length != 0)
            {
                throw new ArgumentException("Directory isn't empty!");
            }

            var gameSave = new GameSave
                               {
                                   _path = path,
                                   _blobFiles = new[]
                                                    {
                                                        File.Open(Path.Combine(path, "blob0"), FileMode.CreateNew,
                                                                  FileAccess.ReadWrite,
                                                                  FileShare.None)
                                                    },
                                   _freeSpace = new[] { new IntRangeList() }
                               };

            gameSave._blobFiles[0].Write(new byte[HeaderLength], 0, HeaderLength);

            return gameSave;
        }

        /// <summary>
        /// <para>Opens an existing save from a specified directory.</para>
        /// <para>This method does not mark available free space!</para>
        /// </summary>
        /// <typeparam name="T">A header block type.</typeparam>
        /// <param name="path"><para>The directory where the existing save is located.</para>
        /// <para>Must contain an existing save.</para></param>
        /// <param name="gameSave">A new GameSave instance managing the existing save.</param>
        /// <param name="header">The header of the existing save.</param>
        public static void Open<T>(string path, out GameSave gameSave, out T header) where T : BlockStructure, IHeader
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Directory doesn't exist!");
            }

            string blobsPath = Path.Combine(path, "blob");

            gameSave = new GameSave
                           {
                               _path = path
                           };

            var blobFiles = new LinkedList<FileStream>();

            for (int i = 0; i < int.MaxValue; i++)
            {
                string blobPath = blobsPath + i;
                if (!File.Exists(blobPath))
                {
                    break;
                }

                blobFiles.AddLast(File.Open(blobPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
            }

            gameSave._blobFiles = blobFiles.ToArray();

            {
                gameSave._freeSpace = new IntRangeList[gameSave._blobFiles.Length];
                for (int i = 0; i < gameSave._freeSpace.Length; i++)
                {
                    gameSave._freeSpace[i] = new IntRangeList();
                }
            }

            {
                // Mark free space and read strings
                ConstructorInfo headerConstructorInfo =
                    typeof (T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                              null,
                                              new[] { typeof (GameSave) }, null);

                if (headerConstructorInfo == null) throw new Exception("Header constructor not found.");

                header = (T) (headerConstructorInfo.Invoke(new object[] { gameSave }));
                gameSave.MarkFreeSpace(header);
            }
        }

        /// <summary>
        /// Disposes all open FileStream instances.<para />
        /// The GameSave instance can't be used for reading or writing after Close() is called.
        /// </summary>
        public void Close()
        {
            foreach (FileStream blob in _blobFiles)
            {
                blob.Dispose();
            }

            _closed = true;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        ~GameSave()
        {
            if (!_closed)
            {
                new Task(() => { throw new InvalidOperationException("GameSave not closed!"); }).Start();
            }
        }
    }
}