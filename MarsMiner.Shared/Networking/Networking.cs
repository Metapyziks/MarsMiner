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
using System.IO;
using System.Net;

namespace MarsMiner.Shared.Networking
{
    public class PacketType
    {
        public UInt16 ID;
        public String Name;

        protected PacketType( UInt16 id, String name )
        {
            ID = id;
            Name = name;
        }
    }

    public static class NetworkConstants
    {
        public const ushort ProtocolVersion = 0x0000;
    }

    public enum DisconnectReason : byte
    {
        ServerStopping = 0x00,
        ServerFull = 0x01,
        ProtocolVersionMismatch = 0x02,
        Kicked = 0x03,
        Timeout = 0x04,
        ClientDisconnect = 0x05,
        BadPassword = 0x06,
        ResourceNotFound = 0x07
    }

    public enum MessageType : byte
    {
        Server = 0x00,
        Chat = 0x01,
        TeamChat = 0x02
    }

    public class RemoteNetworkedObject
    {
        private DateTime myLastReceivedTime;
        private bool myTimingOut;

        public double TimeOutDelay;
        public double AliveCheckPeriod;

        public bool IsConnected { get; private set; }

        public byte AuthLevel { get; protected set; }

        public bool IsAdmin
        {
            get
            {
                return AuthLevel > 127;
            }
        }

        public double SecondsSinceLastPacket
        {
            get
            {
                return ( DateTime.Now - myLastReceivedTime ).TotalSeconds;
            }
        }

        public bool TimingOut
        {
            get
            {
                if ( !myTimingOut && SecondsSinceLastPacket >= AliveCheckPeriod )
                {
                    SendAliveCheck( true );
                    myTimingOut = true;
                }
                else if ( myTimingOut && SecondsSinceLastPacket < AliveCheckPeriod )
                    myTimingOut = false;

                return myTimingOut;
            }
        }

        public virtual IPAddress IPAddress
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public String SanitisedIPAddress
        {
            get
            {
                return IPAddress.ToString().Replace( '.', '_' );
            }
        }

        public RemoteNetworkedObject()
        {
            myLastReceivedTime = DateTime.Now;
            myTimingOut = false;

            TimeOutDelay = 30.0;
            AliveCheckPeriod = 2.0;
        }

        public virtual void CheckForPackets()
        {
            throw new NotImplementedException();
        }

        protected virtual bool ReadPacket( Stream stream )
        {
            OnReceivePacket();
            myLastReceivedTime = DateTime.Now;
            myTimingOut = false;

            return true;
        }

        protected virtual void OnReceivePacket()
        {
            return;
        }

        public virtual Stream StartPacket( String typeName )
        {
            throw new NotImplementedException();
        }

        public virtual Stream StartPacket( PacketType type )
        {
            throw new NotImplementedException();
        }

        protected Stream StartPacket( PacketType type, Stream stream )
        {
            stream.Write( BitConverter.GetBytes( type.ID ), 0, 2 );
            return stream;
        }

        public virtual void SendPacket()
        {
            throw new NotImplementedException();
        }

        public void SendPacket( PacketType type )
        {
            StartPacket( type );
            SendPacket();
        }

        public virtual void Disconnect()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnConnect()
        {
            IsConnected = true;
        }

        protected virtual void OnDisconnect( DisconnectReason reason, String comment )
        {
            IsConnected = false;
            return;
        }

        public void SendAliveCheck( bool expectReply )
        {
            Stream str = StartPacket( "AliveCheck" );
            str.WriteByte( (byte) ( expectReply ? 0xFF : 0x00 ) );
            SendPacket();
        }

        protected bool OnReceiveAliveCheck( Stream stream )
        {
            switch ( stream.ReadByte() )
            {
                case 0x00:
                    return true;
                case 0xFF:
                    SendAliveCheck( false );
                    return true;
                default:
                    return false;
            }
        }

        public virtual bool PacketPending()
        {
            throw new NotImplementedException();
        }
    }
}