using LiteNetLib;
using LiteNetLib.Utils;
using NetPackets;
using NetPackets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    internal class NetServer
    {
        public const int MaxPlayers = 10;
        public const int TickTime = 50; //ms

        public readonly string ConnectionKey;
        public readonly int Port;

        public readonly ByteNetPacketProcessor netPacketProcessor;

        private EventBasedNetListener listener;
        private NetManager server;
        private List<Player> players;

        public NetServer(int port, string connectionKey)
        {
            Port = port;
            netPacketProcessor = new ByteNetPacketProcessor();
            netPacketProcessor.RegisterAllNetSerializable();
            ConnectionKey = connectionKey;
        }

        public void Start()
        {
            players = new List<Player>();
            listener = new EventBasedNetListener();
            server = new NetManager(listener);

            server.DisconnectTimeout = int.MaxValue;

            server.Start(Port);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < MaxPlayers)
                    request.AcceptIfKey(ConnectionKey);
                else
                    request.Reject();
            };

            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.NetworkLatencyUpdateEvent += Listener_NetworkLatencyUpdateEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.NetworkErrorEvent += Listener_NetworkErrorEvent;
            listener.NetworkReceiveUnconnectedEvent += Listener_NetworkReceiveUnconnectedEvent;
            listener.DeliveryEvent += Listener_DeliveryEvent;
            listener.NtpResponseEvent += Listener_NtpResponseEvent;

            MonitoringPlayers();
        }

        public void ServerTick()
        {
            server.PollEvents();
        }

        public void Stop()
        {
            server.DisconnectAll();
            server.Stop();
        }

        public void SendMessageTo(Player target, INetSerializable message)
        {
            netPacketProcessor.SendNetSerializable(target.Peer, message, DeliveryMethod.ReliableOrdered);
        }

        public void SendMessageToAll<T>(T message) where T : INetSerializable
        {
            netPacketProcessor.SendNetSerializable(server, message, DeliveryMethod.ReliableOrdered);
        }

        public void CreateNetCommunicationChain<TRequest, TUserData, TResponce>(
            Func<TRequest, TUserData, TResponce> onReceiveAndAnswer,
            DeliveryMethod method,
            Action postAction = null,
            Func<TUserData, NetPeer> toPeer = null)

            where TRequest : INetSerializable, new()
            where TResponce : INetSerializable 
            
        {
            netPacketProcessor.SubscribeNetSerializable<TRequest, TUserData>((req, user) =>
            {
                TResponce res = onReceiveAndAnswer.Invoke(req, user);
                NetPeer peer = toPeer == null ? user as NetPeer : toPeer(user);
                netPacketProcessor.SendNetSerializable(peer, res, method);
                postAction?.Invoke();
            });
        }

        public Player GetPlayerData(NetPeer netPeer)
        {
            Player userData = players.FirstOrDefault(x => x.Peer == netPeer);

            return userData;
        }

        private void MonitoringPlayers()
        {
            //Creating players
            CreateNetCommunicationChain<PlayerReq, NetPeer, PlayerRes>((req, peer) =>
            {
                var player = new Player(peer)
                {
                    NickName = req.NickName,
                };

                players.Add(player);

                return new PlayerRes()
                {
                    Id = player.Id,
                };
            },
            DeliveryMethod.ReliableUnordered,
               PlayersStateChange
            );
        }

        private void PlayersStateChange()
        {
            var data = new AllPlayersNotification()
            {
                Players = players.Select(x => new PlayerData { Id = x.Id, NickName = x.NickName }).ToArray(),
            };

            netPacketProcessor.SendNetSerializable(server, data, DeliveryMethod.ReliableUnordered);
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
