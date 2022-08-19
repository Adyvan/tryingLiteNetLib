using System;
using LiteNetLib.Utils;

namespace NetPackets
{
    public class PositionReq : INetSerializable
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Direction { get; set; }

        public PositionReq() { }

        public void Deserialize(NetDataReader reader)
        {
            X = reader.GetByte();
            Y = reader.GetByte();
            Direction = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(X);
            writer.Put(Y);
            writer.Put(Direction);
        }
    }

    public class PositionNotification : INetSerializable
    {
        public byte PlayerId { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Direction { get; set; }

        public PositionNotification() { }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetByte();
            X = reader.GetByte();
            Y = reader.GetByte();
            Direction = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(X);
            writer.Put(Y);
            writer.Put(Direction);
        }
    }
}