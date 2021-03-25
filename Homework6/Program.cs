using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Blog;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using HW5.DB;


namespace Homework6
{
    class Program
    {
        private static TcpListener listener;
        private const int serverPort = 2365;
        private static bool run;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Server start");
            using (Context context = new Context())
                context.Database.EnsureCreated();
            listener = new TcpListener(IPAddress.Any, serverPort);
            run = true;
            await Listen();
        }
        
        private static async Task Listen()
        {
            List<Task> registerTasks = new List<Task>();
            listener.Start();
            while (run)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                registerTasks.Add(RegisterClient(client));
                registerTasks.RemoveAll(t => t.IsCompleted);
            }
            listener.Stop();
            foreach (Task task in registerTasks)
                await task;
        }
        
        private static async Task RegisterClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            string message = await reader.ReadLineAsync();
            string[] listMessage = message.Split(" ");
            string name = listMessage[0];
            string email = listMessage[1];
            string login = listMessage[2];
            string passwordHash = GetHash(listMessage[3]);
            if (CheckInput(name) && CheckInput(email) && CheckInput(login) && CheckInput(passwordHash))
            {
                using (Context context = new Context())
                {
                    IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
                    try
                    {
                        if (IsValidEmail(email))
                        {
                            try
                            {

                                User newUser = new User()
                                {
                                    Name = name,
                                    Email = email,
                                    Login = login,
                                    PasswordHash = passwordHash
                                };
                                context.Users.Add(newUser);
                                await transaction.CommitAsync();
                                await context.SaveChangesAsync();
                                await writer.WriteLineAsync("success");
                            }
                            catch
                            {
                                await writer.WriteLineAsync("failed");
                            }

                        }
                        else
                        {
                            await writer.WriteLineAsync("failed");
                        }
                    }
                    catch
                    {
                        await writer.WriteLineAsync("failed");
                    }

                    await writer.FlushAsync();
                }
            }
            client.Close();
        }
        static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i=0;i < arrInput.Length -1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
        public static bool IsValidEmail(string source)
        {
            return new EmailAddressAttribute().IsValid(source);
        }

        static string GetHash(string password)
        {
            byte[] source;
            byte[] hashPass;
            source = ASCIIEncoding.ASCII.GetBytes(password);
            hashPass = new MD5CryptoServiceProvider().ComputeHash(source);
            return ByteArrayToString(hashPass);
        }
        
        static private bool CheckInput(string input)
        {
            if (input.Length > 0)
            {
                return true;
            }
            return false;
        }
    }
}