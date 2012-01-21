﻿/**
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
        const int MaximumBlockStartAddress = 100000000;
        private const uint NullPointer = 0;

        private const uint GlobalPointerFlag = 0x80000000;

        private Queue<WriteTransaction> writeQueue;

        private FileStream pointerFile;
        List<Tuple<int, uint>> pointers;

        private FileStream stringFile;
        Dictionary<uint, string> stringsByAddress;
        Dictionary<string, uint> addressByString;

        private FileStream[] blobFiles;
        private IntRangeList[] freeSpace;

        private void WriteTransaction(WriteTransaction transaction)
        {
            var addresses = transaction.Blocks.ToDictionary(x => x, AllocateSpace);

            foreach (var block in transaction.Blocks)
            {
                WriteBlock(block, addresses);
            }

            //TODO: Finish saving
        }

        private void WriteBlock(IBlockStructure block, Dictionary<IBlockStructure, Tuple<int, uint>> addresses)
        {
            var blockAddress = addresses[block];

            var blockBlob = blobFiles[blockAddress.Item1];

            blockBlob.Seek(blockAddress.Item2, SeekOrigin.Begin);

            block.Write(blockBlob, o =>
            {
                {
                    var s = o as string;
                    if (s != null)
                    {
                        uint address;
                        if (!addressByString.TryGetValue(s, out address))
                        {
                            address = AddString(s);
                        }
                        return address;
                    }
                }
                {
                    var b = o as IBlockStructure;
                    if (b != null)
                    {
                        var bAddress = addresses[b];

                        if (blockAddress.Item1 == bAddress.Item1)
                        {
                            //Same file
                            return bAddress.Item2;
                        }
                        else
                        {
                            return GetPointerTo(bAddress);
                        }
                    }
                }

                if (o == null)
                {
                    return NullPointer;
                }

                throw new ArgumentException("Tried to find address for object that was neither a string nor an IBlockStructure!");
            });
        }

        private uint GetPointerTo(Tuple<int, uint> target)
        {
            var pointerIds = pointers.Where(x => x.Item1 == target.Item1 && x.Item2 == target.Item2).Select(x => (uint)pointers.IndexOf(x)).ToArray();

            if (pointerIds.Length > 0)
            {
                return GlobalPointerFlag | pointerIds[0];
            }


            //TODO: Put this somewhere else?
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

        private Tuple<int, uint> AllocateSpace(IBlockStructure blockStructure)
        {
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
                    }
                }
            }

            AllocateSpace(bestMatch.Item1, bestMatch.Item2);

            return new Tuple<int, uint>(bestMatch.Item1, (uint)bestMatch.Item2.Item1);
        }

        private void AllocateSpace(int fileIndex, Tuple<int, int> spaceArea)
        {
            freeSpace[fileIndex].Subtract(spaceArea);
        }
    }
}
