using LiteNetLib;
using LiteNetLib.Utils;
using NetPackets;
using NetPackets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class NetClient
    {
        public readonly string ConnectionKey;
        public readonly int Port;
        public readonly string Address;

        public readonly ByteNetPacketProcessor netPacketProcessor;

        public bool NeedToReconect { get; set; }

        private bool isRunning;
        private EventBasedNetListener listener;
        private NetManager client;
        private NetPeer server;
        private PlayerData[] players;
        private PlayerData CurrentPlayer;

        public NetClient(string address, int port, string connectionKey)
        {
            Port = port;
            netPacketProcessor = new ByteNetPacketProcessor();
            netPacketProcessor.RegisterAllNetSerializable();
            ConnectionKey = connectionKey;
            Address = address;
        }

        public void Start(string playerName)
        {
            CurrentPlayer = new PlayerData() { NickName = playerName, };
            listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.DisconnectTimeout = int.MaxValue;
            client.Start();

            server = client.Connect(Address, Port, ConnectionKey);

            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.NetworkLatencyUpdateEvent += Listener_NetworkLatencyUpdateEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.NetworkErrorEvent += Listener_NetworkErrorEvent;
            listener.NetworkReceiveUnconnectedEvent += Listener_NetworkReceiveUnconnectedEvent;
            listener.DeliveryEvent += Listener_DeliveryEvent;
            listener.NtpResponseEvent += Listener_NtpResponseEvent;

            isRunning = true;
        }

        public void Update()
        {
            client.PollEvents();
        }

        public void Stop()
        {
            isRunning = false;
            client.Stop();
        }

        public void SendMessage<T>(T message) where T : INetSerializable
        {
            netPacketProcessor.SendNetSerializable(server, message, DeliveryMethod.ReliableOrdered);
        }

        public void SubscribeNetSerializable<T>(Action<T> action) where T : INetSerializable, new()
        {
            netPacketProcessor.SubscribeNetSerializable<T>(action);
        }

        private void MonitoringPlayers()
        {
            netPacketProcessor.SubscribeNetSerializable<PlayerRes>(res =>
            {
                CurrentPlayer.Id = res.Id;
                Console.WriteLine($"CurrentPlayer {CurrentPlayer.Id}:{CurrentPlayer.NickName}");
            });

            netPacketProcessor.SubscribeNetSerializable<AllPlayersNotification>(res =>
            {
                players = res.Players;
                Console.WriteLine(string.Join(',', players.Select(x => $"{{{x.Id},{x.NickName}}}")));
            });
        }

        private void TryToReconnect()
        {
            if (server.ConnectionState == ConnectionState.Disconnected)
            {
                Console.WriteLine("TryToReconnect");
                server = client.Connect(Address, Port, ConnectionKey);
            }
            else
            {
                Console.WriteLine("NotReconnect");
            }
        }

        #region Subscribes

        private void Listener_NtpResponseEvent(NtpPacket packet)
        {
            Console.WriteLine($"NtpResponse {packet}");
        }

        private void Listener_DeliveryEvent(NetPeer peer, object userData)
        {
            Console.WriteLine($"Delivery {peer}:{userData}");
        }

        private void Listener_NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            var mess = reader.GetString(100);
            reader.Recycle();

            Console.WriteLine($"NetworkReceiveUnconnected {remoteEndPoint}:'{mess}':{messageType}");
        }

        private void Listener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine($"NetworkError {endPoint}:{socketError}");
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Console.WriteLine($"PeerConnected {peer}");

            MonitoringPlayers();

            netPacketProcessor.SendNetSerializable(
                server,
                new PlayerReq { NickName = CurrentPlayer.NickName },
                DeliveryMethod.ReliableUnordered);
        }

        private void Listener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            //Console.WriteLine($"LatencyUpdate {peer}: ({latency} ms)");
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"Disconnected {peer}");
            TryToReconnect();
        }

        #endregion
    }
}