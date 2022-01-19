using System;
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
	}
}

