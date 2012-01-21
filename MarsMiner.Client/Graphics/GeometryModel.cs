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

using OpenTK;

using MarsMiner.Shared.Octree;
using MarsMiner.Shared.Geometry;

namespace MarsMiner.Client.Graphics
{
    public class GeometryModel
    {        
        public static GeometryModel Cube( string tex )
        {
            return Cube( tex, tex, tex, tex, tex, tex );
        }

        public static GeometryModel Cube( string texTop, string texSide )
        {
            return Cube( texSide, texSide, texSide, texSide, texTop, texSide );
        }

        public static GeometryModel Cube( string texTop, string texBottom, string texSide )
        {
            return Cube( texSide, texBottom, texSide, texSide, texTop, texSide );
        }

        public static GeometryModel Cube( string texLeft, string texBottom, string texFront,
            string texRight, string texTop, string texBack )
        {
            GeometryModel cube = new GeometryModel();

            cube.AddFace( new ModelFace( texLeft, new float[]
            {
                0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 0.0f, 1.0f,
            } ), Face.Left );
            cube.AddFace( new ModelFace( texBottom, new float[]
            {
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            } ), Face.Bottom );
            cube.AddFace( new ModelFace( texFront, new float[]
            {
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            } ), Face.Front );
            cube.AddFace( new ModelFace( texRight, new float[]
            {
                1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            } ), Face.Right );
            cube.AddFace( new ModelFace( texTop, new float[]
            {
                0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 0.0f, 1.0f,
            } ), Face.Top );
            cube.AddFace( new ModelFace( texBack, new float[]
            {
                1.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
            } ), Face.Back );

            return cube;
        }

        private static List<String> stUsedTextures = new List<string>();

        public static String[] UsedTextures
        {
            get { return stUsedTextures.ToArray(); }
        }

        private Dictionary<Face,List<ModelFace>> myFaces;

        public GeometryModel()
        {
            myFaces = new Dictionary<Face, List<ModelFace>>();
        }

        public void AddFace( ModelFace face )
        {
            AddFace( face, Face.All );
        }

        public void AddFace( ModelFace face, Face requiredVisibleFaces )
        {
            if ( !myFaces.ContainsKey( requiredVisibleFaces ) )
                myFaces.Add( requiredVisibleFaces, new List<ModelFace>() );

            myFaces[ requiredVisibleFaces ].Add( face );

            if ( !stUsedTextures.Contains( face.TextureName ) )
                stUsedTextures.Add( face.TextureName );
        }

        public void UpdateTextureIndexes( Texture2DArray texArray )
        {
            foreach ( List<ModelFace> faces in myFaces.Values )
                foreach ( ModelFace face in faces )
                    face.UpdateTextureIndex( texArray );
        }

        public float[] GenerateVertexData( Vector3 offset, float size, Face visibleFaces )
        {
            int count = 0;

            foreach ( KeyValuePair<Face,List<ModelFace>> faceGroup in myFaces )
                if ( !( visibleFaces & faceGroup.Key ).HasNone )
                    foreach ( ModelFace face in faceGroup.Value )
                        count += face.GeometryDataLength;

            float[] data = new float[ count ];
            GenerateVertexData( data, 0, offset, size, visibleFaces );
            return data;
        }

        public int GenerateVertexData( float[] output, int startIndex,
            Vector3 offset, float size, Face visibleFaces )
        {
            foreach ( KeyValuePair<Face,List<ModelFace>> faceGroup in myFaces )
            {
                if ( !( visibleFaces & faceGroup.Key ).HasNone )
                {
                    foreach ( ModelFace face in faceGroup.Value )
                    {
                        float[] data = face.GenerateGeometryVertexData( offset, size );
                        Array.Copy( data, 0, output, startIndex, data.Length );
                        startIndex += data.Length;
                    }
                }
            }

            return startIndex;
        }
    }
}
