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
using System.IO;

using OpenTK;

using ResourceLib;

using MarsMiner.Shared;
using MarsMiner.Shared.Octree;
using MarsMiner.Shared.Geometry;

namespace MarsMiner.Client.Graphics
{
    public class RGeometryModelManager : RManager
    {
        public RGeometryModelManager()
            : base( typeof( GeometryModel ), 3, "gmdl" )
        {

        }

        public override ResourceItem[] LoadFromFile( string keyPrefix, string fileName, string fileExtension, FileStream stream )
        {
            return new ResourceItem[] { new ResourceItem( keyPrefix + fileName, new GeometryModel( stream ) ) };
        }

        public override object LoadFromArchive( BinaryReader stream )
        {
            return new GeometryModel( stream.BaseStream );
        }

        public override void SaveToArchive( BinaryWriter stream, object item )
        {
            ( (GeometryModel) item ).Save( stream.BaseStream );
        }
    }

    public enum TerrainCorner : byte
    {
        None = 0,
        FrontLeft = 1,
        BackLeft = 2,
        FrontRight = 4,
        BackRight = 8,

        Left = FrontLeft | BackLeft,
        Front = FrontLeft | FrontRight,
        Right = FrontRight | BackRight,
        Back = BackLeft | BackRight,

        All = Left | Right
    }

    public class GeometryModel
    {
        public const UInt16 FileFormatVersion = 0x0000;

        #region Default Models
        #region Terrain
        public static GeometryModel Terrain( TerrainCorner highCorners, string texTop, string texWall, string texWallTri,
            string texBottom )
        {
            Vector2 pfl = new Vector2( 0.0f, 0.0f );
            Vector2 pfr = new Vector2( 1.0f, 0.0f );
            Vector2 pbl = new Vector2( 0.0f, 1.0f );
            Vector2 pbr = new Vector2( 1.0f, 1.0f );

            bool bfl = ( highCorners & TerrainCorner.FrontLeft  ) != 0;
            bool bfr = ( highCorners & TerrainCorner.FrontRight ) != 0;
            bool bbl = ( highCorners & TerrainCorner.BackLeft   ) != 0;
            bool bbr = ( highCorners & TerrainCorner.BackRight  ) != 0;

            float hfl = bfl ? 1.0f : 0.0f;
            float hfr = bfr ? 1.0f : 0.0f;
            float hbl = bbl ? 1.0f : 0.0f;
            float hbr = bbr ? 1.0f : 0.0f;

            Tuple<Face,Vector2,Vector2,float,float>[] faces = new Tuple<Face,Vector2,Vector2,float,float>[]
            {
                new Tuple<Face,Vector2,Vector2,float,float>
                ( Face.Left, pbl, pfl, hbl, hfl ),
                new Tuple<Face,Vector2,Vector2,float,float>
                ( Face.Front, pfl, pfr, hfl, hfr ),
                new Tuple<Face,Vector2,Vector2,float,float>
                ( Face.Right, pfr, pbr, hfr, hbr ),
                new Tuple<Face,Vector2,Vector2,float,float>
                ( Face.Back, pbr, pbl, hbr, hbl )
            };

            GeometryModel mdl = new GeometryModel();

            foreach( Tuple<Face,Vector2,Vector2,float,float> face in faces )
            {
                if( face.Item4 == 0.0f && face.Item5 == 0.0f )
                    continue;

                ModelFace mf;
                if( face.Item4 == 0.0f || face.Item5 == 0.0f )
                {
                    Vector2 corner = face.Item4 == 0.0f ? face.Item3 : face.Item2;

                    mf = new ModelFace( texWallTri, new float[]
                    {
                        face.Item2.X, face.Item4, face.Item2.Y, 0.0f, face.Item4,
                        face.Item3.X, face.Item5, face.Item3.Y, 1.0f, face.Item5,
                        corner.X, 0.0f, corner.Y, face.Item4 == 0.0f ? 1.0f : 0.0f, 0.0f
                    } );
                }
                else
                {
                    mf = new ModelFace( texWallTri, new float[]
                    {
                        face.Item2.X, face.Item4, face.Item2.Y, 0.0f, face.Item4,
                        face.Item3.X, face.Item5, face.Item3.Y, 1.0f, face.Item5,
                        face.Item3.X, 0.0f, face.Item3.Y, 1.0f, 0.0f,
                        face.Item2.X, 0.0f, face.Item2.Y, 0.0f, 0.0f
                    } );
                }
                mdl.AddFace( mf, face.Item1 );
            }

           mdl.AddFace( new ModelFace( texTop, new float[]
            {
                pbl.X, hbl, pbl.Y, 0.0f, 0.0f,
                pbr.X, hbr, pbr.Y, 1.0f, 0.0f,
                pfr.X, hfr, pfr.Y, 1.0f, 1.0f,
                pfl.X, hfl, pfl.Y, 0.0f, 1.0f
            } ), bbl && bbr && bfl && bfr ? Face.Top : Face.All );

            return mdl;
        }
        #endregion
        #region Cube
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
                0.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            } ), Face.Left );
            cube.AddFace( new ModelFace( texBottom, new float[]
            {
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            } ), Face.Bottom );
            cube.AddFace( new ModelFace( texFront, new float[]
            {
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            } ), Face.Front );
            cube.AddFace( new ModelFace( texRight, new float[]
            {
                1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            } ), Face.Right );
            cube.AddFace( new ModelFace( texTop, new float[]
            {
                0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
            } ), Face.Top );
            cube.AddFace( new ModelFace( texBack, new float[]
            {
                1.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            } ), Face.Back );

            return cube;
        }
        #endregion
        #endregion

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

        public GeometryModel( String filePath )
        {
            myFaces = new Dictionary<Face, List<ModelFace>>();

            FileStream stream = File.Open( filePath, FileMode.Open );
            Load( stream );
            stream.Close();
        }

        public GeometryModel( Stream stream )
        {
            myFaces = new Dictionary<Face, List<ModelFace>>();

            Load( stream );
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

        public void Save( Stream stream )
        {
            BinaryWriter writer = new BinaryWriter( stream );

            writer.Write( FileFormatVersion );
            writer.Write( (byte) myFaces.Count );

            foreach( KeyValuePair<Face, List<ModelFace>> keyVal in myFaces )
            {
                writer.Write( keyVal.Key.Bitmap );
                writer.Write( (UInt16) keyVal.Value.Count );

                foreach ( ModelFace face in keyVal.Value )
                {
                    writer.Write( face.TextureName );
                    writer.Write( (byte) face.Vertices.Length );

                    for ( int i = 0; i < face.Vertices.Length; ++i )
                    {
                        writer.Write( face.Vertices[ i ].X );
                        writer.Write( face.Vertices[ i ].Y );
                        writer.Write( face.Vertices[ i ].Z );
                        writer.Write( face.TextureCoords[ i ].X );
                        writer.Write( face.TextureCoords[ i ].Y );
                    }
                }
            }
        }

        public void Load( Stream stream )
        {
            myFaces.Clear();

            BinaryReader reader = new BinaryReader( stream );

            UInt16 vers = reader.ReadUInt16();
            UInt16 faceGroups = reader.ReadByte();

            for ( int i = 0; i < faceGroups; ++i )
            {
                Face face = new Face( reader.ReadByte() );
                UInt16 faces = reader.ReadUInt16();

                for ( int j = 0; j < faces; ++j )
                {
                    String texName = reader.ReadString();
                    byte vertsCount = reader.ReadByte();

                    Vector3[] verts = new Vector3[ vertsCount ];
                    Vector2[] txCos = new Vector2[ vertsCount ];

                    for ( int k = 0; k < vertsCount; ++k )
                    {
                        verts[ k ] = new Vector3
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle()
                        };
                        txCos[ k ] = new Vector2
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle()
                        };
                    }

                    AddFace( new ModelFace( texName, verts, txCos ), face );
                }
            }
        }
    }
}
