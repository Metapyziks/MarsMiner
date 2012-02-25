using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarsMiner.Server.Networking
{
    public struct ServerBuilder
    {
        public String Name;
        public String Password;

        public int SlotCount;

        public ServerBuilder( String name = "MarsMiner Server",
            String password = null, int slotCount = 16 )
        {
            Name = name;
            Password = password;
            SlotCount = slotCount;
        }
    }
}
