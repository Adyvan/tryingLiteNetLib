using System;
using LiteNetLib.Utils;

namespace NetPackets
{
	public class MessageReq : INetSerializable
	{
		public string Message { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Message);
        }

        public void Deserialize(NetDataReader reader)
        {
            Message = reader.GetString();
        }
    }

    public class MessageNotification : INetSerializable
    {
        public string Message { get; set; }

        public string From { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Message);
            writer.Put(From);
        }

        public void Deserialize(NetDataReader reader)
        {
            Message = reader.GetString();
            From = reader.GetString();
        }
    }
}

