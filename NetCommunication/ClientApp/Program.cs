
using LiteNetLib;
using LiteNetLib.Utils;
using NetPackets;

namespace ClientApp
{
    public class App
    {
        const string Server = "localhost";
        //const string Server = "207.154.230.172";//DO

        const int Port = 9050; //Docker and DO
        //const int Port = 9051; // Local run

        public static string PlayerName { get; set; }
        public static byte PlayerId { get; set; }

        public static void Main(params string[] input)
        {
            Console.WriteLine($"{Server}:{Port} <<<<<< to");
            Console.Write("Enter name: ");
            PlayerName = Console.ReadLine();

            var client = new NetClient(Server, Port, "SomeConnectionKey");

            client.SubscribeNetSerializable<MessageNotification>(mess =>
            {
                Console.WriteLine($"{mess.From}: {mess.Message}");
            });

            client.Start(PlayerName);

            

            string mess = null;

            var messageReq = new MessageReq();

            Thread thread = new Thread(() => {
                while (!"quit".Equals(mess))
                {
                    mess = Console.ReadLine();
                    if (!"quit".Equals(mess))
                    {
                        messageReq.Message = mess;
                        client.SendMessage(messageReq);
                    }
                }
            });

            thread.Start();

            while (!"quit".Equals(mess) )
            {
                
                client.Update();
                Thread.Sleep(15);
            }

            client.Stop();
        }
    }
}

