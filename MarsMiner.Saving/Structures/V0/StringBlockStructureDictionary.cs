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
using System.IO;
using System.Linq;
using System.Reflection;
using MarsMiner.Saving.Common;

namespace MarsMiner.Saving.Structures.V0
{
    public sealed class StringBlockStructureDictionary<TValue> : BlockStructure where TValue : BlockStructure
    {
        private KeyValuePair<string, TValue>[] _keyValuePairs;

        internal StringBlockStructureDictionary(GameSave gameSave, Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        public StringBlockStructureDictionary(GameSave gameSave, IEnumerable<KeyValuePair<string, TValue>> keyValuePairs)
            : base(gameSave)
        {
            _keyValuePairs = keyValuePairs.ToArray();
        }

        public Dictionary<string, TValue> Dictionary
        {
            get
            {
                Load();
                return _keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public override BlockStructure[] ReferencedBlocks
        {
            get
            {
                Load();
                return _keyValuePairs.Select(kv => kv.Value).ToArray<BlockStructure>();
            }
        }

        protected override void ReadData(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            _keyValuePairs = new KeyValuePair<string, TValue>[length];

            ConstructorInfo blockConstructorInfo =
                typeof(TValue).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                          null,
                                          new[] { typeof(GameSave), typeof(Tuple<int, uint>) }, null);

            if (blockConstructorInfo == null) throw new Exception("Constructor for bound " + typeof(TValue) + " not found.");

            for (int i = 0; i < length; i++)
            {
                var key = GameSave.ResolveString(reader.ReadUInt32());
                var value = (TValue)(blockConstructorInfo.Invoke(new object[] { GameSave, GameSave.ResolvePointer(Address.Item1, reader.ReadUInt32()) }));
                _keyValuePairs[i] = new KeyValuePair<string, TValue>(key, value);
            }
        }

        protected override void ForgetData()
        {
            _keyValuePairs = null;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(_keyValuePairs.Length);
            foreach (var keyValuePair in _keyValuePairs)
            {
                writer.Write(GameSave.FindStringAddress(keyValuePair.Key));
                writer.Write(GameSave.FindBlockPointer(this, keyValuePair.Value));
            }
        }

        protected override void UpdateLength()
        {
            Length = 4 // Length
                     + _keyValuePairs.Length *
                     (4 // string address
                      + 4); // block address
        }
    }
}