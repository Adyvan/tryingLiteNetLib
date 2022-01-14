
using LiteNetLib;
using LiteNetLib.Utils;

namespace ClientApp
{
    public class App
    {
        const string Server = "localhost";
        //const string Server = "207.154.230.172";//DO

        //const int Port = 9050; //Docker and DO
        const int Port = 9051; // Local run
        public static void Main(params string[] input)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager client = new NetManager(listener);
            client.Start();
            var peer = client.Connect(Server /* host ip or name */, Port /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                Console.WriteLine("We got: {0}", dataReader.GetString(100 /* max length of string */));
                dataReader.Recycle();
            };

            string mess = null;

            Thread thread = new Thread(() => {
                while (!"quit".Equals(mess) || peer.ConnectionState == ConnectionState.Connected)
                {
                    mess = Console.ReadLine();
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(mess);
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
            });

            thread.Start();

            while (!"quit".Equals(mess) || peer.ConnectionState == ConnectionState.Connected)
            {
                client.PollEvents();
                Thread.Sleep(15);
            }

            client.Stop();
        }
    }

}

