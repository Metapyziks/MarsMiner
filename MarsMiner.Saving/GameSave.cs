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
using MarsMiner.Saving.Cache;
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

        private Queue<WriteTransaction> writeQueue;

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

        internal void WriteTransaction(WriteTransaction transaction)
        {
            var blocksToWrite = transaction.Blocks.Where(b => b.Address == null).ToArray();
            foreach (var block in blocksToWrite)
            {
                AllocateSpace(block);
            }

            foreach (var block in blocksToWrite)
            {
                WriteBlock(block);
            }

            UpdatePointerFileHeader();
            UpdateStringFileHeader();

            pointerFile.Flush();
            stringFile.Flush();
            foreach (var blobFile in blobFiles)
            {
                blobFile.Flush();
            }

            {
                //Write header
                blobFiles[0].Seek(0, SeekOrigin.Begin);
                transaction.Header.Write(blobFiles[0], FindBlockPointer, FindStringAddress);
                blobFiles[0].Flush();
            }
        }

        internal T Read<T>(Func<Tuple<Stream, int>, Func<Stream, uint, Tuple<Stream, int>>, Func<uint, string>, T> readFunc)
        {
            return readFunc(new Tuple<Stream, int>(blobFiles[0], 0), ResolvePointer, ResolveString);
        }

        private Tuple<Stream, int> ResolvePointer(Stream stream, uint pointer)
        {
            if ((pointer & GlobalPointerFlag) == 0)
            {
                return new Tuple<Stream, int>(stream, (int)pointer);
            }
            else
            {
                var pointerIndex = (int)(pointer & PointerDataMask);
                var globalPointer = pointers[pointerIndex];
                return new Tuple<Stream, int>(blobFiles[globalPointer.Item1], (int)globalPointer.Item2);
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
            var blockBlob = blobFiles[block.Address.Item1];

            blockBlob.Seek(block.Address.Item2, SeekOrigin.Begin);

            block.Write(blockBlob, FindBlockPointer, FindStringAddress);
        }

        private uint FindBlockPointer(IBlockStructure source, IBlockStructure target)
        {
            if (source.Address.Item1 == target.Address.Item1)
            {
                return target.Address.Item2;
            }

            for (int i = 0; i < pointers.Count; i++)
            {
                if (pointers[i] == target.Address)
                {
                    return GlobalPointerFlag | (uint)i;
                }
            }

            pointers.Add(target.Address);
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

        private uint GetPointerTo(Tuple<int, uint> target)
        {
            var pointerIds = pointers.Where(x => x.Item1 == target.Item1 && x.Item2 == target.Item2).Select(x => (uint)pointers.IndexOf(x)).ToArray();

            if (pointerIds.Length > 0)
            {
                return GlobalPointerFlag | pointerIds[0];
            }


            //TODO: Put this somewhere else?
            //TODO: Find unused space
            pointerFile.Seek(pointerFile.Length, SeekOrigin.Begin);
            var w = new BinaryWriter(pointerFile);

            w.Write(target.Item1);
            w.Write(target.Item2);

            pointers.Add(target);

            return GlobalPointerFlag | (uint)(pointers.Count - 1);
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

                bestMatch = new Tuple<int, Tuple<int, int>>(newBlobIndex,
                    new Tuple<int, int>(
                        (int)blobFiles[newBlobIndex].Length,
                        (int)blobFiles[newBlobIndex].Length + blockLength));
            }

            AllocateSpace(bestMatch.Item1, bestMatch.Item2);

            blockStructure.Address = new Tuple<int, uint>(bestMatch.Item1, (uint)bestMatch.Item2.Item1);
        }

        private void AllocateSpace(int fileIndex, Tuple<int, int> spaceArea)
        {
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
