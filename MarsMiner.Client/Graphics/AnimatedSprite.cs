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

using OpenTK;

namespace MarsMiner.Client.Graphics
{
    public class AnimatedSprite : Sprite
    {
        private static long CurrentMilliseconds()
        {
            return DateTime.Now.Ticks / 10000;
        }

        private int myFrameWidth;
        private int myFrameHeight;

        private long myStartTime;
        private long myStopTime;

        private Vector2[] myFrameLocations;
        private int myLastFrame;

        private bool myPlaying;

        public int StartFrame;
        public int FrameCount;

        public double FrameRate;

        public AnimatedSprite( Texture texture, int frameWidth, int frameHeight, double frameRate, float scale = 1.0f )
            : base( texture, scale )
        {
            myFrameWidth = frameWidth;
            myFrameHeight = frameHeight;

            FrameRate = frameRate;

            myStartTime = 0;
            myStopTime = 0;

            SubrectSize = new Vector2( frameWidth, frameHeight );

            FindFrameLocations();

            StartFrame = 0;
            FrameCount = myFrameLocations.Length;

            myLastFrame = -1;
        }

        private void FindFrameLocations()
        {
            int xMax = Texture.Width / myFrameWidth;
            int yMax = Texture.Height / myFrameHeight;

            int frameCount = xMax * yMax;

            myFrameLocations = new Vector2[ frameCount ];

            int i = 0;

            for ( int y = 0; y < yMax; ++y )
                for ( int x = 0; x < xMax; ++x, ++ i )
                    myFrameLocations[ i ] = new Vector2( x * myFrameWidth, y * myFrameHeight );
        }

        public void Start()
        {
            if( !myPlaying )
            {
                myStartTime = CurrentMilliseconds();
                myPlaying = true;
            }
        }

        public void Stop()
        {
            if( myPlaying )
            {
                myStopTime = CurrentMilliseconds() - myStartTime;
                myPlaying = false;
            }
        }

        public void Reset()
        {
            myStopTime = 0;

            if( myPlaying )
                myStartTime = CurrentMilliseconds();
        }

        public override void Render( SpriteShader shader )
        {
            double secs = ( CurrentMilliseconds() - myStartTime + myStopTime ) / 1000.0;
            int frame = StartFrame + (int) ( (long) ( secs * FrameRate ) % (long) FrameCount );

            if ( frame != myLastFrame )
                SubrectOffset = myFrameLocations[ frame ];

            base.Render( shader );
        }
    }
}
