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
using System.Net;
using System.Threading;

namespace MarsMiner.Client.Networking
{
    public class GameClient
    {
        public bool IsRunning { get; private set; }
        public ServerBase Server { get; private set; }

        public bool ConnectLocal()
        {
            Server = new LocalServer();
            return Server.Connect();
        }

        public bool ConnectRemote( IPAddress server, int port = 35820 )
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            IsRunning = true;

            while ( IsRunning )
            {
                if ( !Server.IsConnected )
                    Stop();
                else
                    Server.CheckForPackets();

                Thread.Sleep( 10 );
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
