using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace ServerConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=..\..\auth.db;Version=3;";
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            TcpServer mainServer = new TcpServer(IPAddress.Parse("127.0.0.1"),8001,manualResetEvent,connectionString);
            mainServer.StringMessage += MainServer_StringMessage;
            mainServer.StartListenAsync();
            manualResetEvent.WaitOne();
        }

        private static void MainServer_StringMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
