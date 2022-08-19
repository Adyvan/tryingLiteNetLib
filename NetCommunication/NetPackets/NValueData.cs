using System;
using LiteNetLib.Utils;

namespace NetPackets
{
    public class NVector2Byte : INetSerializable
    {
        public byte X { get; set; }
        public byte Y { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            X = reader.GetByte();
            Y = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(X);
            writer.Put(Y);
        }
    }

    public class NColor : INetSerializable
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            R = reader.GetByte();
            G = reader.GetByte();
            B = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(R);
            writer.Put(G);
            writer.Put(B);
        }
    }
}