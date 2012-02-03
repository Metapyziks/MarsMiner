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
using MarsMiner.Saving;
using MarsMiner.Shared.Geometry;

using SaveChunk = MarsMiner.Saving.Structures.V0.Chunk;
using SaveOctree = MarsMiner.Saving.Structures.V0.Octree;
using SaveBlockTypeTable = MarsMiner.Saving.Structures.V0.BlockTypeTable;
using MarsMiner.Shared.Octree;
using System.Collections;

namespace MarsMiner.Shared.Extensions
{
    internal static class GameSaveTranslator
    {
        public static void WriteChunk(this GameSave gameSave, Chunk chunk)
        {
            var saveChunk = TranslateChunk(chunk);

            throw new NotImplementedException("TODO: Enqueue saveChunk save");
        }

        private static SaveChunk TranslateChunk(Chunk chunk)
        {
            if (!chunk.Loaded)
            {
                throw new ArgumentException("Chunk isn't loaded.", "chunk");
            }

            var saveOctrees = new SaveOctree[chunk.Octrees.Length];

            for (int i = 0; i < chunk.Octrees.Length; i++)
            {
                saveOctrees[i] = TranslateOctree(chunk.Octrees[i]);
            }

            var saveBlockTypeTable = TranslateBlockTypeTable(BlockManager.GetAll()); //TODO: Find most used blocks in chunk?

            var saveChunk = new SaveChunk(saveBlockTypeTable, saveOctrees);

            return saveChunk;
        }

        private static SaveBlockTypeTable TranslateBlockTypeTable(BlockType[] blockTypes)
        {
            return new SaveBlockTypeTable(blockTypes.Select(x => new Tuple<string, int>(x.Name, x.SubType)));
        }

        private static SaveOctree TranslateOctree(Octree.Octree<ushort> octree)
        {
            var octreeFlags = new List<bool>();
            var octreeValues = new List<byte>();

            foreach (var node in octree)
            {
                {
                    var branch = node as OctreeBranch<ushort>;
                    if (branch != null)
                    {
                        octreeFlags.Add(true); //HasChildren
                        //TODO: Find LOD value.
                        { //Start of filler code.
                            octreeFlags.Add(false);
                            octreeValues.Add(0);
                        } //End of filler code.
                        continue;
                    }
                }
                {
                    var leaf = node as OctreeLeaf<ushort>;
                    if (leaf != null)
                    {
                        octreeFlags.Add(false); //HasChildren
                        if (leaf.Value <= byte.MaxValue)
                        {
                            octreeFlags.Add(false); //HasLargeValue
                            octreeValues.Add((byte)leaf.Value);
                        }
                        else
                        {
                            octreeFlags.Add(true); //HasLargeValue
                            octreeValues.AddRange(BitConverter.GetBytes(leaf.Value));
                        }
                        continue;
                    }
                }
                throw new ArgumentException("Node isn't leaf or branch.", "octree");
            }

            return new SaveOctree(new BitArray(octreeFlags.ToArray()), octreeValues.ToArray());
        }
    }
}
