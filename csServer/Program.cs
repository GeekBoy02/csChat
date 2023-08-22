using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent; // Import concurrent namespace
using System.Text.Json;

namespace SocketChatServer
{
    class Program
    {
        const int port = 8888; // Server port
        const int bufferSize = 1024; // Buffer size
        static TcpListener listener = new TcpListener(IPAddress.Any, port); // TCP listener
        static ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>(); // Connected clients
        static List<string> connected_users = new List<string>();

        static void Main(string[] args)
        {
            listener.Start(); // Start listener
            Console.WriteLine("Server started on port {0}", port);
            Thread acceptThread = new Thread(AcceptClients); // Thread to accept clients
            acceptThread.Start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            listener.Stop(); // Stop listener
            foreach (var client in clients) // Close all clients
            {
                client.Close();
            }
        }

        static void AcceptClients()
        {
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient(); // Accept client
                    clients.Add(client); // Add to list
                    Console.WriteLine("New client connected: {0}", client.Client.RemoteEndPoint);
                    SendMessage(client, "Welcome to the chat server, enter your Username: "); // Send welcome message
                    Thread receiveThread = new Thread(() => ReceiveMessages(client)); // Thread to receive messages
                    receiveThread.Start();
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.Interrupted) // Listener stopped
                    {
                        break;
                    }
                    else // Socket error
                    {
                        Console.WriteLine("Socket error: {0}", ex.Message);
                    }
                }
                catch (Exception ex) // Other error
                {
                    Console.WriteLine("Error: {0}", ex.Message);

                }
            }
        }

        static void ReceiveMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream(); // Get stream
            byte[] buffer = new byte[bufferSize]; // Create buffer
            StringBuilder message = new StringBuilder(); // Create message
            bool firstMessage = true; // Check if it is the first message
            string username = ""; // Store the username

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

                    if (message.Length == 0) // Client disconnected
                    {
                        break;
                    }

                    message.Length--; // Remove \n from message

                    string firstChar = "";
                    if (!String.IsNullOrEmpty(message.ToString()))
                    {
                        firstChar = message.ToString().Substring(0, 1);
                    }

                    if (firstMessage) // If it is the first message, store it as the username and set the flag to false
                    {
                        username = message.ToString();
                        firstMessage = false;

                        // Create a user object with the username and the client's remote endpoint
                        User user = new User(username, client.Client.RemoteEndPoint.ToString());
                        // Check if a json file with the username already exists
                        if (File.Exists(username + ".json"))
                        {
                            // If so, deserialize it to a user object
                            user = JsonSerializer.Deserialize<User>(File.ReadAllText(username + ".json"));

                            // Increment its ConnectionCount property by 1
                            user.ConnectionCount++;
                        }
                        // Create a json serializer options object with some settings
                        JsonSerializerOptions options = new JsonSerializerOptions
                        {
                            WriteIndented = true, // Write indented json for readability
                            IgnoreNullValues = true // Ignore null values in the object
                        };
                        // Serialize the user object to json format using the options
                        string json = JsonSerializer.Serialize(user, options);
                        // Write the json string to a file with the username as the file name
                        File.WriteAllText(username + ".json", json);

                        connected_users.Add(username);

                    }
                    else if (firstChar != "!" && !String.IsNullOrEmpty(message.ToString()))// Otherwise, print and broadcast the message with the username
                    {
                        // Deserialize the json file with the username to a user object
                        User user = JsonSerializer.Deserialize<User>(File.ReadAllText(username + ".json"));
                        // Increment its MessageCount property by 1
                        user.MessageCount++;
                        // Create a json serializer options object with some settings
                        JsonSerializerOptions options = new JsonSerializerOptions
                        {
                            WriteIndented = true, // Write indented json for readability
                            IgnoreNullValues = true // Ignore null values in the object
                        };
                        // Serialize the user object to json format using the options
                        string json = JsonSerializer.Serialize(user, options);
                        // Write the json string to a file with the username as the file name
                        File.WriteAllText(username + ".json", json);

                        Console.WriteLine("Message from {0}: {1}", username, message); // Print and broadcast message
                        BroadcastMessage(client, username, message.ToString());
                    }
                    else // Game
                    {
                        if (message.ToString() == "!h")
                        {
                            string help_message = "Available commands:\n" +
                                "!class [class_name] - Sets the user's class to [class_name], which must be one of: Soldier, Engineer, Explorer\n" +
                                "!fight [enemy_level] - Initiates a battle with an enemy of the specified level\n" +
                                "!duel [username] - Initiates a battle with another User if he/she is online\n" +
                                "!allocate attributes [speed] [intellect] [luck] - Increases the user's speed, intellect, and luck attributes by the specified amounts\n" +
                                "!attributes - Displays the user's current attributes\n" +
                                "!users - Displays a list of all connected users";
                            SendMessage(client, help_message);

                        }
                        else if (message.ToString() == "!a")
                        {
                            User user = JsonSerializer.Deserialize<User>(File.ReadAllText(username + ".json"));
                            string UserAttributes = Environment.NewLine +
                                "Class: " + user.Class + Environment.NewLine +
                                "Level: " + user.Level + Environment.NewLine +
                                "XP: " + user.xp + Environment.NewLine +
                                "HP: " + user.hp + Environment.NewLine +
                                "Speed: " + user.speed + Environment.NewLine +
                                "Intellect: " + user.intellect + Environment.NewLine +
                                "Luck: " + user.luck + Environment.NewLine +
                                "Free AP: " + user.freeAP + Environment.NewLine;
                            SendMessage(client, UserAttributes);
                        }
                        else if (message.ToString() == "!aa")
                        {
                            SendMessage(client, "allocate player attributes: blabla");
                        }
                        else if (message.ToString() == "!users")
                        {
                            SendMessage(client, GetConnectedUsers());
                        }
                    }

                    message.Clear(); // Clear message for next read
                }
                catch (Exception ex) // Error
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    break;
                }
            }
            connected_users.Remove(username);
            clients.TryTake(out client); // Remove and close client
            client.Close();

            Console.WriteLine("Client disconnected: {0}", username); // Print message with the username
        }


        static void SendMessage(TcpClient client, string message)
        {
            try
            {
                NetworkStream stream = client.GetStream(); // Get stream

                message += "\n"; // Add \n to message and convert to bytes
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                stream.Write(buffer, 0, buffer.Length); // Write and flush buffer
                stream.Flush();
            }
            catch (Exception ex) // Error
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        static void BroadcastMessage(TcpClient sender, string UserName, string message)
        {
            foreach (var client in clients) // For each client except sender
            {
                if (client != sender)
                {
                    if (!String.IsNullOrEmpty(message))
                    {
                        message = UserName + ": " + message;
                        SendMessage(client, message.ToString()); // Send message
                    }
                }
            }
        }

        static string GetConnectedUsers()
        {
            // Create a StringBuilder object
            StringBuilder sb = new StringBuilder();

            // Append a header line
            sb.AppendLine("Currently connected users:");

            // Loop through each user object in the list
            foreach (string user in connected_users)
            {
                // Append the user properties in a formatted way
                sb.AppendLine(user);
            }

            // Return the StringBuilder object as a string
            return sb.ToString();
        }

    }
}
