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
