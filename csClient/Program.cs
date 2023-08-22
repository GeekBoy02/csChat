using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketChatClient
{
    class Program
    {
        const int port = 8888; // Server port
        const int bufferSize = 1024; // Buffer size
        static TcpClient client = new TcpClient("127.0.0.1", port); // TCP client
        static NetworkStream stream = client.GetStream(); // Network stream

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Connected to the chat server.");
            //Console.WriteLine("Enter Username: ");
            Thread receiveThread = new Thread(ReceiveMessages); // Thread to receive messages
            receiveThread.Start();
            SendMessages(); // Send messages in main thread
            client.Close(); // Close client
            Console.WriteLine("Disconnected from the chat server.");
        }

        static void ReceiveMessages()
        {
            byte[] buffer = new byte[bufferSize]; // Create buffer
            StringBuilder message = new StringBuilder(); // Create message

            while (true)
            {
                try
                {
                    int bytesRead = 0; // Read from stream until \n
                    do
                    {
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        message.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    }
                    while (stream.DataAvailable);

                    if (message.Length == 0) // Server disconnected
                    {
                        break;
                    }

                    message.Length--; // Remove \n from message

                    Console.WriteLine(message); // Print message

                    message.Clear(); // Clear message for next read
                }
                catch (Exception ex) // Error
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    break;
                }
            }
        }

        static void SendMessages()
        {
            while (true)
            {
                try
                {
                    string input = Console.ReadLine(); // Read input from console

                    if (input == "exit") // Exit command
                    {
                        break;
                    }

                    input += "\n"; // Add \n to input and convert to bytes
                    byte[] buffer = Encoding.UTF8.GetBytes(input);

                    stream.Write(buffer, 0, buffer.Length); // Write and flush buffer
                    stream.Flush();
                }
                catch (Exception ex) // Error
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    break;
                }
            }
        }
    }
}
