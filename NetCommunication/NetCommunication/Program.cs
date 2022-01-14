using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;

namespace ServerApp
{
    public class App
    {
        const int Port_Docker = 9050;
        const int Port_Test = 9051;
        public class TargetPeer
        {
            public bool All { get; set; }
            public NetPeer targetPeer { get; set; }
            public NetPeer sourcePeer { get; set; }

            public static TargetPeer ToAll(NetPeer source)
            {
                return new TargetPeer() { All = true, sourcePeer = source };
            }
        }
        public static Queue<Tuple<string, TargetPeer>> messages = new Queue<Tuple<string, TargetPeer>>();

        public static TcpListener Listener { get; private set; }

        public static void Main(params string[] input)
        {
            //for health check
            //using var http = RunHttp(8080);

            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);
            int port = OperatingSystem.IsWindows() ? Port_Test : Port_Docker;
            server.Start( port /* port */);

            Console.WriteLine($"server start port:{port}");

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < 10 /* max connections */)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("PeerConnected: {0}", peer.EndPoint); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);          // Send with reliability
            };

            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;

            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.NetworkLatencyUpdateEvent += Listener_NetworkLatencyUpdateEvent;

            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.NetworkErrorEvent += Listener_NetworkErrorEvent;
            listener.NetworkReceiveUnconnectedEvent += Listener_NetworkReceiveUnconnectedEvent;
            listener.DeliveryEvent += Listener_DeliveryEvent;
            listener.NtpResponseEvent += Listener_NtpResponseEvent;


            while (true)
            {
                server.PollEvents();
                SendDataIfNeeded(server);
                Thread.Sleep(15);
                
            }
            //http.Stop();
            server.Stop();
        }

        private static void Listener_NtpResponseEvent(NtpPacket packet)
        {
            Console.WriteLine($"NtpResponse {packet}");
        }

        private static void Listener_DeliveryEvent(NetPeer peer, object userData)
        {
            Console.WriteLine($"Delivery {peer}:{userData}");
        }

        private static void Listener_NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            var mess = reader.GetString(100);
            reader.Recycle();

            Console.WriteLine($"NetworkError {remoteEndPoint}:'{mess}':{messageType}");
        }

        private static void Listener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine($"NetworkError {endPoint}:{socketError}");
        }

        private static void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Console.WriteLine($"PeerConnected {peer}");
        }

        private static void Listener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            Console.WriteLine($"LatencyUpdate {peer}: ({latency} ms)");
        }

        private static void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"Disconnected {peer}");
        }

        public static void SendDataIfNeeded(NetManager server)
        {
            Tuple<string, TargetPeer> mess = null;
            while (messages.TryDequeue(out mess))
            {
                var (data, target) = mess;
                if(target.All)
                {
                    foreach(var peer in server.ConnectedPeerList)
                    {
                        if(peer.ConnectionState == ConnectionState.Connected && !peer.Equals(target.sourcePeer))
                        {
                            SendDataToPeer(peer, data);
                        }
                    }
                }
            }
        }

        private static void SendDataToPeer(NetPeer peer, string data)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(data);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        private static void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var mess = reader.GetString(100);
            messages.Enqueue(new Tuple<string, TargetPeer>(mess, TargetPeer.ToAll(peer)));
            Console.WriteLine($"We got mess from {peer.EndPoint}:'{mess}'  ({deliveryMethod})");
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

