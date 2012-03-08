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

namespace MarsMiner.Saving.Common
{
    public abstract class UniqueBlockStructure : BlockStructure
    {
        /// <summary>
        /// <para>Initializes a new UniqueBlockStructure instance.</para>
        /// <para>This constructor is for blocks that are read from disk.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        /// <param name="address">This block's address.</param>
        protected UniqueBlockStructure(GameSave gameSave,
                                       Tuple<int, uint> address)
            : base(gameSave, address)
        {
        }

        /// <summary>
        /// <para>Initializes a new UniqueBlockStructure instance.</para>
        /// <para>This constructor is for blocks that are newly created.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        protected UniqueBlockStructure(GameSave gameSave)
            : base(gameSave)
        {
        }

        internal abstract bool TryBindToExistingAddress();

        internal abstract void UniqueBlockStructureBound();
    }

    public abstract class UniqueBlockStructure<T> : UniqueBlockStructure where T : UniqueBlockStructure<T>
    {
        private static readonly Dictionary<GameSave, Dictionary<UniqueBlockStructure<T>, Tuple<int, uint>>>
            UniqueBlockStructureCache =
                new Dictionary<GameSave, Dictionary<UniqueBlockStructure<T>, Tuple<int, uint>>>();

        /// <summary>
        /// <para>Initializes a new UniqueBlockStructure instance.</para>
        /// <para>This constructor is for blocks that are read from disk.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        /// <param name="address">This block's address.</param>
        protected UniqueBlockStructure(GameSave gameSave,
                                       Tuple<int, uint> address) : base(gameSave, address)
        {
            Dictionary<UniqueBlockStructure<T>, Tuple<int, uint>> uniqueTCache;
            if (!UniqueBlockStructureCache.TryGetValue(gameSave, out uniqueTCache))
            {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
                uniqueTCache = new Dictionary<UniqueBlockStructure<T>, Tuple<int, uint>>(ValueComparer);
// ReSharper restore DoNotCallOverridableMethodsInConstructor
                UniqueBlockStructureCache.Add(gameSave, uniqueTCache);

                gameSave.MarkingFreeSpace += () => { lock (uniqueTCache) uniqueTCache.Clear(); };
                gameSave.Closing +=
                    () => { lock (UniqueBlockStructureCache) UniqueBlockStructureCache.Remove(gameSave); };
            }
            if (!uniqueTCache.ContainsKey(this))
            {
                uniqueTCache.Add(this, address);
            }
#if DebugVerboseUniqueBlocks
            Console.WriteLine("Unique {0} {1} from file at {2}.", GetType(), ValueComparer.GetHashCode(this), Address);
#endif
        }

        /// <summary>
        /// <para>Initializes a new UniqueBlockStructure instance.</para>
        /// <para>This constructor is for blocks that are newly created.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        protected UniqueBlockStructure(GameSave gameSave) : base(gameSave)
        {
        }

        protected abstract IEqualityComparer<UniqueBlockStructure<T>> ValueComparer { get; }

        internal override sealed bool TryBindToExistingAddress()
        {
            Dictionary<UniqueBlockStructure<T>, Tuple<int, uint>> uniqueTCache;
            if (UniqueBlockStructureCache.TryGetValue(GameSave, out uniqueTCache))
            {
                Tuple<int, uint> address;
                if (uniqueTCache.TryGetValue(this, out address))
                {
#if DebugVerboseUniqueBlocks
                    Console.WriteLine("Unique {0} {1} found at {2}.", GetType(), ValueComparer.GetHashCode(this), address);
#endif
                    Address = address;
                    Written = true;
                    return true;
                }
            }
            return false;
        }

        internal override sealed void UniqueBlockStructureBound()
        {
            Dictionary<UniqueBlockStructure<T>, Tuple<int, uint>> uniqueTCache;
            if (!UniqueBlockStructureCache.TryGetValue(GameSave, out uniqueTCache))
            {
                // ReSharper disable DoNotCallOverridableMethodsInConstructor
                uniqueTCache = new Dictionary<UniqueBlockStructure<T>, Tuple<int, uint>>(ValueComparer);
                // ReSharper restore DoNotCallOverridableMethodsInConstructor
                UniqueBlockStructureCache.Add(GameSave, uniqueTCache);

                GameSave.MarkingFreeSpace += () => { lock (uniqueTCache) uniqueTCache.Clear(); };
                GameSave.Closing +=
                    () => { lock (UniqueBlockStructureCache) UniqueBlockStructureCache.Remove(GameSave); };
            }
            uniqueTCache[this] = Address;
#if DebugVerboseUniqueBlocks
            Console.WriteLine("Unique {0} {1} bound at {2}.", GetType(), ValueComparer.GetHashCode(this), Address);
#endif
        }
    }
}