using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    public class Player
    {
        private static byte Counter { get; set; } = 0;

        public readonly byte Id;
        public readonly NetPeer Peer;

        public string NickName { get; set; }

        public Player(NetPeer peer)
        {
            Id = GetNextId();
            Peer = peer;
        }

        private static byte GetNextId()
        {
            // next Id cannot be equals 0, min value for id == 1 (max is 255)
            if (Counter == byte.MaxValue)
            {
                Counter = byte.MinValue;
            }
            return ++Counter;
        }
    }
}
