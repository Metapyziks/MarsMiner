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
using System.IO;
using System.Linq;
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Common
{
    public abstract class BlockStructure
    {
        /// <summary>
        /// The GameSave instance this block is attached to.
        /// </summary>
        protected readonly GameSave GameSave;

        private Tuple<int, uint> _address;
        private int? _length;
        private Dictionary<int, IntRangeList> _recursiveUsedSpace;
        private Dictionary<int, IntRangeList> _usedSpace;

        /// <summary>
        /// <para>Initializes a new BlockStructure instance.</para>
        /// <para>This constructor is for blocks that are read from disk.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        /// <param name="address">This block's address.</param>
        protected BlockStructure(GameSave gameSave,
                                 Tuple<int, uint> address)
        {
            if (gameSave == null) throw new ArgumentNullException("gameSave");
            if (address == null) throw new ArgumentNullException("address");
            GameSave = gameSave;
            Address = address;
            Written = true;
            Loaded = false;
        }

        /// <summary>
        /// <para>Initializes a new BlockStructure instance.</para>
        /// <para>This constructor is for blocks that are newly created.</para>
        /// </summary>
        /// <param name="gameSave">The GameSave instance this block is attached to.</param>
        protected BlockStructure(GameSave gameSave)
        {
            if (gameSave == null) throw new ArgumentNullException("gameSave");
            GameSave = gameSave;
            Address = null;
            Written = false;
            Loaded = true;
        }

        /// <summary>
        /// The block's address.
        /// </summary>
        public Tuple<int, uint> Address
        {
            get { return _address; }
            set
            {
                if (_address != null)
                {
                    throw new InvalidOperationException("Tried to set Address more than once.");
                }
                _address = value;
            }
        }

        /// <summary>
        /// <value>true</value>, if the block was bound to a location in a save file.
        /// </summary>
        public bool Bound
        {
            get { return Address != null; }
        }

        /// <summary>
        /// <value>true</value>, if the block's data was loaded into memory.
        /// </summary>
        public bool Loaded { get; private set; }

        /// <summary>
        /// <value>true</value>, if the block's data was written to disk.
        /// </summary>
        public bool Written { get; private set; }

        /// <summary>
        /// The block's length in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                if (_length == null)
                {
                    throw new InvalidOperationException("Length isn't set.");
                }

                return _length.Value;
            }
            protected set { _length = value; }
        }

        /// <summary>
        /// Gets the space in the blob files used by this block and all referenced blocks.
        /// </summary>
        public Dictionary<int, IntRangeList> RecursiveUsedSpace
        {
            get
            {
                UpdateRecursiveUsedSpace();

                return _recursiveUsedSpace;
            }
        }

        /// <summary>
        /// Gets the BlockStructures referenced by this block.
        /// </summary>
        public abstract BlockStructure[] ReferencedBlocks { get; }

        /// <summary>
        /// Gets the space used by this block.
        /// </summary>
        public Dictionary<int, IntRangeList> UsedSpace
        {
            get
            {
                if (_usedSpace == null)
                {
                    _usedSpace = new Dictionary<int, IntRangeList>();

                    _usedSpace[Address.Item1] = new IntRangeList();
                    _usedSpace[Address.Item1] += new Tuple<int, int>((int) Address.Item2, (int) Address.Item2 + Length);
                }

                return _usedSpace;
            }
        }

        private void UpdateRecursiveUsedSpace()
        {
            if (_recursiveUsedSpace != null) return;

            //Optimize?: This may be slow if there are a lot of fixed length blocks that don't reference other blocks.
            bool wasLoaded = Loaded;
            if (!wasLoaded)
            {
                Load();
            }

            _recursiveUsedSpace = new Dictionary<int, IntRangeList>();
            foreach (BlockStructure block in ReferencedBlocks)
            {
                _recursiveUsedSpace.Add(block.RecursiveUsedSpace);
            }
            _recursiveUsedSpace.Add(UsedSpace);

            if (!wasLoaded)
            {
                Unload();
            }
        }

        /// <summary>
        /// Loads data from disk if it isn't already loaded.
        /// </summary>
        public void Load()
        {
            if (Loaded) return;
            if (!Written)
            {
                // This shouldn't happen.
                throw new InvalidOperationException("Tried to load unwritten block.");
            }

#if DebugVerboseBlocks
            Console.WriteLine("Reading {0} from {1}", GetType(), source);
#endif

            Stream stream = GameSave.GetBlobFile(_address.Item1);
            stream.Seek(_address.Item2, SeekOrigin.Begin);
            var reader = new BinaryReader(stream);

            ReadData(reader);

            Loaded = true;

            UpdateLength();

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", GetType(), Address, Address.Item2 + Length, stream.Position);
#endif
#if AssertBlockLength
            if (Address.Item2 + Length != stream.Position)
            {
                throw new Exception("Length mismatch in " + GetType() + "!");
            }
#endif

            UpdateRecursiveUsedSpace();
        }

        /// <summary>
        /// Implementations must override this to read data.
        /// </summary>
        /// <param name="reader">A BinaryReader instance pointing to the start of the block.</param>
        protected abstract void ReadData(BinaryReader reader);

        /// <summary>
        /// <para>Removes references to block data.</para>
        /// <para>Unloading a block that hasn't been committed to disk is an invalid operation.</para>
        /// </summary>
        public void Unload()
        {
            if (!Loaded)
            {
                return;
            }
            if (!Written)
            {
                throw new InvalidOperationException("Tried to unload unwritten block.");
            }

            ForgetData();

            Loaded = false;
        }

        /// <summary>
        /// Implementations must override this to remove references to data.
        /// </summary>
        protected abstract void ForgetData();

        /// <summary>
        /// Writes this block and all referenced blocks to disk (if they weren't written already).
        /// </summary>
        /// <param name="unload">
        /// <para><value>true</value>: Unloads this block.</para>
        /// <para><value>false</value>: Doesn't unload this block.</para>
        /// <para>Referenced blocks aren't directly affected.</para>
        /// </param>
        public void Write(bool unload)
        {
            if (!Bound)
            {
                GameSave.BindBlock(this);
            }

            if (Written)
            {
                return;
            }

            if (!Loaded)
            {
                // This shouldn't happen
                throw new InvalidOperationException("Tried to write unwritten, unloaded block.");
            }

            foreach (BlockStructure block in ReferencedBlocks.Where(block => block.Written == false))
            {
                block.Write(false);
                // Don't unload referenced blocks. If unload is true, the reference is lost anyway, unless they are used somewhere else.
            }

            Stream stream = GameSave.GetBlobFile(_address.Item1);
            stream.Seek(_address.Item2, SeekOrigin.Begin);

#if AssertBlockLength
            long start = stream.Position;
#endif

            var writer = new BinaryWriter(stream);

#if DebugVerboseBlocks
            Console.WriteLine("Writing {0} from {1} to {2}", GetType(), Address, Address.Item2 + Length);
#endif

            bool isHeader = this is IHeader;
            if (isHeader)
            {
                GameSave.FlushFiles();
            }

            WriteData(writer);

#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch after writing " + GetType() + "!");
            }
#endif

            Written = true;

            UpdateRecursiveUsedSpace();

            if (unload)
            {
                Unload();
            }

            if (isHeader)
            {
                GameSave.MarkFreeSpace(this);
            }
        }

        /// <summary>
        /// Implementations must override this to read data.
        /// </summary>
        /// <param name="writer">A BinaryWriter instance pointing to the start of the block.</param>
        protected abstract void WriteData(BinaryWriter writer);

        /// <summary>
        /// Implementations must override this to update the Length property to the blocks length in bytes.
        /// </summary>
        protected abstract void UpdateLength();

        protected static void WriteAddress(BinaryWriter writer, Tuple<int, uint> tuple)
        {
            writer.Write(tuple.Item1);
            writer.Write(tuple.Item2);
        }

        protected static Tuple<int, uint> ReadAddress(BinaryReader reader)
        {
            return new Tuple<int, uint>(reader.ReadInt32(), reader.ReadUInt32());
        }
    }
}