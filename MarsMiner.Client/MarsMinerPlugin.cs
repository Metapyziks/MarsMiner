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
            BlockType sandCube = BlockManager.Get( "MarsMiner_Sand", 0 );
            sandCube.SetComponant( new VisibilityBComponant( true, Face.All ) );
            sandCube.SetComponant( new ModelBComponant( GeometryModel.Cube( "images_blocks_sand" ) ) );

            Face[] faces = new Face[] { Face.Front, Face.Left, Face.Back, Face.Right };

            for ( int i = 0; i < 4; ++i )
            {
                Face face = faces[ i ];
                BlockType sandSlope = BlockManager.Get( "MarsMiner_Sand", i + 1 );
                sandSlope.SetComponant( new VisibilityBComponant( true, Face.Bottom | face.Opposite ) );
                sandSlope.SetComponant( new ModelBComponant( GeometryModel.Slope( face, "images_blocks_sand", "images_blocks_sandtri" ) ) );
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
