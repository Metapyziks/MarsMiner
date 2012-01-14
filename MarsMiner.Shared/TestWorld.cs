using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MarsMiner.Shared
{
    public class TestChunkLoadEventArgs : EventArgs
    {
        public readonly TestChunk Chunk;

        public TestChunkLoadEventArgs( TestChunk chunk )
        {
            Chunk = chunk;
        }
    }

    public class TestWorld
    {
        private Thread myGeneratorThread;
        private Dictionary<UInt16,TestChunk> myLoadedChunks;
        private Queue<TestChunk> myChunksToLoad;

        public OctreeTestWorldGenerator Generator { get; private set; }
        public bool GeneratorRunning { get; private set; }

        public event EventHandler<TestChunkLoadEventArgs> ChunkLoaded;
        public event EventHandler<TestChunkLoadEventArgs> ChunkUnloaded;
        public event EventHandler<TestChunkLoadEventArgs> ChunkChanged;

        public TestWorld( int seed = 0 )
        {
            myLoadedChunks = new Dictionary<UInt16, TestChunk>();
            myChunksToLoad = new Queue<TestChunk>();
            Generator = new OctreeTestWorldGenerator( seed );

            int limit = 1024 / TestChunk.ChunkSize;

            for ( int x = -limit; x < limit; ++x )
                for ( int z = -limit; z < limit; ++z )
                    LoadChunk( x * TestChunk.ChunkSize, z * TestChunk.ChunkSize );/*/

            LoadChunk( 0, 0 );
            LoadChunk( TestChunk.ChunkSize, 0 );
            LoadChunk( -TestChunk.ChunkSize, 0 );
            LoadChunk( 0, TestChunk.ChunkSize );
            LoadChunk( 0, -TestChunk.ChunkSize );*/

            myChunksToLoad = new Queue<TestChunk>( myChunksToLoad.OrderBy( x => Math.Max( Math.Abs( x.CenterX ), Math.Abs( x.CenterZ ) ) ) );

            GeneratorRunning = false;
        }

        public void LoadChunk( int x, int z )
        {
            x = Tools.FloorDiv( x, TestChunk.ChunkSize ) * TestChunk.ChunkSize;
            z = Tools.FloorDiv( z, TestChunk.ChunkSize ) * TestChunk.ChunkSize;

            myChunksToLoad.Enqueue( new TestChunk( this, x, z ) );
        }

        private UInt16 FindChunkID( int x, int z )
        {
            byte cx = (byte) Tools.FloorDiv( x, TestChunk.ChunkSize );
            byte cz = (byte) Tools.FloorDiv( z, TestChunk.ChunkSize );

            return (UInt16) ( cx << 8 | cz );
        }

        public TestChunk FindChunk( int x, int z )
        {
            UInt16 id = FindChunkID( x, z );

            if ( myLoadedChunks.ContainsKey( id ) )
                return myLoadedChunks[ id ];

            return null;
        }

        public OctreeTest FindOctree( int x, int y, int z, int size )
        {
            if ( size > TestChunk.ChunkSize )
                return null;

            TestChunk chunk = FindChunk( x, z );

            if ( chunk != null )
                return chunk.FindOctree( x, y, z, size );

            return null;
        }

        public void StartGenerator()
        {
            if ( GeneratorRunning )
                return;

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

            while ( GeneratorRunning && myChunksToLoad.Count != 0 )
            {
                Monitor.Enter( myChunksToLoad );
                TestChunk chunk = myChunksToLoad.Dequeue();
                Monitor.Exit( myChunksToLoad );
                Monitor.Enter( myLoadedChunks );
                chunk.Generate();
                myLoadedChunks.Add( FindChunkID( chunk.X, chunk.Z ), chunk );
                chunk.UpdateNeighbours();
                Monitor.Exit( myLoadedChunks );

                if ( ChunkLoaded != null )
                    ChunkLoaded( this, new TestChunkLoadEventArgs( chunk ) );

                if ( ChunkChanged != null )
                {
                    TestChunk n;
                    n = FindChunk( chunk.X - TestChunk.ChunkSize, chunk.Z );
                    if ( n != null ) ChunkChanged( this, new TestChunkLoadEventArgs( n ) );
                    n = FindChunk( chunk.X + TestChunk.ChunkSize, chunk.Z );
                    if ( n != null ) ChunkChanged( this, new TestChunkLoadEventArgs( n ) );
                    n = FindChunk( chunk.X, chunk.Z - TestChunk.ChunkSize );
                    if ( n != null ) ChunkChanged( this, new TestChunkLoadEventArgs( n ) );
                    n = FindChunk( chunk.X, chunk.Z + TestChunk.ChunkSize );
                    if ( n != null ) ChunkChanged( this, new TestChunkLoadEventArgs( n ) );
                }
            }

            GeneratorRunning = false;
        }
    }
}
