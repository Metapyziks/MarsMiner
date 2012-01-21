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
        const int MaximumBlockStartAddress = 100000000;

        private Queue<WriteTransaction> writeQueue;

        private FileStream pointerFile;
        private FileStream stringFile;

        private FileStream[] blobFiles;
        private IntRangeList[] freeSpace;

        private void WriteTransaction(WriteTransaction transaction)
        {
            var addresses = transaction.Blocks.ToDictionary(x => x, AllocateSpace);
        }

        private Tuple<int, int> AllocateSpace(IBlockStructure blockStructure)
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

            return new Tuple<int, int>(bestMatch.Item1, bestMatch.Item2.Item1);
        }

        private void AllocateSpace(int fileIndex, Tuple<int, int> spaceArea)
        {
            freeSpace[fileIndex].Subtract(spaceArea);
        }
    }
}
