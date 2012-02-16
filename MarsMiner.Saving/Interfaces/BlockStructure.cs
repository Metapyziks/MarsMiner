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

namespace MarsMiner.Saving.Interfaces
{
    public abstract class BlockStructure
    {
        //TODO: Make private and add shortcut methods
        protected readonly GameSave GameSave;
        private Tuple<int, uint> _address;
        private int? _length;
        private Dictionary<int, IntRangeList> _recursiveUsedSpace;
        private Dictionary<int, IntRangeList> _usedSpace;

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

        protected BlockStructure(GameSave gameSave)
        {
            if (gameSave == null) throw new ArgumentNullException("gameSave");
            GameSave = gameSave;
            Address = null;
            Written = false;
            Loaded = true;
        }

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

        public bool Bound
        {
            get { return Address != null; }
        }

        public bool Loaded { get; private set; }

        public bool Written { get; private set; }

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

        public BlockStructure[] UnboundBlocks
        {
            get { throw new NotImplementedException(); }
        }

        public Dictionary<int, IntRangeList> RecursiveUsedSpace
        {
            get
            {
                if (_recursiveUsedSpace == null)
                {
                    _recursiveUsedSpace = new Dictionary<int, IntRangeList>();
                    foreach (BlockStructure block in ReferencedBlocks)
                    {
                        _recursiveUsedSpace.Add(block.RecursiveUsedSpace);
                    }
                    _recursiveUsedSpace.Add(UsedSpace);
                }

                return _recursiveUsedSpace;
            }
        }

        public abstract BlockStructure[] ReferencedBlocks { get; }

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

            Loaded = true;
        }

        protected abstract void ReadData(BinaryReader reader);

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

        protected abstract void ForgetData();

        public void Write()
        {
            if (!Bound)
            {
                GameSave.BindBlock(this);
            }

            if (Written)
            {
                return;
            }

            foreach (var block in ReferencedBlocks.Where(block => block.Written == false))
            {
                block.Write();
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

            WriteData(writer);

#if AssertBlockLength
            if (stream.Position - start != Length)
            {
                throw new Exception("Length mismatch after writing " + GetType() + "!");
            }
#endif

            Written = true;
        }

        protected abstract void WriteData(BinaryWriter writer);

        protected abstract void UpdateLength();
    }
}