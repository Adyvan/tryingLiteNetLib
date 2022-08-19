using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib.Utils;
using NetPackets.Utils;

namespace NetPackets
{
    public class StartGameReq : INetSerializable
    {
        public DificultLevel Dificult { get; set; }
        
        public StartGameReq()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Dificult);
        }

        public void Deserialize(NetDataReader reader)
        {
            Dificult = (DificultLevel)reader.GetByte();
        }
    }

    public class StartGameRes : INetSerializable
    {
        public DificultLevel Dificult { get; set; }

        public uint RoomId { get; set; }

        public DateTime EndWaitingTime { get; set; }

        public StartGameRes()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Dificult);
            writer.Put(RoomId);
            writer.Put((int)EndWaitingTime.TimeOfDay.TotalMilliseconds);
        }

        public void Deserialize(NetDataReader reader)
        {
            Dificult = (DificultLevel) reader.GetByte();
            RoomId = reader.GetUInt();
            var totalMilliseconds = reader.GetInt();
            EndWaitingTime = DateTime.UtcNow.Date.AddMilliseconds(totalMilliseconds);
        }
    }

    public class StartGameNotification : INetSerializable
    {
        public uint RoomId { get; set; }

        /// <summary>
        /// Max Length (255)
        /// </summary>
        public PlayerStartData[] Players { get; set; }

        public StartGameNotification()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(RoomId);
            writer.Put((byte)Players.Length);

            var data = Players.Select(x => x.Accept).ToArray().ToSerealizeToBytes();
            writer.Put(data.length);
            writer.Put(data.data);

            foreach(var p in Players)
            {
                writer.Put(p);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            RoomId = reader.GetUInt();
            var playersLength = reader.GetByte();

            var arrBoolLength = reader.GetByte();
            var bytes = new byte[arrBoolLength];
            reader.GetBytes(bytes, arrBoolLength);
            var bools = bytes.ToSerealizeToBools(playersLength);

            Players = new PlayerStartData[playersLength];
            for (byte t = 0; t < Players.Length; t++)
            {
                Players[t] = reader.Get<PlayerStartData>();
                Players[t].Accept = bools[t];
            }
        }
    }

    public class PlayerStartData : INetSerializable
    {
        public byte Id { get; set; }
        public bool Accept { get; set; }
        public NColor Color { get; set; }
        public NVector2Byte Position { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(Color);
            writer.Put(Position);
        }
        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Color = reader.Get<NColor>();
            Position = reader.Get<NVector2Byte>();
        }
    }

    [Flags]
    public enum DificultLevel : byte
    {
        None = 0,
        Easy = 1,
        Normal = 2,
        Hard = 4,
        VeryHard = 8,
    }

    public class MapLab : INetSerializable
    {
        [Flags]
        public enum TileWall
        {
            None = 0,
            Up = 1,
            Right = 2,
        }

        public NVector2Byte Size { get; set; }

        public TileWall[] Tiles { get; set; }

        public MapLab()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            Size.Serialize(writer);
            writer.Put(this.TilesArr);
        }

        public void Deserialize(NetDataReader reader)
        {
            Size = new NVector2Byte();
            Size.Deserialize(reader);
            Tiles = new TileWall[Size.X * Size.Y];

            var bytes = new byte[GetArrayLength(Tiles.Length)];
            reader.GetBytes(bytes, bytes.Length);
            TilesArr = bytes;
        }

        private byte[] TilesArr
        {
            get
            {

                var val = new byte[GetArrayLength(Tiles.Length)];                
                for(byte t = 0; t < Tiles.Length; t++)
                {
                    val[t / 4] |= (byte) (((byte)Tiles[t]) << t % 4);
                }
                return val;
            }
            set
            {
                for(byte t = 0; t < Tiles.Length; t++)
                {
                    Tiles[t] = (TileWall)((byte)((value[t / 4] >> t % 4) & 0x03)); // x & 0x03 (0000 0011) => last 2 bits
                }
            }
        }

        private static int GetArrayLength(int tilesLength)
        {
            var length = tilesLength / 4;
            if (tilesLength % 4 > 0)
            {
                length++;
            }
            return length;
        }
    }
}
