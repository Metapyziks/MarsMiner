/**
 * Copyright (c) 2012 James King
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 * 
 * James King [metapyziks@gmail.com]
 */

using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    sealed class VertexBuffer
    {
        private int myStride;

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

        public VertexBuffer( int stride )
        {
            myStride = stride;
        }

        public void SetData<T>( T[] vertices ) where T : struct
        {
            myLength = vertices.Length / myStride;

            GL.BindVertexArray( VaoID );

            GL.BindBuffer( BufferTarget.ArrayBuffer, VboID );
            GL.BufferData( BufferTarget.ArrayBuffer, new IntPtr( vertices.Length * Marshal.SizeOf( typeof( T ) ) ), vertices, BufferUsageHint.StaticDraw );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindVertexArray( 0 );

            CheckForError();

            myDataSet = true;
        }

        private void CheckForError()
        {
            ErrorCode error = GL.GetError();

            if ( error != ErrorCode.NoError )
                throw new Exception( "OpenGL hates your guts: " + error.ToString() );
        }

        public void Render( ShaderProgram shader )
        {
            if ( myDataSet )
            {
                GL.BindVertexArray( VaoID );
                GL.BindBuffer( BufferTarget.ArrayBuffer, VboID );

                foreach ( AttributeInfo info in shader.Attributes )
                {
                    GL.VertexAttribPointer( info.Location, info.Size, info.PointerType,
                        info.Normalize, shader.VertexDataStride, info.Offset );
                    GL.EnableVertexAttribArray( info.Location );
                }

                GL.DrawArrays( BeginMode.Quads, 0, myLength );

                foreach ( AttributeInfo info in shader.Attributes )
                    GL.DisableVertexAttribArray( info.Location );

                GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
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