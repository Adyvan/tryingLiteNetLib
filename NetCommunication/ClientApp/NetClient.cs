using LiteNetLib;
using LiteNetLib.Utils;
using NetPackets;
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

        public readonly NetPacketProcessor netPacketProcessor;

        private EventBasedNetListener listener;
        private NetManager client;
        private NetPeer server;
        private PlayerData[] players;
        private PlayerData CurrentPlayer;

        public NetClient(string address, int port, string connectionKey)
        {
            Port = port;
            netPacketProcessor = new NetPacketProcessor();
            ConnectionKey = connectionKey;
            Address = address;
        }

        public void Start(string playerName)
        {
            CurrentPlayer = new PlayerData() { NickName = playerName, };
            listener = new EventBasedNetListener();
            client = new NetManager(listener);
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

            MonitoringPlayers();

            netPacketProcessor.SendNetSerializable(
                server, 
                new PlayerReq { NickName = playerName }, 
                DeliveryMethod.ReliableUnordered);
        }

        public void Update()
        {
            client.PollEvents();
        }

        public void Stop()
        {
            client.Stop();
        }

        public void SendMessage(INetSerializable message)
        {
            netPacketProcessor.SendNetSerializable(server, message, DeliveryMethod.ReliableOrdered);
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
        }

        private void Listener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            Console.WriteLine($"LatencyUpdate {peer}: ({latency} ms)");
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"Disconnected {peer}");
        }

        #endregion
    }
}