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

namespace MarsMiner.Saving.Common
{
    public abstract class UniqueBlockStructure : BlockStructure
    {
        /// <summary>
        /// <para>Initializes a new BlockStructure instance.</para>
        /// <para>This constructor is for blocks that are read from disk.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        /// <param name="address">This block's address.</param>
        protected UniqueBlockStructure(GameSave gameSave,
                                       Tuple<int, uint> address) : base(gameSave, address)
        {
        }

        /// <summary>
        /// <para>Initializes a new BlockStructure instance.</para>
        /// <para>This constructor is for blocks that are newly created.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        protected UniqueBlockStructure(GameSave gameSave) : base(gameSave)
        {
        }

        protected abstract bool HasEqualData(BlockStructure other);

        public abstract override int GetHashCode();
    }
}