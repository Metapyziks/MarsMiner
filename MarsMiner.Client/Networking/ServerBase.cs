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
using System.Linq;
using System.Text;
using System.IO;

using MarsMiner.Shared.Networking;
using MarsMiner.Shared.Geometry;

namespace MarsMiner.Client.Networking
{
    public class DisconnectEventArgs : EventArgs
    {
        public readonly DisconnectReason Reason;
        public readonly String Comment;

        public DisconnectEventArgs( DisconnectReason reason, String comment )
        {
            Reason = reason;
            Comment = comment;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public readonly MessageType Type;
        public readonly String Message;

        public MessageEventArgs( MessageType type, String message )
        {
            Type = type;
            Message = message;
        }
    }

    public class ServerBase : RemoteNetworkedObject
    {
        protected static readonly ServerPacketType PTHandshake =
            PacketManager.Register( "Handshake", 0x00, delegate( ServerBase sender,
                ServerPacketType type, Stream stream )
            {
                return sender.OnReceiveHandshake( stream );
            } );

        protected static readonly ServerPacketType PTPacketDictionary =
            PacketManager.Register( "PacketDictionary", 0x01, delegate( ServerBase sender,
                ServerPacketType type, Stream stream )
            {
                return sender.OnReceivePacketDictionary( stream );
            } );

        protected static readonly ServerPacketType PTAliveCheck =
            PacketManager.Register( "AliveCheck", 0x02, delegate( ServerBase sender,
                ServerPacketType type, Stream stream )
            {
                return sender.OnReceiveAliveCheck( stream );
            } );

        protected static readonly ServerPacketType PTDisconnect =
            PacketManager.Register( "Disconnect", 0x03, delegate( ServerBase sender,
                ServerPacketType type, Stream stream )
            {
                return sender.OnReceiveDisconnect( stream );
            } );

        protected static readonly ServerPacketType PTMessage =
            PacketManager.Register( "Message", delegate( ServerBase sender,
                ServerPacketType type, Stream stream )
            {
                return sender.OnReceiveMessage( stream );
            } );

        public event EventHandler<DisconnectEventArgs> Disconnected;
        public event EventHandler<MessageEventArgs> ReceivedMessage;

        public String Name { get; private set; }
        public String Password { get; private set; }

        public World World { get; private set; }

        public bool Connect( String password = null )
        {
            Password = password;

            if ( AttemptConnection() )
            {
                OnConnect();
                return true;
            }

            return false;
        }

        protected virtual bool AttemptConnection()
        {
            throw new NotImplementedException();
        }

        protected override bool ReadPacket( Stream stream )
        {
            base.ReadPacket( stream );
            return PacketManager.HandlePacket( this, stream );
        }

        public override Stream StartPacket( String typeName )
        {
            return StartPacket( PacketManager.GetType( typeName ) );
        }

        public void SendPacketDictionary()
        {
            StartPacket( PTPacketDictionary );
            SendPacket();
        }

        protected bool OnReceivePacketDictionary( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            UInt16 count = reader.ReadUInt16();
            for ( int i = 0; i < count; ++i )
            {
                String name = reader.ReadString();
                UInt16 id = reader.ReadUInt16();

                try
                {
                    PacketManager.SetTypeID( name, id );
                }
                catch ( KeyNotFoundException )
                {
                    return false;
                }
            }

            return true;
        }

        public void SendDisconnect()
        {
            StartPacket( PTDisconnect );
            SendPacket();

            OnDisconnect( DisconnectReason.ClientDisconnect, "" );

            if ( Disconnected != null )
                Disconnected( this, new DisconnectEventArgs( DisconnectReason.ClientDisconnect, "" ) );
        }

        protected bool OnReceiveDisconnect( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            DisconnectReason reason = (DisconnectReason) reader.ReadByte();
            String comment = reader.ReadString();

            OnDisconnect( reason, comment );

            if ( Disconnected != null )
                Disconnected( this, new DisconnectEventArgs( reason, comment ) );

            return true;
        }

        public void SendMessage( String message, bool team = false )
        {
            Stream stream = StartPacket( PTMessage );
            BinaryWriter writer = new BinaryWriter( stream );
            writer.Write( (byte) ( team ? MessageType.TeamChat : MessageType.Chat ) );
            writer.Write( message );
            SendPacket();
        }

        protected bool OnReceiveMessage( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            MessageType type = (MessageType) reader.ReadByte();
            String message = reader.ReadString();

            OnReceiveMessage( type, message );

            if ( ReceivedMessage != null )
                ReceivedMessage( this, new MessageEventArgs( type, message ) );

            return true;
        }

        protected virtual void OnReceiveMessage( MessageType type, String message )
        {
            return;
        }

        private void SendHandshake()
        {
            Stream stream = StartPacket( PTHandshake );
            BinaryWriter writer = new BinaryWriter( stream );
            writer.Write( NetworkConstants.ProtocolVersion );
            SendPacket();
        }

        protected bool OnReceiveHandshake( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            Password = reader.ReadString();

            return true;
        }
    }
}
