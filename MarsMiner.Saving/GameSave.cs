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
using MarsMiner.Saving.Util;
using MarsMiner.Saving.Interfaces;

namespace MarsMiner.Saving
{
    /// <summary>
    /// Each GameSave instance is a handle for a saved game on the disk.
    /// </summary>
    public class GameSave
    {
        private string path;

        private bool closing = false;
        private bool closed = false;

        const int MaximumBlockStartAddress = 100000000;
        //const int MaximumBlockStartAddress = 100; // for testing blob creation
        private const uint NullPointer = 0;

        private const uint GlobalPointerFlag = 0x80000000;
        private const uint PointerDataMask = uint.MaxValue ^ GlobalPointerFlag;

        private FileStream pointerFile;
        List<Tuple<int, uint>> pointers;

        private FileStream stringFile;
        private IntRangeList freeStringSpace;
        private Dictionary<uint, string> stringsByAddress;
        private Dictionary<string, uint> addressByString;

        private FileStream[] blobFiles;
        private IntRangeList[] freeSpace;

        private GameSave()
        {
            freeStringSpace = new IntRangeList();
            addressByString = new Dictionary<string, uint>();
            stringsByAddress = new Dictionary<uint, string>();
        }

        public void Write(IBlockStructure block, bool unload)
        {
            var unboundReferenced = block.UnboundBlocks;
            foreach (var b in unboundReferenced)
            {
                Write(b, unload);
            }

            if (block.Address == null)
            {
                // Unbound
                AllocateSpace(block);
            }

            var blockAsHeader = block as IHeader;
            if (blockAsHeader == null)
            {
                WriteBlock(block);
            }
            else
            {
                // block is a header, update files before writing

                UpdatePointerFileHeader();
                UpdateStringFileHeader();

                // Write barrier
                pointerFile.Flush();
                stringFile.Flush();
                foreach (var blobFile in blobFiles)
                {
                    blobFile.Flush();
                }

                WriteBlock(block);

                blobFiles[0].Flush(); // Don't cache header write

                MarkFreeSpace(blockAsHeader);
            }

            if (unload)
            {
                block.Unload();
            }
        }

        public T Read<T, RO>(Func<Tuple<int, uint>, Func<int, uint, Tuple<int, uint>>, Func<uint, string>, Func<int, Stream>, RO, T> readFunc, RO readOptions) where T : IHeader
        {
            T header = readFunc(new Tuple<int, uint>(0, 0), ResolvePointer, ResolveString, x => blobFiles[x], readOptions);
            MarkFreeSpace(header);
            return header;
        }

        private void MarkFreeSpace<T>(T header) where T : IHeader
        {
#if DebugVerboseSpace
            PrintUsedSpace();
            Console.WriteLine("MarkFreeSpace called.");
#endif

            for (int i = 0; i < freeSpace.Length; i++)
            {
                freeSpace[i] = new IntRangeList();
                freeSpace[i].Add(new Tuple<int, int>(8, (int)blobFiles[i].Length));
            }

            foreach (var kv in header.RecursiveUsedSpace)
            {
                freeSpace[kv.Key].Subtract(kv.Value);
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

        private Tuple<int, uint> ResolvePointer(int sourceBlob, uint pointer)
        {
            if ((pointer & GlobalPointerFlag) == 0)
            {
                return new Tuple<int, uint>(sourceBlob, pointer);
            }
            else
            {
                var pointerIndex = (int)(pointer & PointerDataMask);
                var globalPointer = pointers[pointerIndex];
                return globalPointer;
            }
        }

        private string ResolveString(uint address)
        {
            string s;
            if (stringsByAddress.TryGetValue(address, out s))
            {
                return s;
            }
            else
            {
                stringFile.Seek(address, SeekOrigin.Begin);
                var r = new BinaryReader(stringFile);

                s = r.ReadString();

                addressByString.Add(s, address);
                stringsByAddress.Add(address, s);

                return s;
            }
        }

        private void UpdateStringFileHeader()
        {
            stringFile.Seek(0, SeekOrigin.Begin);

            var w = new BinaryWriter(stringFile);
            w.Write((int)0); //TODO: Move string file version to constant.
            w.Write((int)stringsByAddress.Count);
        }

        private void UpdatePointerFileHeader()
        {
            pointerFile.Seek(0, SeekOrigin.Begin);

            var w = new BinaryWriter(pointerFile);
            w.Write((int)0); //TODO: Move pointer file version to constant.
            w.Write((int)0); //Reserved
        }

        private void WriteBlock(IBlockStructure block)
        {
#if DebugVerboseBlocks
            Console.WriteLine("Writing {0} from {1} to {2}", block.GetType().FullName.Split('.').Last(), block.Address, block.Address.Item2 + block.Length);
#endif

            var blockBlob = blobFiles[block.Address.Item1];

            blockBlob.Seek(block.Address.Item2, SeekOrigin.Begin);

            block.Write(blockBlob, FindBlockPointer, FindStringAddress);
        }

        private uint FindBlockPointer(IBlockStructure source, IBlockStructure target)
        {
            if (source.Address.Item1 == target.Address.Item1)
            {
#if DebugVerbosePointers
                Console.WriteLine("Local: {0} → {1}", source.Address, target.Address);
#endif
                return target.Address.Item2;
            }

            for (int i = 0; i < pointers.Count; i++)
            {
                if (pointers[i] == target.Address)
                {
#if DebugVerbosePointers
                    Console.WriteLine("Global {2}: {0} → {1}", source.Address, target.Address, i);
#endif
                    return GlobalPointerFlag | (uint)i;
                }
            }
            
#if DebugVerbosePointers
            Console.WriteLine("New global {2}: {0} → {1}", source.Address, target.Address, pointers.Count - 1);
#endif
            return GlobalPointerFlag | AddGlobalPointer(target.Address);
        }

        private uint AddGlobalPointer(Tuple<int, uint> target)
        {
            pointers.Add(target);
            pointerFile.Seek(pointerFile.Length, SeekOrigin.Begin);
            var w = new BinaryWriter(pointerFile);

            w.Write(target.Item1);
            w.Write(target.Item2);

            return GlobalPointerFlag | (uint)(pointers.Count - 1);
        }

        private uint FindStringAddress(string s)
        {
            if (s == null)
            {
                return NullPointer;
            }

            uint address;
            if (addressByString.TryGetValue(s, out address))
                return address;

            return AddString(s);
        }

        private uint AddString(string s)
        {
            stringFile.Seek(stringFile.Length, SeekOrigin.Begin);

            var address = (uint)stringFile.Position;

            var w = new BinaryWriter(stringFile);
            w.Write(s);

            stringsByAddress.Add(address, s);
            addressByString.Add(s, address);

            return address;
        }

        private void AllocateSpace(IBlockStructure blockStructure)
        {
            if (blockStructure.Address != null)
            {
                throw new ArgumentException("blockStructure.Address already set!");
            }

            if (blockStructure.UnboundBlocks.Length != 0)
            {
                throw new ArgumentException("blockStructure has unbound referenced blocks!");
            }

            var blockLength = blockStructure.Length;

            Tuple<int, Tuple<int, int>> bestMatch = null;

            for (int fileIndex = 0; fileIndex < freeSpace.Length; fileIndex++)
            {
                foreach (var freeArea in freeSpace[fileIndex].Items)
                {
                    var freeAreaLength = freeArea.Item2 - freeArea.Item1;

                    if (freeAreaLength < blockLength) { continue; }

                    if (bestMatch != null && bestMatch.Item2.Item2 - bestMatch.Item2.Item1 < freeAreaLength) { continue; }

                    bestMatch = new Tuple<int, Tuple<int, int>>(fileIndex, freeArea);
                }
            }
            if (bestMatch != null)
            {
                //Resize to fit block
                bestMatch = new Tuple<int, Tuple<int, int>>(bestMatch.Item1, new Tuple<int, int>(bestMatch.Item2.Item1, bestMatch.Item2.Item1 + blockLength));
            }

            if (bestMatch == null)
            {
                for (int fileIndex = 0; fileIndex < blobFiles.Length; fileIndex++)
                {
                    if (blobFiles[fileIndex].Length < MaximumBlockStartAddress)
                    {
                        bestMatch = new Tuple<int, Tuple<int, int>>(
                            fileIndex,
                            new Tuple<int, int>(
                                (int)blobFiles[fileIndex].Length,
                                (int)blobFiles[fileIndex].Length + blockLength));

                        blobFiles[fileIndex].SetLength(blobFiles[fileIndex].Length + blockLength);

                        break;
                    }
                }
            }

            if (bestMatch == null)
            {
                int newBlobIndex = AddNewBlob();

                bestMatch = new Tuple<int, Tuple<int, int>>(newBlobIndex,
                    new Tuple<int, int>(
                        (int)blobFiles[newBlobIndex].Length,
                        (int)blobFiles[newBlobIndex].Length + blockLength));
            }

            if (bestMatch.Item2.Item2 - bestMatch.Item2.Item1 != blockLength)
            {
                throw new Exception("Area mismatch! Wanted" + blockLength + "bytes, but got " + (bestMatch.Item2.Item2 - bestMatch.Item2.Item1) + " bytes.");
            }

            AllocateSpace(bestMatch.Item1, bestMatch.Item2);

            blockStructure.Address = new Tuple<int, uint>(bestMatch.Item1, (uint)bestMatch.Item2.Item1);
        }

        private int AddNewBlob()
        {
            int newBlobIndex = blobFiles.Length;

            var newBlobFiles = new FileStream[blobFiles.Length + 1];
            blobFiles.CopyTo(newBlobFiles, 0);

            var newFreeSpace = new IntRangeList[freeSpace.Length + 1];
            freeSpace.CopyTo(newFreeSpace, 0);

            newBlobFiles[newBlobIndex] = File.Open(Path.Combine(path, "blob" + newBlobIndex), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            newFreeSpace[newBlobIndex] = new IntRangeList();

            newBlobFiles[newBlobIndex].Write(new byte[8], 0, 8);

            blobFiles = newBlobFiles;
            freeSpace = newFreeSpace;
            return newBlobIndex;
        }

        private void AllocateSpace(int fileIndex, Tuple<int, int> spaceArea)
        {
            while (blobFiles[fileIndex].Length < spaceArea.Item2)
            {
                var oldlength = blobFiles[fileIndex].Length;
                blobFiles[fileIndex].SetLength(oldlength + spaceArea.Item2);
                freeSpace[fileIndex].Add(new Tuple<int, int>((int)oldlength, (int)blobFiles[fileIndex].Length));
            }
            freeSpace[fileIndex].Subtract(spaceArea);
        }

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

            var gameSave = new GameSave();

            gameSave.path = path;

            gameSave.stringFile = File.Open(Path.Combine(path, "strings"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            gameSave.stringFile.Write(new byte[8], 0, 8);

            gameSave.pointerFile = File.Open(Path.Combine(path, "pointers"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            gameSave.pointerFile.Write(new byte[8], 0, 8);
            gameSave.pointers = new List<Tuple<int, uint>>();

            gameSave.blobFiles = new FileStream[] { File.Open(Path.Combine(path, "blob0"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None) };
            gameSave.freeSpace = new IntRangeList[] { new IntRangeList() };
            gameSave.blobFiles[0].Write(new byte[8], 0, 8);

            return gameSave;
        }

        public static GameSave Open(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Directory doesn't exist!");
            }

            var stringsPath = Path.Combine(path, "strings");
            var pointersPath = Path.Combine(path, "pointers");
            var blobsPath = Path.Combine(path, "blob");

            var gameSave = new GameSave();

            gameSave.path = path;

            gameSave.stringFile = File.Open(stringsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            gameSave.pointerFile = File.Open(pointersPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            gameSave.ReadPointers();

            var blobFiles = new LinkedList<FileStream>();

            for (int i = 0; i < int.MaxValue; i++)
            {
                var blobPath = blobsPath + i;
                if (!File.Exists(blobPath))
                {
                    break;
                }

                blobFiles.AddLast(File.Open(blobPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
            }

            gameSave.blobFiles = blobFiles.ToArray();

            {
                //TODO: Find free space
                gameSave.freeStringSpace = new IntRangeList();
                gameSave.freeSpace = new IntRangeList[gameSave.blobFiles.Length];
                for (int i = 0; i < gameSave.freeSpace.Length; i++)
                {
                    gameSave.freeSpace[i] = new IntRangeList();
                }
            }

            return gameSave;
        }

        private void ReadPointers()
        {
            pointerFile.Seek(0, SeekOrigin.Begin);
            var r = new BinaryReader(pointerFile);

            var version = r.ReadInt32();
            if (version != 0)
            {
                throw new InvalidDataException("Invalid pointer file version!");
            }
            var reserved = r.ReadInt32();

            pointers = new List<Tuple<int, uint>>();
            while (pointerFile.Position < pointerFile.Length)
            {
                pointers.Add(new Tuple<int, uint>(
                    r.ReadInt32(),
                    r.ReadUInt32()));
            }
        }

        public void Close()
        {
            closing = true;

            Flush();

            pointerFile.Dispose();
            stringFile.Dispose();
            foreach (var blob in blobFiles)
            {
                blob.Dispose();
            }

            closed = true;
        }

        private void Flush()
        {
            //TODO: Flush save
        }

        ~GameSave()
        {
            if (!closed)
            {
                new System.Threading.Tasks.Task(() => { throw new InvalidOperationException("GameSave not closed!"); }).Start();
            }
        }
    }
}
