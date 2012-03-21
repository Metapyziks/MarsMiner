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
using System.IO;
using System.Linq;
using MarsMiner.Saving.Common;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class Octree : BlockStructure
    {
        private BitArray _octreeFlags;
        private byte[] _octreeValues;

        private Octree(GameSave gameSave, Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        internal static Octree FromSave(GameSave gameSave, Tuple<int, uint> address)
        {
            Octree octree;
            if (!gameSave.TryGetFromBlockStructureCache(address, out octree))
            {
                octree = new Octree(gameSave, address);
                gameSave.AddToBlockStructureCache(address, octree);
            }
            return octree;
        }

        public Octree(GameSave gameSave, BitArray octreeFlags, byte[] octreeValues)
            : base(gameSave)
        {
            _octreeFlags = octreeFlags;
            _octreeValues = octreeValues;

            UpdateLength();
        }

        public BitArray OctreeFlags
        {
            get
            {
                Load();
                return new BitArray(_octreeFlags);
            }
        }

        public IEnumerable<byte> OctreeValues
        {
            get
            {
                Load();
                return _octreeValues.AsEnumerable();
            }
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get { return new BlockStructure[0]; }
        }

        protected override void ReadData(BinaryReader reader)
        {
            int octreeFlagsLength = reader.ReadInt32();
            int octreeValuesLength = reader.ReadInt32();

            _octreeFlags = new BitArray(reader.ReadBytes(octreeFlagsLength));
            _octreeValues = reader.ReadBytes(octreeValuesLength);
        }

        protected override void ForgetData()
        {
            _octreeFlags = null;
            _octreeValues = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            int octreeFlagsLength = (_octreeFlags.Length / 8) + (_octreeFlags.Length % 8 == 0 ? 0 : 1);

            writer.Write(octreeFlagsLength);

            writer.Write(_octreeValues.Length);

            var buffer = new byte[octreeFlagsLength];
            _octreeFlags.CopyTo(buffer, 0);
            writer.Write(buffer);

            writer.Write(_octreeValues);
        }

        protected override void UpdateLength()
        {
            Length = 4 // octreeFlags length
                     + 4 // octreeValueList length
                     + (_octreeFlags.Length / 8) + (_octreeFlags.Length % 8 == 0 ? 0 : 1) // octreeFlags
                     + _octreeValues.Length; // octreeValues
        }
    }
}