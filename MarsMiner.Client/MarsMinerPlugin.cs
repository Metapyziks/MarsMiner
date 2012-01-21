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
using System.Linq;
using System.Text;

using MarsMiner.Shared;
using MarsMiner.Shared.Geometry;
using MarsMiner.Shared.Octree;
using MarsMiner.Client.Graphics;

namespace MarsMiner.Client
{
    public class MarsMinerPlugin : Plugin
    {
        protected MarsMinerPlugin( bool client, bool server )
            : base( true, false )
        {

        }

        public override void OnWorldIntitialize( World world )
        {
            BlockType sand = BlockManager.Get( "MarsMiner_Sand" );
            sand.SetComponant( new VisibilityBComponant( true, Face.All ) );
            sand.SetComponant( new ModelBComponant( new CubeGModel( "images_blocks_sand" ) ) );

            BlockType rock = BlockManager.Get( "MarsMiner_Rock" );
            sand.SetComponant( new VisibilityBComponant( true, Face.All ) );
            sand.SetComponant( new ModelBComponant( new CubeGModel( "images_blocks_rock" ) ) );

            BlockType boulder = BlockManager.Get( "MarsMiner_Boulder" );
            sand.SetComponant( new VisibilityBComponant( true, Face.All ) );
            sand.SetComponant( new ModelBComponant( new CubeGModel( "images_blocks_boulder" ) ) );
        }
    }
}
