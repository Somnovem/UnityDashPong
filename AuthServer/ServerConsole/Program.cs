using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;
namespace ServerConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string pathToDatabase = "..\\..\\auth.db";
            if (!File.Exists(pathToDatabase)) 
            {
                Console.WriteLine("No database file found");
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
                Environment.Exit(-1);
            }
            string connectionString = $"Data Source={pathToDatabase};Version=3;";
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            TcpServer mainServer = new TcpServer(IPAddress.Any, 8001, manualResetEvent,pathToDatabase,connectionString);
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
