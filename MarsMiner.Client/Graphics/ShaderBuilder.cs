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
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics.OpenGL;

namespace MarsMiner.Client.Graphics
{
    public enum ShaderVarType
    {
        Int,
        Float,
        Vec2,
        Vec3,
        Vec4,
        Sampler2D,
        Mat4
    }

    public class ShaderBuilder
    {
        private struct ShaderVariable
        {
            public ShaderVarType Type;
            public String Identifier;

            public String TypeString
            {
                get
                {
                    String str = Type.ToString();

                    return str[ 0 ].ToString().ToLower()
                        + str.Substring( 1 );
                }
            }
        }

        private bool myTwoDimensional;

        private List<ShaderVariable> myUniforms;
        private List<ShaderVariable> myAttribs;
        private List<ShaderVariable> myVaryings;

        public ShaderType Type { get; private set; }

        public String Logic { get; set; }
        public String FragOutIdentifier { get; set; }

        public ShaderBuilder( ShaderType type, bool twoDimensional )
        {
            Type = type;
            myTwoDimensional = twoDimensional;

            myUniforms = new List<ShaderVariable>();
            myAttribs  = new List<ShaderVariable>();
            myVaryings = new List<ShaderVariable>();

            Logic = "";
            FragOutIdentifier = "out_frag_colour";
        }

        public void AddUniform( ShaderVarType type, String identifier )
        {
            myUniforms.Add( new ShaderVariable { Type = type, Identifier = identifier } );
        }

        public void AddAttribute( ShaderVarType type, String identifier )
        {
            myAttribs.Add( new ShaderVariable { Type = type, Identifier = identifier } );
        }

        public void AddVarying( ShaderVarType type, String identifier )
        {
            myVaryings.Add( new ShaderVariable { Type = type, Identifier = identifier } );
        }

        public String Generate( bool gl3 )
        {
            String nl = Environment.NewLine;

            String output = 
                "#version 1" + ( gl3 ? "3" : "2" ) + "0" + nl + nl
                + ( gl3 ? "precision highp float;" + nl + nl : "" )
                + ( Type == ShaderType.VertexShader && myTwoDimensional
                    ? "uniform vec2 screen_resolution;" + nl + nl
                    : "" );

            foreach ( ShaderVariable var in myUniforms )
                output += "uniform "
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if( myUniforms.Count != 0 )
                output += nl;

            foreach ( ShaderVariable var in myAttribs )
                output += ( gl3 ? "in " : "attribute " )
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if( myAttribs.Count != 0 )
                output += nl;

            foreach ( ShaderVariable var in myVaryings )
                output += ( gl3 ? Type == ShaderType.VertexShader
                    ? "out " : "in " : "varying " )
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if ( gl3 && Type == ShaderType.FragmentShader )
                output += "out vec4 " + FragOutIdentifier + ";" + nl + nl;

            int index = Logic.IndexOf( "void" ) - 1;
            String indent = "";
            while ( index >= 0 && Logic[ index ] == ' ' )
                indent += Logic[ index-- ];

            indent = new String( indent.Reverse().ToArray() );

            String logic = indent.Length == 0 ? Logic.Trim() : Logic.Trim().Replace( indent, "" );

            if ( Type == ShaderType.FragmentShader )
            {
                if ( !gl3 )
                    logic = logic.Replace( FragOutIdentifier, "gl_FragColor" )
                        .Replace( "texture(", "texture2D(" );
            }
            else if( myTwoDimensional )
            {
                logic = logic.Replace( "gl_Position", "vec2 _pos_" );
                index = logic.IndexOf( "_pos_" );
                index = logic.IndexOf( ';', index ) + 1;
                logic = logic.Insert( index, nl
                    + "    _pos_ -= screen_resolution / 2.0;" + nl
                    + "    _pos_.x /= screen_resolution.x / 2.0;" + nl
                    + "    _pos_.y /= -screen_resolution.y / 2.0;" + nl
                    + "    gl_Position = vec4( _pos_, 0.0, 1.0 );" );
            }

            output += logic;

            return output;
        }
    }
}
