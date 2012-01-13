using System;

using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    sealed class VertexBuffer
    {
        private ShaderProgram myShader;

        private bool mySetUp = false;
        private bool myDataSet = false;

        private int myVaoID;
        private int myVboID;
        private int myLength;

        private int VaoID
        {
            get
            {
                if ( myVaoID == 0 )
                    GL.GenVertexArrays( 1, out myVaoID );
                
                return myVaoID;
            }
        }

        private int VboID
        {
            get
            {
                if ( myVboID == 0 )
                    GL.GenBuffers( 1, out myVboID );

                return myVboID;
            }
        }

        public VertexBuffer( ShaderProgram shader )
        {
            myShader = shader;
        }

        public void SetData( float[] vertices )
        {
            if ( !mySetUp )
                SetUp();

            myLength = vertices.Length / myShader.VertexDataSize;

            GL.BindVertexArray( VaoID );

            GL.BindBuffer( BufferTarget.ArrayBuffer, VboID );
            GL.BufferData( BufferTarget.ArrayBuffer, new IntPtr( vertices.Length * sizeof( float ) ), vertices, BufferUsageHint.StaticDraw );

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindVertexArray( 0 );

            myDataSet = true;
        }

        public void SetUp()
        {
            int stride = myShader.VertexDataStride;

            GL.BindVertexArray( VaoID );

            GL.BindBuffer( BufferTarget.ArrayBuffer, VboID );

            foreach ( AttributeInfo info in myShader.Attributes )
            {
                GL.VertexAttribPointer( info.Location, info.Size, info.PointerType,
                    info.Normalize, myShader.VertexDataStride, info.Offset );
                GL.EnableVertexAttribArray( info.Location );
            }

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindVertexArray( 0 );

            mySetUp = true;
        }

        private void CheckForError()
        {
            ErrorCode error = GL.GetError();

            if ( error != ErrorCode.NoError )
                throw new Exception( "OpenGL hates your guts: " + error.ToString() );
        }

        public void Render()
        {
            if ( myDataSet )
            {
                if ( !myShader.Active )
                    myShader.Use();

                GL.BindVertexArray( VaoID );
                GL.DrawArrays( BeginMode.Quads, 0, myLength );
                GL.BindVertexArray( 0 );

                CheckForError();
            }
        }

        public void Dispose()
        {
            if ( myDataSet )
            {
                GL.DeleteVertexArrays( 1, ref myVaoID );
                GL.DeleteBuffers( 1, ref myVboID );
            }

            myDataSet = false;
        }
    }
}