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
        public static string speedModItemNamesListPath = "world/ItemDB/speedMods.json";
        public static string intModItemNamesListPath = "world/ItemDB/intMods.json";
        public static string luckModItemNamesListPath = "world/ItemDB/luckMods.json";

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

            Timer HealofflineUsers_Timer = new Timer(User.HealOfflineUsers, null, 0, (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            Timer ReviveDeadEnemies_Timer = new Timer(state => Enemy.ReviveDeadEnemiesInWorld(world), null, 0, (int)TimeSpan.FromMinutes(1).TotalMilliseconds);         // 1min cooldown to check for dead enemies and revive them, adjust cooldown as needed
            Timer ResetEnemiesUserObj_Timer = new Timer(state => Enemy.ResetEnemyLvlUserObjInWorld(world), null, 0, (int)TimeSpan.FromMinutes(5).TotalMilliseconds);    // 5min cooldown to reset Enemy userObj to match Enemy lvl, this is to prevent enemies from becoming too strong after multiple players are defeated by one enemy, adjust cooldown as needed

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
        /// <summary>
        /// HandleClient is a method that runs in a separate thread for each connected client. It manages the client's connection, handles incoming messages, and processes commands. 
        /// It first prompts the client for a username and checks if it's already in use. If the username is valid, it adds the client to the list of connected clients and loads or 
        /// creates a user profile. The method then enters a loop to read messages from the client, distinguishing between chat messages and commands (prefixed with "!"). Chat messages 
        /// are broadcasted to all other clients, while commands are processed through a command handler. If the client disconnects, it removes the client from the list and broadcasts a 
        /// message about the disconnection.
        /// </summary>
        /// <param name="obj"></param>
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

                    // Validate username - reject if contains illegal filename characters or HTTP headers
                    if (ContainsInvalidCharacters(attemptedName))
                    {
                        SendMessage(client, "Invalid username. Please use alphanumeric characters only.");
                        client.Close();
                        return; // Stop processing this client
                    }

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
                        user = User.LoadUserFromJsonFile(user.Name);
                        Location.AddVisitors(user, world);          // add player to Location Visitors on login
                        user.Status = UserStatus.Idle;              // set user status to idle on login
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
                            SendMessage(client, "Unknown Command");
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
                User.SaveUserToJsonFile(user);
                onlineUserList.Remove(user);
                Location.RemoveVisitors(user, world);
            }

            client.Close();
        }

        //Functions

        /// <summary>
        /// Validates that a username doesn't contain characters illegal in filenames or HTTP protocol indicators
        /// </summary>
        static bool ContainsInvalidCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return true;

            // Reject if contains illegal filename characters
            char[] invalidChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*', '\n', '\r', '\t' };
            if (input.IndexOfAny(invalidChars) >= 0) return true;

            // Reject if contains HTTP protocol strings (WebSocket upgrade attempts)
            if (input.ToUpper().Contains("HTTP") || input.ToUpper().Contains("GET") ||
                input.ToUpper().Contains("HOST") || input.Contains("Upgrade"))
                return true;

            // Reject if too long
            if (input.Length > 32) return true;

            return false;
        }

        /// <summary>
        /// SendMessage is a method that takes in a TcpClient and a message string, converts the message to a byte array, and sends it to the client through the network stream.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
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
        /// <summary>
        /// BroadcastMessage is a method that takes in a message string, converts it to a byte array, and sends it to all connected clients through their respective network streams.
        /// </summary>
        /// <param name="message"></param>
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
        /// <summary>
        /// IsUserOnline is a method that checks if a user with a given username is currently online by iterating through the list of connected clients and comparing their 
        /// associated usernames to the provided username. If a match is found, it returns true, indicating that the user is online; otherwise, it returns false.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool IsUserOnline(string username)
        {
            foreach (string name in clients.Values)
            {
                if (name == username) return true;
            }
            return false;
        }
        /// <summary>
        /// SendPrivateMessage is a method that allows a user to send a private message to another user. It takes in the sender's TcpClient, the sender's username, 
        /// the recipient's username, and the message to be sent. The method iterates through the list of connected clients to find the recipient's TcpClient based on the 
        /// provided username. If the recipient is found, it sends the private message to that client in a specific format indicating that it's a private message from the sender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="fromUser"></param>
        /// <param name="toUser"></param>
        /// <param name="message"></param>
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
        /// <summary>
        /// LoadWorld is a method that checks for the existence of a "world" directory, creates it if it doesn't exist, and then loads all location files from 
        /// the directory into the world list. Each location file is expected to be in JSON format, and the method uses the Location class's LoadFromJsonFile 
        /// method to create Location objects from the files. The names of the loaded locations are printed to the console.
        /// </summary>
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
        /// <summary>
        /// FindLocation is a method that takes in a location name as a string and searches through the world list to find a Location object with a matching name. 
        /// If a matching Location is found, it is returned; otherwise, the method returns null. This method
        /// </summary>
        /// <param name="locName"></param>
        /// <returns></returns>
        public static Location FindLocation(string locName) // used to find a Location in the Location-List by name
        {
            return world.Find(location => location.Name == locName);
        }
        /// <summary>
        /// FindOnlineUser is a method that takes in a username as a string and searches through the onlineUserList to find a User object with a matching name. 
        /// If a matching User is found, it is returned; otherwise, the method returns null. This method is used to look up online users by their username.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static User FindOnlineUser(string userName) // used to find a Location in the Location-List by name
        {
            return onlineUserList.Find(user => user.Name == userName);
        }
    }
}