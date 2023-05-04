using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;

namespace ServerConsole
{
    internal class TcpClientConnection
    {
        private TcpClient client;
        public IPEndPoint Adress { get; private set; }

        public delegate void ClientDisconnectedDelegate(TcpClientConnection clientConnection);
        public event ClientDisconnectedDelegate ClientDisconnected;

        public TcpClientConnection(TcpClient client)
        {
            this.client = client;
        }

        public async void StartMessaging()
        {
            NetworkStream ns = client.GetStream();
            string[] credentials = null;
            string request = await ReceiveString(ns);
            string[] parameters = request.Split(':');
            if (parameters[0].StartsWith("E")) 
            {
                SQLiteCredentialsChecker.LogOut(parameters[1]);
            }
            else
            {
                credentials = parameters[1].Split(' ');
                int result = SQLiteCredentialsChecker.CheckCredentials(parameters[0], credentials[0], credentials[1]);
                await SendString(ns, result.ToString());
            }
            client.Close();
            ClientDisconnected?.Invoke(this);
        }

        public Task StartMessagingAsync() => Task.Run(StartMessaging);

        private async Task SendString(NetworkStream stream, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch{}
        }

        private async Task<string> ReceiveString(NetworkStream stream)
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];
            try
            {
                bytes = await stream.ReadAsync(data, 0, data.Length);
                do
                {
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                } while (stream.DataAvailable);
            }
            catch{}
            return builder.ToString();
        }
    }
}
