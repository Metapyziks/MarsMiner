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
using System.Reflection;

using MarsMiner.Shared.Geometry;

namespace MarsMiner.Shared
{
    public class Plugin
    {
        private static List<Plugin> myRegisteredPlugins = new List<Plugin>();

        public static void Register( String name, bool client, bool server )
        {
            Plugin plugin = Create( name, client, server );
            myRegisteredPlugins.Add( plugin );
            plugin.OnRegister();
        }

        private static Plugin Create( String name, bool client, bool server )
        {
            foreach ( Assembly asm in AppDomain.CurrentDomain.GetAssemblies() )
            {
                Type t = asm.GetType( name );
                if ( t != null )
                {
                    ConstructorInfo c = t.GetConstructor( BindingFlags.NonPublic
                        | BindingFlags.CreateInstance | BindingFlags.Instance,
                        null, new Type[] { typeof( bool ), typeof( bool ) }, null );
                    return (Plugin) c.Invoke( new object[] { client, server } );
                }
            }

            throw new Exception( "Type \"" + name + "\" not found" );
        }

        protected readonly bool Client;
        protected readonly bool Server;

        protected Plugin( bool client, bool server )
        {
            Client = client;
            Server = server;
        }

        public virtual void OnRegister()
        {
            return;
        }

        public virtual void OnWorldIntitialize( World world )
        {
            return;
        }
    }
}
