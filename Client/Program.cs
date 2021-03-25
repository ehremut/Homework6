using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private const int serverPort = 2365;
        private static TcpClient client;
        private static string message;

        static async Task Main(string[] args)
        {
            Console.Write("Enter fullname: ");
            string name = Input();
            Console.Write("Enter email: ");
            string email = Input();
            Console.Write("Enter login: ");
            string login = Input();
            Console.Write("Enter password: ");
            string password = Input();
            message = name + ' ' + email + ' ' + login + ' ' + password;
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, serverPort);
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
            string answer = await reader.ReadLineAsync();
            Console.WriteLine(answer);
            client.Close();
        }

        static private string Input()
        {
            string input = "";
            do
            {
                input = Console.ReadLine();
            } while (input.Length < 0);
            return input;
        }
    }
}