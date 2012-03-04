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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsMiner.Saving.Common;
using MarsMiner.Saving.Structures.V0;

namespace MarsMiner.Saving.Test
{
    public static class Tests
    {
        public static void TestSaving(GameSave gameSave, string saveName)
        {
            var octree = new Octree(gameSave, new BitArray(new[] { false, false }), new byte[] { 1 });
            var blockTypeTable = new BlockTypeTable(gameSave,
                                                    new[] { new StringBlock(gameSave, "Block 0"),
                                                        new StringBlock(gameSave, "Block 1"),
                                                        new StringBlock(gameSave, "Block 2"),
                                                        new StringBlock(gameSave, "Block 2") },
                                                    new[] { 0, 0, 0, 1 });
            var chunk = new Chunk(gameSave, blockTypeTable, new[] { octree });
            var chunkTable = new ChunkTable(gameSave, new[] { 0 }, new[] { 0 }, new[] { chunk });
            var mainIndex = new SavedStateIndex(gameSave, DateTime.UtcNow.Ticks, new StringBlock(gameSave, saveName), chunkTable);
            var header = new Header(gameSave, mainIndex);

            header.Write(true);
        }

        public static void TestAddChunkToUnloadedChunks(GameSave gameSave, Header header)
        {
            ChunkTable oldChunkTable = header.SaveIndex.ChunkTable;

            var octree = new Octree(gameSave, new BitArray(new[] { false, false }), new byte[] { 1 });
            var blockTypeTable = new BlockTypeTable(gameSave,
                                                    new[] { new StringBlock(gameSave, "Block 0"),
                                                        new StringBlock(gameSave, "Block 1"),
                                                        new StringBlock(gameSave, "Block 2"),
                                                        new StringBlock(gameSave, "Block 2") },
                                                    new[] { 0, 0, 0, 1 });
            var chunk = new Chunk(gameSave, blockTypeTable, new[] { octree });

            var r = new Random();

            int x = r.Next();
            int z = r.Next();

            List<Tuple<int, int, Chunk>> chunks =
                oldChunkTable.GetChunks().Where(c => c.Item1 != x || c.Item2 != z).ToList();
            chunks.Add(new Tuple<int, int, Chunk>(x, z, chunk));

            var chunkTable = new ChunkTable(gameSave, chunks.ToArray());
            var mainIndex = new SavedStateIndex(gameSave, DateTime.UtcNow.Ticks,
                                                new StringBlock(gameSave, "ChunkTable Length: " + chunkTable.Length),
                                                chunkTable);
            var newHeader = new Header(gameSave, mainIndex);

            newHeader.Write(true);
        }

        public static void TestReading(Header header)
        {
            var readQueue = new Queue<BlockStructure>();

            readQueue.Enqueue(header);

            while (readQueue.Count > 0)
            {
                BlockStructure block = readQueue.Dequeue();
                block.Load();

                foreach (BlockStructure b in block.ReferencedBlocks)
                {
                    readQueue.Enqueue(b);
                }

                block.Unload();
            }
        }

        public static void TestModify(GameSave gameSave, Header header)
        {
            var newChunkTable =
                new ChunkTable(gameSave,
                               header.SaveIndex.ChunkTable.GetChunks().Concat(
                                   header.SaveIndex.ChunkTable.GetChunks().Take(1).Select(
                                       x => new Tuple<int, int, Chunk>(x.Item1 + 1, x.Item2 - 1, x.Item3))).ToArray());

            var newHeader = new Header(gameSave,
                                       new SavedStateIndex(gameSave, DateTime.UtcNow.Ticks, new StringBlock(gameSave, "Modified Save"),
                                                           newChunkTable));

            newHeader.Write(true);
        }

        public static void TestMarkModify(GameSave gameSave, Header header)
        {
            var newChunkTable =
                new ChunkTable(gameSave,
                               header.SaveIndex.ChunkTable.GetChunks().Concat(
                                   header.SaveIndex.ChunkTable.GetChunks().Take(1).Select(
                                       x => new Tuple<int, int, Chunk>(x.Item1 + 1, x.Item2 - 1, x.Item3))).ToArray());

            var newHeader = new Header(gameSave,
                                       new SavedStateIndex(gameSave, DateTime.UtcNow.Ticks, new StringBlock(gameSave, "Modified Save"),
                                                           newChunkTable));

            newHeader.Write(true);
        }
    }
}