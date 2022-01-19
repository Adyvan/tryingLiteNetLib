using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using NetPackets;

namespace ServerApp
{
    public class App
    {
        const int Port_Docker = 9050;
        const int Port_Test = 9051;

        public static void Main(params string[] input)
        {
            int port = OperatingSystem.IsWindows() ? Port_Test : Port_Docker;

            NetServer netServer = new NetServer(port, "SomeConnectionKey");

            MessageNotification messageNotification = new MessageNotification();
            netServer.netPacketProcessor.SubscribeNetSerializable<MessageReq, NetPeer>((mess, peer) =>
            {
                var player = netServer.GetPlayerData(peer);
                Console.WriteLine($"Mess: {player.NickName}: {mess.Message}");
                messageNotification.Message = mess.Message;
                messageNotification.From = player.NickName;
                netServer.SendMessageToAll(messageNotification);
            });

            netServer.Start();

            Console.WriteLine($"server start port:{port}");

            try
            {
                while (true)
                {
                    netServer.ServerTick();
                    Thread.Sleep(15);
                }
            }
            finally
            {
                netServer.Stop();
            }
        }
    }
}

