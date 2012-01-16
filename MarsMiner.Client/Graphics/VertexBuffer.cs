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