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

using ResourceLib;

using MarsMiner.Client.Graphics;

namespace MarsMiner.Client.UI
{
    public class UIWindow : UIObject
    {
        private float myScale;
        private FrameSprite myFrameSprite;
        private UILabel myTitleText;
        private UIWindowCloseButton myCloseButton;
        private bool myDragging;
        private Vector2 myDragPos;
        private bool myCanClose;

        public bool CanDrag;

        public String Title
        {
            get
            {
                return myTitleText.Text;
            }
            set
            {
                myTitleText.Text = value;
            }
        }

        public bool CanClose
        {
            get
            {
                return myCanClose;
            }
            set
            {
                myCanClose = value;
                myCloseButton.IsEnabled = value;
                myCloseButton.IsVisible = value;
            }
        }

        public UIWindow( float scale = 1.0f )
            : this( new Vector2(), new Vector2(), scale )
        {

        }

        public UIWindow( Vector2 size, float scale = 1.0f )
            : this( size, new Vector2(), scale )
        {

        }

        public UIWindow( Vector2 size, Vector2 position, float scale = 1.0f )
            : base( size, position )
        {
            myScale = scale;

            PaddingLeft = 4.0f * scale;
            PaddingTop = 20.0f * scale;
            PaddingRight = 4.0f * scale;
            PaddingBottom = 4.0f * scale;

            myFrameSprite = new FrameSprite( Res.Get<Texture>( "images_gui_panels" ), scale )
            {
                SubrectSize = new Vector2( 32, 32 ),
                SubrectOffset = new Vector2( 0, 0 ),
                FrameTopLeftOffet = new Vector2( 4, 20 ),
                FrameBottomRightOffet = new Vector2( 4, 4 ),
                Size = size
            };

            myTitleText = new UILabel( Font.Large, scale )
            {
                Position = new Vector2( 6 * scale - PaddingLeft, 4 * scale - PaddingTop ),
                IsEnabled = false
            };

            AddChild( myTitleText );

            myCloseButton = new UIWindowCloseButton( new Vector2( size.X - 18.0f * scale - PaddingLeft, 2.0f * scale - PaddingTop ), scale );

            myCloseButton.Click += delegate( object sender, OpenTK.Input.MouseButtonEventArgs e )
            {
                Close();
            };

            AddChild( myCloseButton );

            CanBringToFront = true;
            CanClose = true;
            CanDrag = true;
        }

        public void Close()
        {
            IsEnabled = false;
            IsVisible = false;

            OnClose();
            if ( Closed != null )
                Closed( this, new EventArgs() );
        }

        public event EventHandler Closed;

        protected virtual void OnClose()
        {

        }

        protected override Vector2 OnSetSize( Vector2 newSize )
        {
            myFrameSprite.Size = newSize;
            myCloseButton.Left = newSize.X - 18.0f * myScale - PaddingLeft;

            return base.OnSetSize( newSize );
        }

        protected override void OnMouseDown( Vector2 mousePos, OpenTK.Input.MouseButton mouseButton )
        {
            if ( CanDrag && mousePos.Y < 20 * myScale )
            {
                myDragging = true;
                myDragPos = mousePos;
            }
        }

        protected override void OnMouseUp( Vector2 mousePos, OpenTK.Input.MouseButton mouseButton )
        {
            myDragging = false;
        }

        protected override void OnMouseMove( Vector2 mousePos )
        {
            if ( myDragging )
            {
                if ( !CanDrag )
                {
                    myDragging = false;
                    return;
                }

                Position += mousePos - myDragPos;

                if ( Left < 0 )
                    Left = 0;
                if ( Top < 0 )
                    Top = 0;
                if ( Right > Parent.InnerWidth )
                    Right = Parent.InnerWidth;
                if ( Bottom > Parent.InnerHeight )
                    Bottom = Parent.InnerHeight;
            }
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            myFrameSprite.Position = renderPosition;
            myFrameSprite.Colour = ( IsEnabled ? OpenTK.Graphics.Color4.White : DisabledColour );
            myFrameSprite.Render( shader );
        }
    }
}
