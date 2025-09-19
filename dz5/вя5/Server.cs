using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static List<TcpClient> clients = new List<TcpClient>();

    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Сервер запущено на порту 5000...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(client);
            Console.WriteLine("Клієнт підключився.");

            Thread t = new Thread(HandleClient);
            t.Start(client);
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int byteCount;

        while ((byteCount = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            string msg = Encoding.UTF8.GetString(buffer, 0, byteCount);
            Console.WriteLine("Отримано: " + msg);
            Broadcast(msg, client);
        }

        Console.WriteLine("Клієнт відключився.");
        clients.Remove(client);
        client.Close();
    }

    static void Broadcast(string msg, TcpClient sender)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg);
        foreach (var c in clients)
        {
            if (c != sender)
            {
                c.GetStream().Write(buffer, 0, buffer.Length);
            }
        }
    }
}
