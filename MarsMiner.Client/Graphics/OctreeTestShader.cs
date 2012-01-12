using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    public class OctreeTestShader : ShaderProgram
    {
        private bool myUpdateMatrix;
        private Vector3 myCameraPos;
        private Vector2 myCameraRot;
        private Matrix4 myViewMatrix;
        private int myViewMatrixLoc;

        public Vector3 CameraPos
        {
            get { return myCameraPos; }
            set
            {
                myCameraPos = value;
                myUpdateMatrix = true;
            }
        }
        public Vector2 CameraRotation
        {
            get { return myCameraRot; }
            set
            {
                myCameraRot = value;
                myUpdateMatrix = true;
            }
        }

        public OctreeTestShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec4, "in_colour" );
            vert.AddVarying( ShaderVarType.Vec4, "var_colour" );
            vert.Logic = @"
                var_colour = in_colour;
                gl_Position = view_matrix * vec4( in_position, 1.0 );
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.Logic = "out_frag_colour = var_colour;";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;

            CameraPos = new Vector3();
            CameraRotation = new Vector2();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_position", 3 );
            AddAttribute( "in_colour", 4 );

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
        }

        private void UpdateViewMatrix()
        {
            myViewMatrix = Matrix4.Identity * Matrix4.CreateTranslation( -CameraPos ) *
                Matrix4.CreateRotationY( CameraRotation.Y ) * Matrix4.CreateRotationX( CameraRotation.X );

            GL.UniformMatrix4( myViewMatrixLoc, false, ref myViewMatrix );

            myUpdateMatrix = false;
        }

        protected override void OnBegin()
        {
            if ( myUpdateMatrix )
                UpdateViewMatrix();

            GL.Enable( EnableCap.DepthTest | EnableCap.CullFace );
        }

        protected override void OnEnd()
        {
            GL.Disable( EnableCap.DepthTest | EnableCap.CullFace );
        }
    }
}
