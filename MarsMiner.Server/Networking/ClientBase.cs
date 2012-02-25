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

namespace MarsMiner.Server.Networking
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
        public readonly bool TeamChat;
        public readonly String Message;

        public MessageEventArgs( String message, bool team )
        {
            TeamChat = team;
            Message = message;
        }
    }

    public class ClientBase : RemoteNetworkedObject
    {
        protected static readonly ClientPacketType PTAliveCheck =
            PacketManager.Register( "AliveCheck", delegate( ClientBase sender,
                ClientPacketType type, Stream stream )
            {
                return sender.OnReceiveAliveCheck( stream );
            } );

        protected static readonly ClientPacketType PTPacketDictionary =
            PacketManager.Register( "PacketDictionary", delegate( ClientBase sender,
                ClientPacketType type, Stream stream )
            {
                return sender.OnReceivePacketDictionary( stream );
            } );

        protected static readonly ClientPacketType PTDisconnect =
            PacketManager.Register( "Disconnect", delegate( ClientBase sender,
                ClientPacketType type, Stream stream )
            {
                return sender.OnReceiveDisconnect( stream );
            } );

        protected static readonly ClientPacketType PTMessage =
            PacketManager.Register( "Message", delegate( ClientBase sender,
                ClientPacketType type, Stream stream )
            {
                return sender.OnReceiveMessage( stream );
            } );

        protected static readonly ClientPacketType PTHandshake =
            PacketManager.Register( "Handshake", delegate( ClientBase sender,
                ClientPacketType type, Stream stream )
            {
                return sender.OnReceiveHandshake( stream );
            } );


        public event EventHandler<DisconnectEventArgs> Disconnected;
        public event EventHandler<MessageEventArgs> ReceivedMessage;

        public int Slot;

        public GameServer GameServer { get; private set; }

        public ClientBase( GameServer server )
        {
            GameServer = server;
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
            Stream stream = StartPacket( PTPacketDictionary );
            BinaryWriter writer = new BinaryWriter( stream );
            ClientPacketType[] types = PacketManager.GetAllTypes();
            writer.Write( (UInt16) types.Length );
            foreach ( ClientPacketType t in types )
            {
                writer.Write( t.Name );
                writer.Write( t.ID );
            }
        }

        protected bool OnReceivePacketDictionary( Stream stream )
        {
            return true;
        }

        public void SendDisconnect( DisconnectReason reason, String comment = "" )
        {
            Stream stream = StartPacket( PTDisconnect );
            BinaryWriter writer = new BinaryWriter( stream );
            writer.Write( (byte) reason );
            writer.Write( comment );
            SendPacket();

            OnDisconnect( reason, comment );

            if ( Disconnected != null )
                Disconnected( this, new DisconnectEventArgs( reason, comment ) );
        }

        protected bool OnReceiveDisconnect( Stream stream )
        {
            OnDisconnect( DisconnectReason.ClientDisconnect, "" );

            if ( Disconnected != null )
                Disconnected( this, new DisconnectEventArgs( DisconnectReason.ClientDisconnect, "" ) );

            return true;
        }

        public void SendMessage( MessageType type, String message )
        {
            Stream stream = StartPacket( PTMessage );
            BinaryWriter writer = new BinaryWriter( stream );
            writer.Write( (byte) type );
            writer.Write( message );
            SendPacket();
        }

        protected bool OnReceiveMessage( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            MessageType type = (MessageType) reader.ReadByte();
            String message = reader.ReadString();

            if ( type != MessageType.Chat && type != MessageType.TeamChat )
                return false;

            bool team = type == MessageType.TeamChat;

            OnReceiveMessage( message, team );

            if ( ReceivedMessage != null )
                ReceivedMessage( this, new MessageEventArgs( message, team ) );

            return true;
        }

        protected virtual void OnReceiveMessage( String message, bool team )
        {
            return;
        }

        private void SendHandshake()
        {
            Stream stream = StartPacket( PTHandshake );
            BinaryWriter writer = new BinaryWriter( stream );
            writer.Write( GameServer.Name );
            writer.Write( GameServer.PasswordRequired );
            writer.Write( GameServer.SlotCount );
            writer.Write( GameServer.ClientCount );
            SendPacket();
        }

        protected bool OnReceiveHandshake( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            ushort version = reader.ReadUInt16();
            if ( version != NetworkConstants.ProtocolVersion )
            {
                SendDisconnect( DisconnectReason.ProtocolVersionMismatch );
                return false;
            }

            if ( GameServer.ClientCount >= GameServer.SlotCount )
            {
                SendDisconnect( DisconnectReason.ServerFull );
                return false;
            }

            SendHandshake();
            return true;
        }
    }
}
