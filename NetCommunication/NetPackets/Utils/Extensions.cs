using System;
using System.Linq;
using LiteNetLib.Utils;

namespace NetPackets.Utils
{
	public static class Extensions
	{
		public static void RegisterAllNetSerializable(this ByteNetPacketProcessor byteNetPacketProcessor)
        {
			var type = typeof(INetSerializable);
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => p.IsClass && type.IsAssignableFrom(p))
				.OrderBy(p => p.FullName)
				.ToArray();
			foreach(var t in types)
            {
				byteNetPacketProcessor.RegisterType(t);
            }
		}

		public static (byte length, byte[] data) ToSerealizeToBytes(this bool[] bools)
        {
			byte length = (byte) ((bools.Length / 8) // one byte has 8 bit
				+ (bools.Length % 8 == 0 ? 0 : 1)); //if need additional byte for bools			

			byte[] res = new byte[length];

			for(byte t = 0; t <= bools.Length; t++)
            {
				res[t / 8] |= (byte) (0x01 << t % 8);
            }
			return (length, res);
        }

		public static bool[] ToSerealizeToBools(this byte[] bools, byte length)
		{
			bool[] res = new bool[length];

			for (byte t = 0; t <= length; t++)
			{
				res[t] = (bools[t / 8] & (0x01 << t % 8)) > 0;
			}
			return res;
		}
	}
}

