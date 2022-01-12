
using LiteNetLib;
using LiteNetLib.Utils;

namespace ClientApp
{
    public class App
    {
        public static void Main(params string[] input)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager client = new NetManager(listener);
            client.Start();
            var peer = client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                Console.WriteLine("We got: {0}", dataReader.GetString(100 /* max length of string */));
                dataReader.Recycle();
            };

            string mess = null;

            Thread thread = new Thread(() => {
                while (!"quit".Equals(mess))
                {
                    mess = Console.ReadLine();
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(mess);
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
            });

            thread.Start();

            while (!"quit".Equals(mess))
            {
                client.PollEvents();
                Thread.Sleep(15);
            }

            client.Stop();
        }
    }

}

