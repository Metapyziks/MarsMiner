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
using System.Threading;

using MarsMiner.Shared.Networking;
using MarsMiner.Shared.Geometry;

namespace MarsMiner.Server.Networking
{
    public class GameServer
    {
        public String Name { get; private set; }
        public String Password { get; private set; }

        public bool PasswordRequired
        {
            get { return Password != null && Password != ""; }
        }

        public bool IsRunning { get; private set; }

        public World World { get; private set; }

        public ClientBase[] Slots { get; private set; }
        public int SlotCount
        {
            get { return Slots.Length; }
        }

        public int ClientCount { get; private set; }
        public bool CanAcceptClients
        {
            get { return ClientCount < SlotCount; }
        }
        public int NextFreeSlot
        {
            get
            {
                for ( int i = 0; i < SlotCount; ++i )
                    if ( Slots[ i ] == null )
                        return i;

                return -1;
            }
        }

        public GameServer( ServerBuilder builder )
        {
            Name = builder.Name;
            Password = builder.Password;

            Slots = new ClientBase[ builder.SlotCount ];
            ClientCount = 0;

            World = new World();
        }

        public void Run()
        {
            IsRunning = true;

            World.Generate( 1024, 1024, 4 );

            while ( IsRunning )
            {
                if ( CanAcceptClients )
                    ListenForConnections();

                if ( ClientCount > 0 )
                    UpdateClients();
            }
        }

        private void ListenForConnections()
        {
            if ( LocalConnection.ConnectionWaiting )
                AddClient( new LocalClient( this ) );
        }

        private void UpdateClients()
        {
            for ( int i = 0; i < Slots.Length; ++i )
            {
                ClientBase client = Slots[ i ];
                if ( client != null )
                {
                    client.CheckForPackets();
                }
            }
        }

        protected void AddClient( ClientBase client )
        {
            client.Slot = NextFreeSlot;
            Slots[ client.Slot ] = client;
            ++ClientCount;
        }

        protected void RemoveClient( ClientBase client )
        {
            Slots[ client.Slot ] = null;
            --ClientCount;
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
