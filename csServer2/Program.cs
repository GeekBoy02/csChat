using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace SocketServer
{
    public static class Program
    {
        //Init Server Socket
        static Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();
        const int port = 8888; // Server port
        const int bufferSize = 1024; // Buffer size
        public static List<User> onlineUserList = new List<User>();

        //Init World???
        public static List<Location> world = new List<Location>()
        {
            //new Location().CryoStation()
            //new Location().LandingBay()
        };

        //Main
        static void Main(string[] args)
        {
            //Init World
            LoadWorld();
            //___________
            Console.OutputEncoding = Encoding.UTF8;
            // configure shared-library callbacks so that User and Game classes
            // can call back into the server implementation for operations such as
            // sending a message or looking up users/locations.
            ServerCallbacks.SendMessage = SendMessage;
            ServerCallbacks.FindOnlineUser = FindOnlineUser;
            ServerCallbacks.FindLocation = FindLocation;
            Console.WriteLine("Starting server...");
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server started on port " + port);

            Timer HealofflineUsers_Timer = new Timer(User.HealOfflineUsers, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            while (true)
            {
                Console.WriteLine("Waiting for client...");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected");

                //Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                Thread t = new Thread(HandleClient);
                t.Start(client);
            }
        }

        static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;

            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[bufferSize];
            int bytes;

            string username = "";
            User user = new User("placeholder", "placeholder"); // user obj used to keep track of User info in a Json file.
            SendMessage(client, "Enter Username: "); // ask client for their username

            while (true)
            {
                bytes = 0;

                try
                {
                    bytes = stream.Read(buffer, 0, buffer.Length);
                }
                catch
                {
                    break;
                }

                if (bytes == 0)
                {
                    break;
                }

                //Login and User Managment
                string message = Encoding.ASCII.GetString(buffer, 0, bytes).Trim();

                if (string.IsNullOrWhiteSpace(message)) continue;

                if (string.IsNullOrWhiteSpace(username)) // the first message is the clients username
                {
                    //chatGPT change here
                    string attemptedName = message.Trim();

                    // Check if username is already online in either list
                    bool nameInClients = clients.Values.Contains(attemptedName, StringComparer.OrdinalIgnoreCase);
                    bool nameInOnlineUsers = onlineUserList.Any(u =>
                        string.Equals(u.Name, attemptedName, StringComparison.OrdinalIgnoreCase));

                    if (nameInClients || nameInOnlineUsers)
                    {
                        SendMessage(client, "Username already in use. Connection denied.");
                        client.Close();
                        return; // Stop processing this client
                    }

                    username = attemptedName;
                    //chatGPT change end
                    //username = message.Trim();
                    clients.Add(client, username);
                    Console.WriteLine("{0} has joined the chat", username);
                    user = new User(username, client.Client.RemoteEndPoint.ToString());
                    if (File.Exists("users/" + user.Name + ".json"))
                    {
                        user = User.LoadFromJsonFile(user.Name);
                        Location.AddVisitors(user, world);          // add player to Location Visitors on login
                    }
                    else
                    {
                        User.CreateJsonFile(user);   // create user if non existent
                        QuestManager qm = new QuestManager(); // assignt first quest to user and start it if it exists, else it defaluts to a placeholder quest that does nothing
                        Quest q = new Quest().DefaultQuest();
                        if (File.Exists("world/intro.json"))
                        {
                            q = q.LoadFromJsonFile("world/intro.json");
                            qm.StartIntroQuest(q, client, user);
                        }
                        user.ActiveQuest = q;
                    }
                    user.ConnectionCount++;
                    onlineUserList.Add(user);
                    //Location.AddVisitors(user, world);
                    continue;
                }

                // command handler zu lookup! Switch Case is... le bad
                if (message.StartsWith("!"))
                {
                    string[] parts = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string commandKey = parts[0].Substring(1).ToLower(); //remove leading ! from commands
                    string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : new string[0]; // <-- fix das später

                    //Dict Lookup und Parsing
                    if (CommandHandler.Commands.TryGetValue(commandKey, out var handler))
                    {
                        handler(client, user, commandKey, args);
                    }
                    else
                    {
                        //On Lookup Fail try DMs
                        //Maybe scuffed
                        string inboxuser = commandKey;
                        string pmMessage = args.Length > 0 ? string.Join(" ", args) : "";
                        if (!string.IsNullOrWhiteSpace(pmMessage) && IsUserOnline(inboxuser))
                        {
                            SendPrivateMessage(client, user.Name, inboxuser, pmMessage);
                        }
                        else
                        {
                            SendMessage(client, "Unknown Coammnd");
                        }
                    }
                }
                // ____________________________chat function____________________________
                else
                {
                    Console.WriteLine("{0}: {1}", username, message);
                    //User.UpdateUserMessageCount(user);
                    user.MessageCount++;
                    //byte[] msg = Encoding.ASCII.GetBytes(string.Format("{0}: {1}", username, message));
                    byte[] msg = Encoding.ASCII.GetBytes($"{username}: {message} ");

                    foreach (TcpClient c in clients.Keys)
                    {
                        if (c != client)
                        {
                            NetworkStream s = c.GetStream();
                            s.Write(msg, 0, msg.Length);
                        }
                    }
                }
            }

            //DC
            if (!string.IsNullOrWhiteSpace(username))
            {
                clients.Remove(client);
                Console.WriteLine("{0} has left the chat", username);
                BroadcastMessage(username + " has left the chat");
                User.SaveToJsonFile(user);
                onlineUserList.Remove(user);
                Location.RemoveVisitors(user, world);
            }

            client.Close();
        }

        //Functions
        public static void SendMessage(TcpClient client, string message)
        {
            message += " ";
            byte[] msg = Encoding.UTF8.GetBytes(message);
            try
            {
                NetworkStream stream = client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
            catch
            {
                string un = "";
                foreach (string name in clients.Values)
                {
                    if (name == un) un = name;
                }
                Console.WriteLine("could not send msg to " + un);
            }
            Thread.Sleep(100);
        }

        public static void BroadcastMessage(string message)
        {
            message += " ";
            byte[] msg = Encoding.ASCII.GetBytes(message);

            foreach (TcpClient c in clients.Keys)
            {
                try
                {
                    NetworkStream stream = c.GetStream();
                    stream.Write(msg, 0, msg.Length);
                }
                catch { Console.WriteLine("error, message not send"); }
            }
            Thread.Sleep(100);
        }

        public static bool IsUserOnline(string username)
        {
            foreach (string name in clients.Values)
            {
                if (name == username) return true;
            }
            return false;
        }

        public static void SendPrivateMessage(TcpClient sender, string fromUser, string toUser, string message)
        {
            foreach (var pair in clients)
            {
                if (pair.Value.Equals(toUser, StringComparison.OrdinalIgnoreCase))
                {
                    NetworkStream s = pair.Key.GetStream();
                    byte[] msg = Encoding.ASCII.GetBytes($"PM: {fromUser}: {message}");
                    s.Write(msg, 0, msg.Length);
                    break;
                }
            }
        }

        public static void LoadWorld()
        {
            if (!Directory.Exists("world"))
            {
                Console.WriteLine("World folder not found. Creating new world folder.");
                Directory.CreateDirectory("world");
                return;
            }
            string[] folders = Directory.GetDirectories("world");
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                Console.WriteLine(name);
                Location l = Location.LoadFromJsonFile(name);
                world.Add(l);
            }
        }

        public static Location FindLocation(string locName) // used to find a Location in the Location-List by name
        {
            return world.Find(location => location.Name == locName);
        }

        public static User FindOnlineUser(string userName) // used to find a Location in the Location-List by name
        {
            return onlineUserList.Find(user => user.Name == userName);
        }
    }
}