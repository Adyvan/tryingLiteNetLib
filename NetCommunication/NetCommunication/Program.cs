
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;

namespace ServerApp
{
    public class App
    {
        public static TcpListener Listener { get; private set; }

        public static void Main(params string[] input)
        {
            using var http = RunHttp(8080);

            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);
            server.Start(9050 /* port */);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < 10 /* max connections */)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
            };

            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;

            while (!Console.KeyAvailable)
            {
                server.PollEvents();
                Thread.Sleep(15);
            }
            http.Stop();
            server.Stop();
        }

        private static void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine($"We got mess from {peer.EndPoint}:'{reader.GetString(100)}'  ({deliveryMethod})");
            reader.Recycle();
        }

        private static HttpListener RunHttp(int port)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();


            Thread thread = new Thread(() => {
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    context.Response.StatusCode = (int) HttpStatusCode.OK;
                    context.Response.Close(System.Text.Encoding.UTF8.GetBytes("{status:\"ok\"}"),false);
                }
            });

            thread.Start();

            return listener;
        }
    }

}

