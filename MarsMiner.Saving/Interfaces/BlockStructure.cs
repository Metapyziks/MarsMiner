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
        private readonly Func<int, Stream> _getStreamFunc;
        private Tuple<int, uint> _address;
        private Func<int, uint, Tuple<int, uint>> _resolvePointerFunc;
        private Func<uint, string> _resolveStringFunc;

        public BlockStructure(Tuple<int, uint> address,
                              Func<int, Stream> getStreamFunc,
                              Func<int, uint, Tuple<int, uint>> resolvePointerFunc,
                              Func<uint, string> resolveStringFunc)
        {
            Address = address;
            _getStreamFunc = getStreamFunc;
            _resolvePointerFunc = resolvePointerFunc;
            _resolveStringFunc = resolveStringFunc;
        }

        public Tuple<int, uint> Address
        {
            get { return _address; }
            set
            {
                if (_address != null)
                {
                    throw new InvalidOperationException("Address can't be reassigned!");
                }
                _address = value;
            }
        }

        public bool Bound
        {
            get { return Address != null; }
        }

        public bool Loaded { get; private set; }

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

            Stream stream = _getStreamFunc(_address.Item1);
            stream.Seek(_address.Item2, SeekOrigin.Begin);
            var reader = new BinaryReader(stream);

            ReadData(reader);

            Loaded = true;
        }

        protected abstract void ReadData(BinaryReader reader);

        public void Unload()
        {
            if (!Loaded) { return; }

            ForgetData();

            Loaded = false;
        }

        protected abstract void ForgetData() { }
    }
}