using LiteNetLib.Utils;

namespace NetPackets
{
    public class PlayerReq : INetSerializable
    {
        public string NickName { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            NickName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NickName);
        }
    }

    public class PlayerRes : INetSerializable
    {
        public byte Id { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
        }
    }

    public class AllPlayersNotification : INetSerializable
    {
        public PlayerData[] Players { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            var count = reader.GetByte();
            Players = new PlayerData[count];
            for (int i = 0; i < count; i++)
            {
                Players[i] = new PlayerData();
                Players[i].Deserialize(reader);
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Players.Length);
            foreach (var p in Players)
            {
                p.Serialize(writer);
            }
        }
    }

    public class PlayerData : INetSerializable
    {
        public byte Id { get; set; }
        public string NickName { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            NickName = reader.GetString();

            Console.WriteLine($"Get name {NickName}");
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(NickName);

            Console.WriteLine($"Put name {NickName}");
        }
    }
}