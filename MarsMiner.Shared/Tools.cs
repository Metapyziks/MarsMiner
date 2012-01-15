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
using System.IO;

namespace MarsMiner.Shared
{
    public static class Tools
    {
        public static bool DoesExtend( this Type self, Type type )
        {
            return self.BaseType == type || ( self.BaseType != null && self.BaseType.DoesExtend( type ) );
        }

        public static byte[] ReadBytes( this Stream self, int count )
        {
            byte[] data = new byte[ count ];
            for ( int i = 0; i < count; ++i )
            {
                int bt = self.ReadByte();
                if ( bt == -1 )
                    throw new EndOfStreamException();

                data[ i ] = (byte) bt;
            }

            return data;
        }

        #region Clamps
        public static Byte Clamp( Byte value, Byte min, Byte max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static UInt16 Clamp( UInt16 value, UInt16 min, UInt16 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static UInt32 Clamp( UInt32 value, UInt32 min, UInt32 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static UInt64 Clamp( UInt64 value, UInt64 min, UInt64 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static SByte Clamp( SByte value, SByte min, SByte max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Int16 Clamp( Int16 value, Int16 min, Int16 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Int32 Clamp( Int32 value, Int32 min, Int32 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Int64 Clamp( Int64 value, Int64 min, Int64 max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Single Clamp( Single value, Single min, Single max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }

        public static Double Clamp( Double value, Double min, Double max )
        {
            return
                ( value < min ) ? min :
                ( value > max ) ? max :
                value;
        }
        #endregion Clamps

        #region MinMax
        public static int Min( params int[] values )
        {
            int min = values[ 0 ];
            foreach ( int val in values )
                if ( val < min )
                    min = val;

            return min;
        }

        public static double Min( params double[] values )
        {
            double min = values[ 0 ];
            foreach ( double val in values )
                if ( val < min )
                    min = val;

            return min;
        }

        public static int Max( params int[] values )
        {
            int max = values[ 0 ];
            foreach ( int val in values )
                if ( val > max )
                    max = val;

            return max;
        }

        public static double Max( params double[] values )
        {
            double max = values[ 0 ];
            foreach ( double val in values )
                if ( val > max )
                    max = val;

            return max;
        }
        #endregion MinMax

        public static int FloorDiv( int numer, int denom )
        {
            return ( numer / denom ) - ( numer < 0 && ( numer % denom ) != 0 ? 1 : 0 );
        }

        public static String ApplyWordWrap( this String text, float charWidth, float wrapWidth )
        {
            if ( wrapWidth <= 0.0f )
                return text;

            String newText = "";
            int charsPerLine = (int) ( wrapWidth / charWidth );
            int x = 0, i = 0;
            while ( i < text.Length )
            {
                String word = "";
                while ( i < text.Length && !char.IsWhiteSpace( text[ i ] ) )
                    word += text[ i++ ];

                if ( x + word.Length > charsPerLine )
                {
                    if ( x == 0 )
                    {
                        newText += word.Substring( 0, charsPerLine ) + "\n" + word.Substring( charsPerLine );
                        x = word.Length - charsPerLine;
                    }
                    else
                    {
                        newText += "\n" + word;
                        x = word.Length;
                    }
                }
                else
                {
                    newText += word;
                    x += word.Length;
                }

                if ( i < text.Length )
                {
                    newText += text[ i ];
                    x++;

                    if ( text[ i++ ] == '\n' )
                        x = 0;
                }
            }

            return newText;
        }

        public static Face Opposite( Face face )
        {
            switch ( face )
            {
                case Face.Left:
                    return Face.Right;
                case Face.Right:
                    return Face.Left;
                case Face.Bottom:
                    return Face.Top;
                case Face.Top:
                    return Face.Bottom;
                case Face.Front:
                    return Face.Back;
                case Face.Back:
                    return Face.Front;
            }

            return Face.None;
        }
    }
}
