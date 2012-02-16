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
using System.Threading.Tasks;
using MarsMiner.Saving.Interfaces;
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
        private const uint NullPointer = 0;

        private const uint GlobalPointerFlag = 0x80000000;
        private const uint PointerDataMask = uint.MaxValue ^ GlobalPointerFlag;
        private const int StringFileVersion = 0;
        private const int PointerFileVersion = 0;

        private FileStream _pointerFile;
        private List<Tuple<int, uint>> _pointers;

        private FileStream _stringFile;
        private readonly Dictionary<uint, string> _stringsByAddress;
        private readonly Dictionary<string, uint> _addressByString;

        private FileStream[] _blobFiles;
        private IntRangeList[] _freeSpace;

        private GameSave()
        {
            _addressByString = new Dictionary<string, uint>();
            _stringsByAddress = new Dictionary<uint, string>();
        }

        internal void FlushFiles()
        {
            //TODO?: Remove these?
            UpdatePointerFileHeader();
            UpdateStringFileHeader();

            _pointerFile.Flush();
            _stringFile.Flush();
            foreach (FileStream blobFile in _blobFiles)
            {
                blobFile.Flush();
            }
        }

        internal FileStream GetBlobFile(int x)
        {
            return _blobFiles[x];
        }

        internal void MarkFreeSpace<T>(T header) where T : BlockStructure, IHeader
        {
#if DebugVerboseSpace
            PrintUsedSpace();
            Console.WriteLine("MarkFreeSpace called.");
#endif

            for (int i = 0; i < _freeSpace.Length; i++)
            {
                _freeSpace[i] = new IntRangeList();
                _freeSpace[i] += new Tuple<int, int>(8, (int) _blobFiles[i].Length);
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
            for (int b = 0; b < blobFiles.Length; b++)
            {
                Console.Write("Blob {0}: ", b);

                var fs = freeSpace[b];

                var max = blobFiles[b].Length;

                var used = 0;

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
                Console.WriteLine("Used ratio: {0}", (float)used / max);
            }
        }
#endif

        internal Tuple<int, uint> ResolvePointer(int sourceBlob, uint pointer)
        {
            if ((pointer & GlobalPointerFlag) == 0)
            {
                return new Tuple<int, uint>(sourceBlob, pointer);
            }
            return _pointers[(int) (pointer & PointerDataMask)];
        }

        internal string ResolveString(uint address)
        {
            string s;
            if (_stringsByAddress.TryGetValue(address, out s))
            {
                return s;
            }

            _stringFile.Seek(address, SeekOrigin.Begin);
            var r = new BinaryReader(_stringFile);

            s = r.ReadString();

            _addressByString.Add(s, address);
            _stringsByAddress.Add(address, s);

            return s;
        }

        private void UpdateStringFileHeader()
        {
            _stringFile.Seek(0, SeekOrigin.Begin);

            var w = new BinaryWriter(_stringFile);
            w.Write(StringFileVersion);
            w.Write(_stringsByAddress.Count);
        }

        private void UpdatePointerFileHeader()
        {
            _pointerFile.Seek(0, SeekOrigin.Begin);

            var w = new BinaryWriter(_pointerFile);
            w.Write(PointerFileVersion);
            w.Write(0); //Reserved
        }

        internal uint FindBlockPointer(BlockStructure source, BlockStructure target)
        {
            if (source.Address.Item1 == target.Address.Item1)
            {
#if DebugVerbosePointers
                Console.WriteLine("Local: {0} → {1}", source.Address, target.Address);
#endif
                return target.Address.Item2;
            }

            for (int i = 0; i < _pointers.Count; i++)
            {
                if (_pointers[i] == target.Address)
                {
#if DebugVerbosePointers
                    Console.WriteLine("Global {2}: {0} → {1}", source.Address, target.Address, i);
#endif
                    return GlobalPointerFlag | (uint) i;
                }
            }

#if DebugVerbosePointers
            Console.WriteLine("New global {2}: {0} → {1}", source.Address, target.Address, pointers.Count - 1);
#endif
            return GlobalPointerFlag | AddGlobalPointer(target.Address);
        }

        private uint AddGlobalPointer(Tuple<int, uint> target)
        {
            _pointers.Add(target);
            _pointerFile.Seek(_pointerFile.Length, SeekOrigin.Begin);
            var w = new BinaryWriter(_pointerFile);

            w.Write(target.Item1);
            w.Write(target.Item2);

            return GlobalPointerFlag | (uint) (_pointers.Count - 1);
        }

        internal uint FindStringAddress(string s)
        {
            if (s == null)
            {
                return NullPointer;
            }

            uint address;
            if (_addressByString.TryGetValue(s, out address))
                return address;

            return AddString(s);
        }

        private uint AddString(string s)
        {
            _stringFile.Seek(_stringFile.Length, SeekOrigin.Begin);

            var address = (uint) _stringFile.Position;

            var w = new BinaryWriter(_stringFile);
            w.Write(s);

            _stringsByAddress.Add(address, s);
            _addressByString.Add(s, address);

            return address;
        }

        internal void BindBlock(BlockStructure blockStructure)
        {
            if (blockStructure.Address != null)
            {
                throw new ArgumentException("blockStructure.Address already set!");
            }

            if (blockStructure.UnboundBlocks.Length != 0)
            {
                throw new ArgumentException("blockStructure has unbound referenced blocks!");
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

            newBlobFiles[newBlobIndex].Write(new byte[8], 0, 8);

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
                                   _stringFile =
                                       File.Open(Path.Combine(path, "strings"), FileMode.CreateNew, FileAccess.ReadWrite,
                                                 FileShare.None)
                               };


            gameSave._stringFile.Write(new byte[8], 0, 8);

            gameSave._pointerFile = File.Open(Path.Combine(path, "pointers"), FileMode.CreateNew, FileAccess.ReadWrite,
                                              FileShare.None);
            gameSave._pointerFile.Write(new byte[8], 0, 8);
            gameSave._pointers = new List<Tuple<int, uint>>();

            gameSave._blobFiles = new[]
                                      {
                                          File.Open(Path.Combine(path, "blob0"), FileMode.CreateNew,
                                                    FileAccess.ReadWrite,
                                                    FileShare.None)
                                      };
            gameSave._freeSpace = new[] { new IntRangeList() };
            gameSave._blobFiles[0].Write(new byte[8], 0, 8);

            return gameSave;
        }

        /// <summary>
        /// Opens an existing save from a specified directory.<para />
        /// This method does not mark available free space!
        /// </summary>
        /// <param name="path">The directory where the existing save is located.<para />
        /// Must contain an existing save.</param>
        /// <returns>A new GameSave instance managing the existing save.</returns>
        public static GameSave Open(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Directory doesn't exist!");
            }

            string stringsPath = Path.Combine(path, "strings");
            string pointersPath = Path.Combine(path, "pointers");
            string blobsPath = Path.Combine(path, "blob");

            var gameSave = new GameSave
                               {
                                   _path = path,
                                   _stringFile =
                                       File.Open(stringsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None),
                                   _pointerFile =
                                       File.Open(pointersPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                               };


            gameSave.ReadPointers();

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

            return gameSave;
        }

        private void ReadPointers()
        {
            _pointerFile.Seek(0, SeekOrigin.Begin);
            var r = new BinaryReader(_pointerFile);

            int version = r.ReadInt32();
            if (version != 0)
            {
                throw new InvalidDataException("Invalid pointer file version!");
            }

            _pointerFile.Seek(4, SeekOrigin.Current); // Reserved

            _pointers = new List<Tuple<int, uint>>();
            while (_pointerFile.Position < _pointerFile.Length)
            {
                _pointers.Add(new Tuple<int, uint>(
                                  r.ReadInt32(),
                                  r.ReadUInt32()));
            }
        }

        /// <summary>
        /// Disposes all open FileStream instances.<para />The GameSave instance can't be used for reading or writing after Close() is called.
        /// </summary>
        public void Close()
        {
            _pointerFile.Dispose();
            _stringFile.Dispose();
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