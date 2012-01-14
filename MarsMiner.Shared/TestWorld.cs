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
        private List<TestChunk> myLoadedChunks;
        private Queue<TestChunk> myChunksToLoad;

        public OctreeTestWorldGenerator Generator { get; private set; }
        public bool GeneratorRunning { get; private set; }

        public event EventHandler<TestChunkLoadEventArgs> ChunkLoaded;
        public event EventHandler<TestChunkLoadEventArgs> ChunkUnloaded;

        public TestWorld( int seed = 0 )
        {
            myLoadedChunks = new List<TestChunk>();
            myChunksToLoad = new Queue<TestChunk>();
            Generator = new OctreeTestWorldGenerator( seed );

            int limit = 256 / TestChunk.ChunkSize;

            for ( int x = -limit; x < limit; ++x )
                for ( int z = -limit; z < limit; ++z )
                    LoadChunk( x, z );

            myChunksToLoad = new Queue<TestChunk>( myChunksToLoad.OrderBy( x => x.CenterX * x.CenterX + x.CenterZ * x.CenterZ ) );

            GeneratorRunning = false;
        }

        public void LoadChunk( int x, int z )
        {
            myChunksToLoad.Enqueue( new TestChunk( this, x * TestChunk.ChunkSize, z * TestChunk.ChunkSize ) );
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
                chunk.Generate();
                Monitor.Enter( myLoadedChunks );
                myLoadedChunks.Add( chunk );
                Monitor.Exit( myLoadedChunks );

                if ( ChunkLoaded != null )
                    ChunkLoaded( this, new TestChunkLoadEventArgs( chunk ) );
            }

            GeneratorRunning = false;
        }
    }
}
