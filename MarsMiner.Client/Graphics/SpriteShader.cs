using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    public class SpriteShader : ShaderProgram2D
    {
        private Texture myTexture;

        public Texture Texture
        {
            get
            {
                return myTexture;
            }
            set
            {
                if ( myTexture != value )
                {
                    SetTexture( "texture0", value );
                    myTexture = value;
                }
            }
        }

        public SpriteShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, true );
            vert.AddAttribute( ShaderVarType.Vec2, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec2, "in_texture" );
            vert.AddAttribute( ShaderVarType.Vec4, "in_colour" );
            vert.AddVarying( ShaderVarType.Vec2, "var_texture" );
            vert.AddVarying( ShaderVarType.Vec4, "var_colour" );
            vert.Logic = @"
                void main( void )
                {
                    var_texture = in_texture;
                    var_colour = in_colour;

                    gl_Position = in_position;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, true );
            frag.AddUniform( ShaderVarType.Sampler2D, "texture0" );
            frag.AddVarying( ShaderVarType.Vec2, "var_texture" );
            frag.AddVarying( ShaderVarType.Vec4, "var_colour" );
            frag.Logic = @"
                void main( void )
                {
                    vec4 clr = texture( texture0, var_texture ) * var_colour;

                    if( clr.a != 0.0 )
                        out_frag_colour = vec4( clr.rgba );
                    else
                        discard;
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;
        }

        public SpriteShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            if ( NVidiaCard )
            {
                AddAttribute( "in_texture", 2, 2 );
                AddAttribute( "in_colour", 4, 4 );
                AddAttribute( "in_position", 2, 0 );
            }
            else
            {
                AddAttribute( "in_position", 2 );
                AddAttribute( "in_texture", 2 );
                AddAttribute( "in_colour", 4 );
            }

            AddTexture( "texture0", TextureUnit.Texture0 );
        }

        protected override void OnStartBatch()
        {
            GL.BlendFunc( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
        }
    }
}
