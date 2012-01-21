/**
 * Copyright (c) 2012 James King [metapyziks@gmail.com]
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

using MarsMiner.Shared.Octree;

namespace MarsMiner.Shared.Geometry
{
    public class BlockComponant
    {

    }

    public class SolidityBComponant : BlockComponant
    {
        public readonly bool IsSolid;

        public SolidityBComponant( bool isSolid )
        {
            IsSolid = isSolid;
        }
    }

    public class BlockType
    {
        private Dictionary<Type, BlockComponant> myComponants;

        public readonly String Name;
        public readonly int SubType;

        internal BlockType( String name, int subType = 0 )
        {
            Name = name;
            SubType = subType;

            myComponants = new Dictionary<Type, BlockComponant>();
        }

        public void SetComponant<T>( T componant )
            where T : BlockComponant
        {
            Type type = typeof( T );

            if ( !myComponants.ContainsKey( type ) )
                myComponants.Add( type, componant );
            else
                myComponants[ type ] = componant;
        }

        public bool HasComponant<T>()
            where T : BlockComponant
        {
            return myComponants.ContainsKey( typeof( T ) );
        }

        public T GetComponant<T>()
            where T : BlockComponant
        {
            Type type = typeof( T );

            if ( myComponants.ContainsKey( type ) )
                return (T) myComponants[ type ];

            return null;
        }
    }
}
