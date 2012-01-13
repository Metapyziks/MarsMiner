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
        private Matrix4 myViewMatrix;
        private int myViewMatrixLoc;

        private Vector3 myCameraPosition;
        private Vector2 myCameraRotation;
        private Matrix4 myPerspectiveMatrix;

        private bool myViewChanged;

        public Vector3 CameraPosition
        {
            get { return myCameraPosition; }
            set
            {
                myCameraPosition = value;
                myViewChanged = true;
            }
        }
        public Vector2 CameraRotation
        {
            get { return myCameraRotation; }
            set
            {
                myCameraRotation = value;
                myViewChanged = true;
            }
        }
        public Matrix4 PerspectiveMatrix
        {
            get { return myPerspectiveMatrix; }
            set
            {
                myPerspectiveMatrix = value;
                myViewChanged = true;
            }
        }

        public bool LineMode { get; set; }

        public OctreeTestShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec4, "in_colour" );
            vert.AddVarying( ShaderVarType.Vec4, "var_colour" );
            vert.Logic = @"
                void main( void )
                {
                    var_colour = in_colour;
                    gl_Position = view_matrix * vec4( in_position, 1.0 );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddVarying( ShaderVarType.Vec4, "var_colour" );
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = var_colour;
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;

            CameraPosition = new Vector3();
            CameraRotation = new Vector2();
        }

        public OctreeTestShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        public void SetScreenSize( int width, int height )
        {
            PerspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(
                (float) Math.PI * ( 60.0f / 180.0f ), (float) width / (float) height, 0.125f, 1024.0f );
            UpdateViewMatrix();
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
            Matrix4 yRot = Matrix4.CreateRotationY( CameraRotation.Y );
            Matrix4 xRot = Matrix4.CreateRotationX( CameraRotation.X );
            Matrix4 trns = Matrix4.CreateTranslation( -CameraPosition );

            myViewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot), xRot ), PerspectiveMatrix );

            GL.UniformMatrix4( myViewMatrixLoc, false, ref myViewMatrix );
        }

        protected override void OnStartBatch()
        {
            if ( myViewChanged )
                UpdateViewMatrix();

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.CullFace );

            GL.CullFace( CullFaceMode.Front );

            if ( LineMode )
                GL.PolygonMode( MaterialFace.Front, PolygonMode.Line );
        }

        protected override void OnEndBatch()
        {
            if ( LineMode )
                GL.PolygonMode( MaterialFace.Front, PolygonMode.Fill );

            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.CullFace );
        }
    }
}
