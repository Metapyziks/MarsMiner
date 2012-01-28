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
            for ( int i = 1; i < 16; ++i )
            {
                TerrainCorner corners =
                    ( ( i >> 0 ) % 2 == 1 ? TerrainCorner.FrontLeft : TerrainCorner.None ) |
                    ( ( i >> 1 ) % 2 == 1 ? TerrainCorner.FrontRight : TerrainCorner.None ) |
                    ( ( i >> 2 ) % 2 == 1 ? TerrainCorner.BackLeft : TerrainCorner.None ) |
                    ( ( i >> 3 ) % 2 == 1 ? TerrainCorner.BackRight : TerrainCorner.None );

                Face solidFaces = Face.Bottom |
                    ( ( corners & TerrainCorner.All ) == TerrainCorner.All ? Face.Top : Face.None ) |
                    ( ( corners & TerrainCorner.Left ) == TerrainCorner.Left ? Face.Left : Face.None ) |
                    ( ( corners & TerrainCorner.Front ) == TerrainCorner.Front ? Face.Front : Face.None ) |
                    ( ( corners & TerrainCorner.Right ) == TerrainCorner.Right ? Face.Right : Face.None ) |
                    ( ( corners & TerrainCorner.Back ) == TerrainCorner.Back ? Face.Back : Face.None );

                BlockType sandQuart = BlockManager.Get( "MarsMiner_Sand", i - 1 );
                sandQuart.SetComponant( new VisibilityBComponant( true, solidFaces ) );
                sandQuart.SetComponant( new ModelBComponant( GeometryModel.Terrain( corners,
                    "images_blocks_sand", "images_blocks_sand",
                    "images_blocks_sandtri", "images_blocks_sand" ) ) );
            }

            BlockType rock = BlockManager.Get( "MarsMiner_Rock" );
            rock.SetComponant( new VisibilityBComponant( true, Face.All ) );
            rock.SetComponant( new ModelBComponant( GeometryModel.Cube( "images_blocks_rock" ) ) );

            BlockType boulder = BlockManager.Get( "MarsMiner_Boulder" );
            boulder.SetComponant( new VisibilityBComponant( true, Face.All ) );
            boulder.SetComponant( new ModelBComponant( GeometryModel.Cube( "images_blocks_boulder" ) ) );
        }
    }
}
