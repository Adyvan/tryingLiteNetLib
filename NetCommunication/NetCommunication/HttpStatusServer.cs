using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    public class HttpStatusServer
    {
        private HttpListener listener;

        public void Start(int port)
        {
            listener = RunHttp(port);
        }

        public void Stop()
        {
            listener?.Stop();
        }

        private HttpListener RunHttp(int port)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();

            Thread thread = new Thread(() => {
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.Close(Encoding.UTF8.GetBytes("{status:\"ok\"}"), false);
                }
            });

            thread.Start();

            return listener;
        }
    }
}
