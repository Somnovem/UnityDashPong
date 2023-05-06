using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ServerConsole
{
    internal class TcpServer : IDisposable
    {
        private TcpListener listener;
        private int port;
        private IPAddress address;
        private List<TcpClientConnection> connectedClients;
        private ManualResetEvent clientHolder;

        public delegate void StringMessageDelegate(string message);
        public event StringMessageDelegate StringMessage;

        public TcpServer(IPAddress address, int port,ManualResetEvent clientHolder, string connectionString)
        {
            this.listener = null;
            this.port = port;
            this.address = address;
            connectedClients = null;
            this.clientHolder = clientHolder;
            SQLiteCredentialsChecker.connectionString = connectionString;
        }
        public Task StartListenAsync() => Task.Run(StartListen);
        public async void StartListen()
        {
            if (listener != null)
            {
                StringMessage?.Invoke("Server is already running!");
                return;
            }
            listener = new TcpListener(address, port);
            connectedClients = new List<TcpClientConnection>();
            listener.Start();
            StringMessage?.Invoke("Server started! Accepting connections...");
            TcpClient client;
            while (true)
            {
                try
                {
                    client = await listener.AcceptTcpClientAsync();
                }
                catch (Exception)
                {
                    //listener was disposed
                    break;
                }



                StringMessage?.Invoke($"Accepted: {client.Client.RemoteEndPoint}");
                TcpClientConnection clientConnection = new TcpClientConnection(client);
                connectedClients.Add(clientConnection);
                clientConnection.ClientDisconnected += ClientConnection_ClientDisconnected;
                _ = clientConnection.StartMessagingAsync();
            }
        }
        private void ClientConnection_ClientDisconnected(TcpClientConnection clientConnection)
        {
            connectedClients.Remove(clientConnection);
        }

        public void StopListening()
        {
            if (listener != null)
            {
                if (connectedClients != null) connectedClients.Clear();
                listener.Stop();
                listener = null;
                StringMessage?.Invoke("Server stopped!");
                clientHolder.Set();
            }
        }
        public void Dispose()
        {
            this.StopListening();
        }
    }
}
