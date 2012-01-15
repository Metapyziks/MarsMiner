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
using System.Collections.Generic;

using OpenTK;

using MarsMiner.Client.Graphics;

namespace MarsMiner.Client.UI
{
    public class ResizeEventArgs : EventArgs
    {
        public readonly Vector2 Size;

        public ResizeEventArgs( Vector2 size )
        {
            Size = size;
        }
    }

    public delegate void ResizeEventHandler( Object sender, ResizeEventArgs e );

    public class RepositionEventArgs : EventArgs
    {
        public readonly Vector2 Position;

        public RepositionEventArgs( Vector2 position )
        {
            Position = position;
        }
    }

    public delegate void RepositionEventHandler( Object sender, RepositionEventArgs e );

    public class VisibilityChangedEventArgs : EventArgs
    {
        public readonly bool Visible;
        public bool Hidden
        {
            get
            {
                return !Visible;
            }
        }

        public VisibilityChangedEventArgs( bool visible )
        {
            Visible = visible;
        }
    }

    public delegate void VisibilityChangedEventHandler( Object sender, VisibilityChangedEventArgs e );

    public class EnabledChangedEventArgs : EventArgs
    {
        public readonly bool Enabled;
        public bool Disabled
        {
            get
            {
                return !Enabled;
            }
        }

        public EnabledChangedEventArgs( bool enabled )
        {
            Enabled = enabled;
        }
    }

    public delegate void EnabledChangedEventHandler( Object sender, EnabledChangedEventArgs e );

    public delegate void MouseButtonEventHandler( Object sender, OpenTK.Input.MouseButtonEventArgs e );

    public delegate void MouseMoveEventHandler( Object sender, OpenTK.Input.MouseMoveEventArgs e );

    public delegate void KeyPressEventHandler( Object sender, KeyPressEventArgs e );

    public class RenderEventArgs : EventArgs
    {
        public readonly SpriteShader ShaderProgram;
        public readonly Vector2 DrawPosition;

        public RenderEventArgs( SpriteShader shader, Vector2 drawPosition )
        {
            ShaderProgram = shader;
            DrawPosition = drawPosition;
        }
    }

    public delegate void RenderEventHandler( Object sender, RenderEventArgs e );

    public class UIObject
    {
        private Vector2 mySize;
        private Vector2 myPosition;
        private Vector2 myPaddingTopLeft;
        private Vector2 myPaddingBottomRight;
        private bool myVisible;
        private bool myEnabled;
        private bool myFocused;
        private List<UIObject> myChildren;
        private Vector2 myMousePos;
        private bool myMouseOver;
        private bool myMouseDown;
        private UIObject myParent;

        protected bool CanReposition;
        protected bool CanResize;
        protected bool CanBringToFront;

        public OpenTK.Graphics.Color4 DisabledColour = new OpenTK.Graphics.Color4( 95, 95, 95, 255 );

        public Vector2 Size
        {
            get
            {
                return mySize;
            }
            set
            {
                SetSize( value );
            }
        }

        public Vector2 Position
        {
            get
            {
                return myPosition;
            }
            set
            {
                SetPosition( value );
            }
        }

        public float Left
        {
            get
            {
                return myPosition.X;
            }
            set
            {
                SetPosition( value, myPosition.Y );
            }
        }

        public float Top
        {
            get
            {
                return myPosition.Y;
            }
            set
            {
                SetPosition( myPosition.X, value );
            }
        }

        public float Right
        {
            get
            {
                return myPosition.X + mySize.X;
            }
            set
            {
                SetPosition( value - mySize.X, myPosition.Y );
            }
        }

        public float Bottom
        {
            get
            {
                return myPosition.Y + mySize.Y;
            }
            set
            {
                SetPosition( myPosition.X, value - mySize.Y );
            }
        }

        public float Width
        {
            get
            {
                return mySize.X;
            }
            set
            {
                SetSize( value, mySize.Y );
            }
        }

        public float Height
        {
            get
            {
                return mySize.Y;
            }
            set
            {
                SetSize( mySize.X, value );
            }
        }

        public float PaddingLeft
        {
            get
            {
                return myPaddingTopLeft.X;
            }
            set
            {
                myPaddingTopLeft.X = value;
            }
        }

        public float PaddingTop
        {
            get
            {
                return myPaddingTopLeft.Y;
            }
            set
            {
                myPaddingTopLeft.Y = value;
            }
        }

        public float PaddingRight
        {
            get
            {
                return myPaddingBottomRight.X;
            }
            set
            {
                myPaddingBottomRight.X = value;
            }
        }

        public float PaddingBottom
        {
            get
            {
                return myPaddingBottomRight.Y;
            }
            set
            {
                myPaddingBottomRight.Y = value;
            }
        }

        public float InnerWidth
        {
            get
            {
                return mySize.X - myPaddingTopLeft.X - myPaddingBottomRight.X;
            }
        }

        public float InnerHeight
        {
            get
            {
                return mySize.Y - myPaddingTopLeft.Y - myPaddingBottomRight.Y;
            }
        }

        public bool IsVisible
        {
            get
            {
                return myVisible && ( Parent == null || Parent.IsVisible );
            }
            set
            {
                if ( value )
                    Show();
                else
                    Hide();
            }
        }

        public bool IsEnabled
        {
            get
            {
                return myEnabled && ( Parent == null || Parent.IsEnabled );
            }
            set
            {
                if ( value )
                    Enable();
                else
                    Disable();
            }
        }

        public bool IsFocused
        {
            get
            {
                return myFocused;
            }
        }

        public Vector2 MousePosition
        {
            get
            {
                return myMousePos;
            }
        }

        public bool MouseOver
        {
            get
            {
                return myMouseOver;
            }
        }

        public bool MouseButtonPressed
        {
            get
            {
                return myMouseDown;
            }
        }

        public UIObject Parent
        {
            get
            {
                return myParent;
            }
        }

        public UIObject[] Children
        {
            get
            {
                return myChildren.ToArray();
            }
        }

        public UIObject()
            : this( new Vector2(), new Vector2() )
        {

        }

        public UIObject( Vector2 size )
            : this( size, new Vector2() )
        {

        }

        public UIObject( Vector2 size, Vector2 position )
        {
            mySize = size;
            myPosition = position;
            myVisible = true;
            myEnabled = true;
            myFocused = false;
            myMouseOver = false;
            myMouseDown = false;

            myChildren = new List<UIObject>();

            CanReposition = true;
            CanResize = true;
            CanBringToFront = false;
        }

        public event ResizeEventHandler Resize;

        protected virtual Vector2 OnSetSize( Vector2 newSize )
        {
            return newSize;
        }

        public event RepositionEventHandler Reposition;

        protected virtual Vector2 OnSetPosition( Vector2 newPosition )
        {
            return newPosition;
        }

        public event EventHandler Focused;

        protected virtual void OnFocus()
        {

        }

        public event EventHandler UnFocused;

        protected virtual void OnUnFocus()
        {

        }

        public event VisibilityChangedEventHandler VisibilityChanged;
        public event EventHandler Shown;

        protected virtual void OnShow()
        {

        }

        public event EventHandler Hidden;

        protected virtual void OnHide()
        {

        }

        public event EnabledChangedEventHandler EnabledChanged;
        public event EventHandler Enabled;

        protected virtual void OnEnable()
        {

        }

        public event EventHandler Disabled;

        protected virtual void OnDisable()
        {

        }

        public event MouseButtonEventHandler MouseDown;

        protected virtual void OnMouseDown( Vector2 mousePos, OpenTK.Input.MouseButton mouseButton )
        {

        }

        public event MouseButtonEventHandler MouseUp;

        protected virtual void OnMouseUp( Vector2 mousePos, OpenTK.Input.MouseButton mouseButton )
        {

        }

        public event MouseButtonEventHandler Click;

        protected virtual void OnClick( Vector2 mousePos, OpenTK.Input.MouseButton mouseButton )
        {

        }

        public event MouseMoveEventHandler MouseMove;

        protected virtual void OnMouseMove( Vector2 mousePos )
        {

        }

        public event MouseMoveEventHandler MouseEnter;

        protected virtual void OnMouseEnter( Vector2 mousePos )
        {

        }

        public event MouseMoveEventHandler MouseLeave;

        protected virtual void OnMouseLeave( Vector2 mousePos )
        {

        }

        public event KeyPressEventHandler KeyPress;

        protected virtual void OnKeyPress( KeyPressEventArgs e )
        {

        }

        public event RenderEventHandler RenderObject;

        protected virtual void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {

        }

        protected virtual bool CheckPositionWithinBounds( Vector2 pos )
        {
            return IsVisible &&
                pos.X >= 0 &&
                pos.Y >= 0 &&
                pos.X < Size.X &&
                pos.Y < Size.Y;
        }

        public void SetSize( float width, float height )
        {
            SetSize( new Vector2( width, height ) );
        }

        public void SetSize( Vector2 size )
        {
            if ( CanResize )
            {
                mySize = OnSetSize( size );
                if ( Resize != null )
                    Resize( this, new ResizeEventArgs( size ) );
            }
        }

        public void SetPosition( float x, float y )
        {
            SetPosition( new Vector2( x, y ) );
        }

        public void SetPosition( Vector2 position )
        {
            if ( CanReposition )
            {
                myPosition = OnSetPosition( position );
                if ( Reposition != null )
                    Reposition( this, new RepositionEventArgs( position ) );
            }
        }

        public void CentreHorizontally()
        {
            if ( Parent != null )
                Left = ( Parent.InnerWidth - Width ) / 2.0f;
        }

        public void CentreVertically()
        {
            if ( Parent != null )
                Top = ( Parent.InnerHeight - Height ) / 2.0f;
        }

        public void Centre()
        {
            if( Parent != null )
                Position = new Vector2( Parent.InnerWidth - Width, Parent.InnerHeight - Height ) / 2.0f;
        }

        public void Focus()
        {
            myFocused = true;

            if ( Parent != null )
            {
                foreach ( UIObject child in Parent.myChildren )
                    if ( child.IsFocused && child != this )
                        child.UnFocus();
            }

            OnFocus();
            if ( Focused != null )
                Focused( this, new EventArgs() );
        }

        public void UnFocus()
        {
            myFocused = false;

            foreach ( UIObject child in myChildren )
                if ( child.IsFocused )
                    child.UnFocus();

            OnUnFocus();
            if ( UnFocused != null )
                UnFocused( this, new EventArgs() );
        }

        public void Show()
        {
            if ( !myVisible )
            {
                OnShow();
                if ( VisibilityChanged != null )
                    VisibilityChanged( this, new VisibilityChangedEventArgs( true ) );
                if ( Shown != null )
                    Shown( this, new EventArgs() );
            }
            myVisible = true;
        }

        public void Hide()
        {
            if ( myFocused )
                UnFocus();

            if ( myVisible )
            {
                OnHide();
                if ( VisibilityChanged != null )
                    VisibilityChanged( this, new VisibilityChangedEventArgs( false ) );
                if ( Hidden != null )
                    Hidden( this, new EventArgs() );
            }
            myVisible = false;
        }

        public void Enable()
        {
            if( !myEnabled )
            {
                OnEnable();
                if ( EnabledChanged != null )
                    EnabledChanged( this, new EnabledChangedEventArgs( true ) );
                if ( Enabled != null )
                    Enabled( this, new EventArgs() );
            }
            myEnabled = true;
        }

        public void Disable()
        {
            if ( myEnabled )
            {
                myMouseDown = false;
                myMouseOver = false;

                OnDisable();
                if ( EnabledChanged != null )
                    EnabledChanged( this, new EnabledChangedEventArgs( false ) );
                if ( Disabled != null )
                    Disabled( this, new EventArgs() );
            }
            myEnabled = false;
        }

        public UIObject GetFirstIntersector( Vector2 pos )
        {
            if ( myChildren.Count > 0 )
            {
                UIObject intersector = null;

                for ( int i = myChildren.Count - 1; i >= 0; --i )
                {
                    UIObject child = myChildren[ i ];

                    if ( child.IsVisible && ( intersector = child.GetFirstIntersector( pos - myPaddingTopLeft - child.Position ) ) != null )
                        return intersector;
                }
            }

            if ( CheckPositionWithinBounds( pos ) )
                return this;

            return null;
        }

        public void SendMouseButtonEvent( Vector2 mousePos, OpenTK.Input.MouseButtonEventArgs e )
        {
            if ( e.IsPressed )
            {
                if ( myChildren.Count > 0 )
                {
                    UIObject intersector = null;

                    for ( int i = myChildren.Count - 1; i >= 0; --i )
                    {
                        UIObject child = myChildren[ i ];

                        Vector2 relativePos = mousePos - myPaddingTopLeft - child.Position;

                        if ( child.IsVisible && ( intersector = child.GetFirstIntersector( relativePos ) ) != null )
                        {
                            if ( child.IsEnabled )
                            {
                                if ( child.CanBringToFront )
                                {
                                    myChildren.Remove( child );
                                    myChildren.Add( child );
                                }

                                child.SendMouseButtonEvent( relativePos, e );
                            }

                            if ( IsEnabled )
                            {
                                Focus();

                                if ( !child.IsEnabled )
                                {
                                    foreach ( UIObject ch in myChildren )
                                        if ( ch.IsFocused )
                                            ch.UnFocus();

                                    myMouseDown = true;
                                    OnMouseDown( mousePos, e.Button );
                                    if ( MouseDown != null )
                                        MouseDown( this, e );
                                }
                            }
                            return;
                        }
                    }
                }

                if ( CheckPositionWithinBounds( mousePos ) )
                {
                    if ( IsEnabled )
                    {
                        Focus();

                        foreach ( UIObject ch in myChildren )
                            if ( ch.IsFocused )
                                ch.UnFocus();

                        myMouseDown = true;
                        OnMouseDown( mousePos, e.Button );
                        if ( MouseDown != null )
                            MouseDown( this, e );
                    }
                }
            }
            else
            {
                UIObject intersector = null;

                if ( IsVisible && ( intersector = GetFirstIntersector( mousePos ) ) != null )
                {
                    OnMouseUp( mousePos, e.Button );
                    if ( MouseUp != null )
                        MouseUp( this, e );
                }

                if ( myMouseDown )
                {
                    myMouseDown = false;

                    if ( IsVisible && intersector != null )
                    {
                        OnClick( mousePos, e.Button );

                        if ( Click != null )
                            Click( this, e );
                    }
                }
                else
                {
                    if ( myChildren.Count > 0 )
                    {
                        for ( int i = myChildren.Count - 1; i >= 0 && i < myChildren.Count; --i )
                        {
                            UIObject child = myChildren[ i ];

                            Vector2 relativePos = mousePos - myPaddingTopLeft - child.Position;

                            if ( child.IsEnabled )
                                child.SendMouseButtonEvent( relativePos, e );
                        }
                    }
                }
            }
        }

        public void SendMouseMoveEvent( Vector2 newPos, OpenTK.Input.MouseMoveEventArgs e )
        {
            if ( IsEnabled && IsVisible && newPos != myMousePos )
            {
                OnMouseMove( newPos );
                if ( MouseMove != null )
                    MouseMove( this, e );
            }

            myMousePos = newPos;

            bool mouseNowOver = CheckPositionWithinBounds( newPos );
            if ( IsEnabled && IsVisible && mouseNowOver != myMouseOver )
            {
                myMouseOver = mouseNowOver;

                if ( myMouseOver )
                {
                    OnMouseEnter( myMousePos );
                    if ( MouseEnter != null )
                        MouseEnter( this, e );
                }
                else
                {
                    OnMouseLeave( myMousePos );
                    if ( MouseLeave != null )
                        MouseLeave( this, e );
                }
            }

            for( int i = myChildren.Count - 1; i >= 0; -- i )
                myChildren[ i ].SendMouseMoveEvent( newPos - myPaddingTopLeft - myChildren[ i ].Position, e );
        }

        public void SendKeyPressEvent( KeyPressEventArgs e )
        {
            if ( IsFocused && IsEnabled )
            {
                OnKeyPress( e );
                if ( KeyPress != null )
                    KeyPress( this, e );

                foreach ( UIObject child in myChildren )
                    if ( child.IsFocused && IsEnabled )
                    {
                        child.SendKeyPressEvent( e );
                        break;
                    }
            }
        }

        public void AddChild( UIObject child )
        {
            if ( child.myParent != null )
                child.myParent.RemoveChild( child );

            myChildren.Add( child );
            child.myParent = this;

            if ( child is UIWindow )
            {
                ( child as UIWindow ).Closed += delegate( object sender, EventArgs e )
                {
                    RemoveChild( sender as UIWindow );
                };
            }
        }

        public void RemoveChild( UIObject child )
        {
            if ( myChildren.Contains( child ) )
            {
                myChildren.Remove( child );
                child.myParent = null;
            }
        }

        public void Render( SpriteShader shader, Vector2 parentPosition = new Vector2() )
        {
            if ( IsVisible )
            {
                parentPosition += Position;

                OnRender( shader, parentPosition );
                if ( RenderObject != null )
                    RenderObject( this, new RenderEventArgs( shader, parentPosition ) );

                try
                {
                    foreach ( UIObject child in myChildren )
                        child.Render( shader, parentPosition + myPaddingTopLeft );
                }
                catch ( Exception )
                {

                }
            }
        }
    }
}
