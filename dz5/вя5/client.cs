using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static void Main()
    {
        Console.Write("Введіть ваш нік: ");
        string name = Console.ReadLine();

        TcpClient client = new TcpClient("127.0.0.1", 5000);
        NetworkStream stream = client.GetStream();

        Thread receiveThread = new Thread(() =>
        {
            byte[] buffer = new byte[1024];
            int byteCount;
            while ((byteCount = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string msg = Encoding.UTF8.GetString(buffer, 0, byteCount);
                Console.WriteLine(msg);
            }
        });
        receiveThread.Start();

        Console.WriteLine("Ви підключені. Напишіть /exit для виходу.");

        while (true)
        {
            string message = Console.ReadLine();
            if (message.ToLower() == "/exit")
                break;

            byte[] buffer = Encoding.UTF8.GetBytes($"{name}: {message}");
            stream.Write(buffer, 0, buffer.Length);
        }

        client.Close();
    }
}