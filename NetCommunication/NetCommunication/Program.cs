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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                netServer.Stop();
            }
        }
    }
}

