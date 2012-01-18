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
using System.Threading;

using MarsMiner.Shared.Octree;

namespace MarsMiner.Shared.Geometry
{
    public enum ChunkEventType
    {
        Loaded = 1,
        Changed = 2,
        Unloaded = 4
    }

    public class ChunkEventArgs : EventArgs
    {
        public readonly Chunk Chunk;
        public readonly ChunkEventType EventType;

        public ChunkEventArgs( Chunk chunk, ChunkEventType type )
        {
            Chunk = chunk;
            EventType = type;
        }
    }

    public class World : IOctreeContainer<UInt16>, IDisposable
    {
        private Thread myGeneratorThread;
        private Dictionary<UInt16,Chunk> myLoadedChunks;
        private Queue<Chunk> myChunksToLoad;
        private Queue<Chunk> myChunksToUnload;

        public WorldGenerator Generator { get; private set; }
        public bool GeneratorRunning { get; private set; }

        public event EventHandler Initialize;

        public event EventHandler<ChunkEventArgs> ChunkLoaded;
        public event EventHandler<ChunkEventArgs> ChunkUnloaded;
        public event EventHandler<ChunkEventArgs> ChunkChanged;

        public World( int seed = 0 )
        {
            BlockManager.ClearTypes();
            OnInitialize();

            if ( Initialize != null )
                Initialize( this, new EventArgs() );

            myLoadedChunks = new Dictionary<UInt16, Chunk>();
            myChunksToLoad = new Queue<Chunk>();
            myChunksToUnload = new Queue<Chunk>();
            Generator = new PerlinGenerator( seed );

            GeneratorRunning = false;
        }

        public void Generate( int width, int height )
        {
            int xLimit = width  / Chunk.Size / 2;
            int zLimit = height / Chunk.Size / 2;

            for ( int x = -xLimit; x < xLimit; ++x )
                for ( int z = -zLimit; z < zLimit; ++z )
                    LoadChunk( x * Chunk.Size, z * Chunk.Size );
        }

        protected virtual void OnInitialize()
        {
            BlockType empty = new BlockType( "Core_Empty" )
            {
                IsSolid     = false,
                IsVisible   = false,
                SolidFaces  = Face.None
            };
            BlockManager.RegisterType( empty );
            BlockType sand = new BlockType( "MarsMiner_Sand" )
            {
                IsSolid     = true,
                IsVisible   = true,
                AutoSmooth  = true,
                SolidFaces  = Face.All,

                TileGraphics = new String[]
                {
                    "images_blocks_sand"
                }
            };
            BlockManager.RegisterType( sand );
            BlockType rock = new BlockType( "MarsMiner_Rock" )
            {
                IsSolid     = true,
                IsVisible   = true,
                AutoSmooth  = false,
                SolidFaces  = Face.All,

                TileGraphics = new String[]
                {
                    "images_blocks_rock"
                }
            };
            BlockManager.RegisterType( rock );
            BlockType boulder = new BlockType( "MarsMiner_Boulder" )
            {
                IsSolid     = true,
                IsVisible   = true,
                AutoSmooth  = false,
                SolidFaces  = Face.All,

                TileGraphics = new String[]
                {
                    "images_blocks_boulder"
                }
            };
            BlockManager.RegisterType( boulder );
        }

        public void LoadChunk( int x, int z )
        {
            x = Tools.FloorDiv( x, Chunk.Size ) * Chunk.Size;
            z = Tools.FloorDiv( z, Chunk.Size ) * Chunk.Size;

            myChunksToLoad.Enqueue( new Chunk( this, x, z ) );

            if ( !GeneratorRunning )
                StartGenerator();
        }

        public void UnloadChunk( int x, int z )
        {
            Chunk chunk = FindChunk( x, z );

            if ( chunk != null )
                myChunksToUnload.Enqueue( chunk );

            if ( !GeneratorRunning )
                StartGenerator();
        }

        private UInt16 FindChunkID( int x, int z )
        {
            byte cx = (byte) Tools.FloorDiv( x, Chunk.Size );
            byte cz = (byte) Tools.FloorDiv( z, Chunk.Size );

            return (UInt16) ( cx << 8 | cz );
        }

        public Chunk FindChunk( int x, int z )
        {
            UInt16 id = FindChunkID( x, z );

            if ( myLoadedChunks.ContainsKey( id ) )
                return myLoadedChunks[ id ];

            return null;
        }

        public OctreeNode<UInt16> FindNode( int x, int y, int z, int size )
        {
            if ( size > Chunk.Size )
                return null;

            Chunk chunk = FindChunk( x, z );

            if ( chunk != null )
                return chunk.FindNode( x, y, z, size );

            return null;
        }

        private void StartGenerator()
        {
            if ( GeneratorRunning )
                return;

            GeneratorRunning = true;

            myGeneratorThread = new Thread( GeneratorLoop );
            myGeneratorThread.Start();
        }

        public void StopGenerator()
        {
            GeneratorRunning = false;
        }

        private void GeneratorLoop()
        {
            GeneratorRunning = true;

            while ( GeneratorRunning && ( myChunksToLoad.Count != 0 || myChunksToUnload.Count != 0 ) )
            {
                if ( myChunksToLoad.Count != 0 )
                {
                    Chunk chunk = myChunksToLoad.Dequeue();
                    Monitor.Enter( myLoadedChunks );
                    chunk.Generate();
                    myLoadedChunks.Add( FindChunkID( chunk.X, chunk.Z ), chunk );
                    Monitor.Exit( myLoadedChunks );

                    if ( ChunkLoaded != null )
                        ChunkLoaded( this, new ChunkEventArgs( chunk, ChunkEventType.Loaded ) );

                    if ( ChunkChanged != null )
                    {
                        Chunk n;
                        n = FindChunk( chunk.X - Chunk.Size, chunk.Z );
                        if ( n != null ) ChunkChanged( this, new ChunkEventArgs( n, ChunkEventType.Changed ) );
                        n = FindChunk( chunk.X + Chunk.Size, chunk.Z );
                        if ( n != null ) ChunkChanged( this, new ChunkEventArgs( n, ChunkEventType.Changed ) );
                        n = FindChunk( chunk.X, chunk.Z - Chunk.Size );
                        if ( n != null ) ChunkChanged( this, new ChunkEventArgs( n, ChunkEventType.Changed ) );
                        n = FindChunk( chunk.X, chunk.Z + Chunk.Size );
                        if ( n != null ) ChunkChanged( this, new ChunkEventArgs( n, ChunkEventType.Changed ) );
                    }
                }
                if ( myChunksToUnload.Count != 0 )
                {
                    Chunk chunk = myChunksToLoad.Dequeue();
                    Monitor.Enter( myLoadedChunks );
                    myLoadedChunks.Remove( FindChunkID( chunk.X, chunk.Z ) );
                    Monitor.Exit( myLoadedChunks );

                    if ( ChunkUnloaded != null )
                        ChunkUnloaded( this, new ChunkEventArgs( chunk, ChunkEventType.Unloaded ) );
                }
            }

            GeneratorRunning = false;
        }

        public void Dispose()
        {
            if ( GeneratorRunning )
                StopGenerator();
        }
    }
}
