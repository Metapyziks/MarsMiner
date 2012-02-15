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
using MarsMiner.Saving.Util;

namespace MarsMiner.Saving.Interfaces
{
    public abstract class BlockStructure
    {
        protected readonly GameSave _gameSave;
        private Tuple<int, uint> _address;

        public BlockStructure(GameSave gameSave,
                              Tuple<int, uint> address)
        {
            if (gameSave == null) throw new ArgumentNullException("gameSave");
            if (address == null) throw new ArgumentNullException("address");
            _gameSave = gameSave;
            Address = address;
            Written = true;
            Loaded = false;
        }

        protected BlockStructure(GameSave gameSave)
        {
            if (gameSave == null) throw new ArgumentNullException("gameSave");
            _gameSave = gameSave;
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

        private bool _loaded;
        public bool Loaded
        {
            get { return _loaded; }
            private set { _loaded = value; }
        }

        public bool Written { get; private set; }

        private int? _length;
        public int Length
        {
            get {
                if (_length == null)
                {
                    throw new InvalidOperationException("Length isn't set.");
                }
                return _length.Value; }
            protected set { _length = value; }
        }

        public IBlockStructure[] UnboundBlocks
        {
            get { throw new NotImplementedException(); }
        }

        public Dictionary<int, IntRangeList> RecursiveUsedSpace
        {
            get { throw new NotImplementedException(); }
        }

        public void Write(Stream stream, Func<IBlockStructure, IBlockStructure, uint> getBlockPointerFunc,
                          Func<string, uint> getStringPointerFunc)
        {
            throw new NotImplementedException();
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
            Console.WriteLine("Reading {0} from {1}", this.GetType(), source);
#endif

            Stream stream = _gameSave.GetBlobFile(_address.Item1);
            stream.Seek(_address.Item2, SeekOrigin.Begin);
            var reader = new BinaryReader(stream);

            ReadData(reader);

            UpdateLength();

#if DebugVerboseBlocks
            Console.WriteLine("Read {0} from {1} to {2} == {3}", this.GetType(), Address, Address.Item2 + Length, stream.Position);
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
                throw new InvalidOperationException("Tried to write unbound block.");
            }

            if (Written)
            {
                return;
            }

            throw new NotImplementedException("Write dependencies...");

            Stream stream = _gameSave.GetBlobFile(_address.Item1);
            stream.Seek(_address.Item2, SeekOrigin.Begin);

#if AssertBlockLength
            long start = stream.Position;
#endif

            var writer = new BinaryWriter(stream);

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