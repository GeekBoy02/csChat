﻿using System;
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
    class Program
    {
        static Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();
        const int port = 8888; // Server port
        const int bufferSize = 1024; // Buffer size

        public static List<User> onlineUserList = new List<User>();
        public static User FindOnlineUser(List<User> userList, string userName)
        {
            return userList.Find(user => user.Name == userName);
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
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

                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
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

                string message = Encoding.ASCII.GetString(buffer, 0, bytes);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (string.IsNullOrWhiteSpace(username)) // the first message is the clients username
                    {
                        username = message.Trim();
                        clients.Add(client, username);
                        Console.WriteLine("{0} has joined the chat", username);
                        user = new User(username, client.Client.RemoteEndPoint.ToString());
                        if (File.Exists("users/" + user.Name + ".json"))
                        {
                            user = User.LoadFromJsonFile(user.Name);
                        }
                        else
                        {
                            User.CreateJsonFile(user);
                        }
                        user.ConnectionCount++;
                        onlineUserList.Add(user);
                    }
                    else if (message.StartsWith("!users"))  // display users online
                    {
                        StringBuilder sb = new StringBuilder();

                        // foreach (string name in clients.Values)
                        // {
                        //     sb.Append(name);
                        //     sb.Append(", ");
                        // }
                        // string userlist = sb.ToString().TrimEnd();
                        // userlist.TrimEnd();

                        foreach (User u in onlineUserList)
                        {
                            sb.Append("< " + u.Name + " > Level: " + u.Level + " hp: " + u.Hp + " \n");
                        }

                        string userlist = sb.ToString();

                        SendMessage(client, userlist);
                    }
                    else if (message.StartsWith("!revive")) // revive
                    {
                        if (user.Hp == 0)
                        {
                            user.HealUser(client, 100, false);
                            user.Credits -= user.Credits / 4;
                            SendMessage(client, $"You got revived for {user.Credits / 4}📀 and now have {user.Hp} HP \n");
                        }
                        else
                        {
                            SendMessage(client, "You can only be revevied when you are DEAD \n");
                        }
                    }
                    else if (message.StartsWith("!i")) // display and use inventory
                    {
                        string[] parts = message.Split(' ', 2);
                        StringBuilder sb = new StringBuilder();
                        if (message.StartsWith("!inventory"))
                        {
                            foreach (Item i in user.Inventory)
                            {
                                sb.Append(i.Icon + "-" + i.Name);
                                sb.Append(", ");
                            }
                        }
                        else if (message.StartsWith("!ir"))
                        {
                            if (parts.Length > 1)
                            {
                                string itemName = message.Split()[1];
                                Item i = user.FindItemInInventory(itemName);
                                if (i != null) user.RemoveItemFromInventory(client, i);
                            }
                        }
                        else if (message.StartsWith("!ii"))
                        {
                            if (parts.Length > 1)
                            {
                                string itemName = message.Split()[1];
                                user.InspectItem(client, itemName);
                            }
                        }
                        else if (message.StartsWith("!isa"))
                        {
                            if (parts.Length > 1)
                            {
                                string itemName = message.Split()[1];
                                user.SellAllofItem(client, itemName);
                            }
                        }
                        else if (message.StartsWith("!is"))
                        {
                            if (parts.Length > 1)
                            {
                                string itemName = message.Split()[1];
                                user.SellItem(client, itemName, true);
                            }
                        }
                        else
                        {
                            if (parts.Length > 1)
                            {
                                string itemName = message.Split()[1];
                                Item.UseItem(client, user, user.FindItemInInventory(itemName), true);
                            }
                            else
                            {
                                foreach (Item i in user.Inventory)
                                {
                                    sb.Append(i.Icon);
                                    sb.Append(",");
                                }
                                sb.Append(" ");
                            }

                        }

                        string itemList = sb.ToString().TrimEnd().TrimEnd();
                        SendMessage(client, itemList);
                    }
                    else if (message.StartsWith("!class")) // duel another user
                    {
                        string[] parts = message.Split(' ', 2);

                        if (parts.Length > 1)
                        {
                            string user_class = message.Split()[1].ToLower();
                            if (new[] { "soldier", "engineer", "explorer" }.Contains(user_class))
                            {
                                if (user_class == "soldier") user.ChangeTo_Soldier();
                                else if (user_class == "engineer") user.ChangeTo_Engineer();
                                else if (user_class == "explorer") user.ChangeTo_Explorer();
                                SendMessage(client, $"You are now a {user_class}!");
                                User.SaveToJsonFile(user);
                            }
                            else
                            {
                                SendMessage(client, "Invalid class. Valid classes are Soldier, Engineer and Explorer.");
                            }

                        }
                    }
                    else if (message.StartsWith("!d") || message.StartsWith("!duel")) // duel another user
                    {
                        string[] parts = message.Split(' ', 2);

                        if (parts.Length > 1)
                        {
                            string opponentName = parts[1].TrimEnd('\n', '\r');
                            if (IsUserOnline(opponentName))
                            {
                                //User opponent = User.LoadFromJsonFile(opponentName);
                                User opponent = FindOnlineUser(onlineUserList, opponentName);
                                User.Fight(client, user, opponent);
                            }
                            else
                            {
                                SendMessage(client, "The Opponent must be online in order to compete.");
                            }
                        }
                    }
                    else if (message.StartsWith("!f") || message.StartsWith("!fight"))
                    {
                        string[] parts = message.Split(' ', 2);

                        if (parts.Length > 1)
                        {
                            if (int.TryParse(parts[1], out int num))
                            {
                                int enemy_lvl = int.Parse(parts[1]);
                                Enemy e = new Enemy("", 1).RougeDrone(enemy_lvl);
                                SendMessage(client, "Your Opponent: ");
                                Game.DisplayProfile(client, e.userObj);
                                User.Fight(client, user, e.userObj);
                            }
                        }
                        else
                        {
                            int enemy_lvl = user.Level;
                            Enemy e = new Enemy("", 1).RougeDrone(enemy_lvl);
                            e.userObj.FreeAP = 0;
                            SendMessage(client, "Your Opponent: ");
                            Game.DisplayProfile(client, e.userObj);
                            User.Fight(client, user, e.userObj);
                        }
                    }
                    else if (message.StartsWith("!aa") || message.StartsWith("!allocate_attributes"))
                    {
                        string[] parts = message.Split(' ', 4);

                        if (parts.Length > 1)
                        {
                            if (int.TryParse(parts[1], out int num) && int.TryParse(parts[2], out int num2) && int.TryParse(parts[3], out int num3))
                            {
                                int speed = int.Parse(parts[1]);
                                int intellect = int.Parse(parts[2]);
                                int luck = int.Parse(parts[3]);

                                Game.AllocateAP(client, user, speed, intellect, luck);
                            }
                        }
                    }
                    else if (message.StartsWith("!a") || message.StartsWith("!attributes"))
                    {
                        Game.DisplayProfile(client, user);
                    }
                    else if (message.StartsWith("!"))  // PM function
                    {
                        string[] parts = message.Split(' ', 2);

                        if (parts.Length > 1)
                        {
                            string recipient = parts[0].Substring(1).Trim();

                            foreach (KeyValuePair<TcpClient, string> pair in clients)
                            {
                                if (pair.Value.Equals(recipient))
                                {
                                    NetworkStream s = pair.Key.GetStream();
                                    byte[] msg = Encoding.ASCII.GetBytes(string.Format("PM: {0}: {1}", username, parts[1]));
                                    s.Write(msg, 0, msg.Length);
                                    user.MessageCount++;
                                    break;
                                }
                            }
                        }
                    }
                    else if (message.StartsWith("!help") || message.StartsWith("!h"))  // help function
                    {
                        string help_message = "Available commands:\n" +
                            "!class [class_name] - Sets the user's class to [class_name], which must be one of: Soldier, Engineer, Explorer\n" +
                            "!inventory - Displays your Inventory\n" +
                            "!i [item name] - Use item from your inventory\n" +
                            "!ir [item name] - Remove item from your inventory\n" +
                            "!ii [item name] - Inspect item in your inventory\n" +
                            "!is [item name] - Sell item from your inventory\n" +
                            "!isa [item name] - Sell all [item name] from your inventory\n" +
                            "!shop - Displays the local shop if there is one \n" +
                            "!shop [item name] - Buy [Item name] from the Shop \n" +
                            "!revive - Revives you\n" +
                            "!fight [enemy_level] - Initiates a battle with an enemy of the specified level\n" +
                            "!duel [username] - Initiates a battle with another User if he/she is online\n" +
                            "!allocate_attributes [speed] [intellect] [luck] - Increases the user's speed, intellect, and luck attributes by the specified amounts\n" +
                            "!attributes - Displays the user's current attributes\n" +
                            "!users - Displays a list of all connected users\n" +
                            "![username] [message] - Send a private message to a user";
                        SendMessage(client, help_message);
                    }
                    else // ____________________________chat function____________________________
                    {
                        Console.WriteLine("{0}: {1}", username, message);
                        //User.UpdateUserMessageCount(user);
                        user.MessageCount++;
                        byte[] msg;

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            msg = Encoding.ASCII.GetBytes(string.Format("{0}: {1}", username, message));

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
                }
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                clients.Remove(client);
                Console.WriteLine("{0} has left the chat", username);
                BroadcastMessage(username + " has left the chat");
                User.SaveToJsonFile(user);
                onlineUserList.Remove(user);
            }

            client.Close();
        }

        public static void SendMessage(TcpClient client, string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);

            NetworkStream stream = client.GetStream();
            stream.Write(msg, 0, msg.Length);
        }
        public static void BroadcastMessage(string message)
        {
            message += " ";
            byte[] msg = Encoding.ASCII.GetBytes(message);

            foreach (TcpClient c in clients.Keys)
            {
                NetworkStream stream = c.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
        }
        public static bool IsUserOnline(string username)
        {
            foreach (string name in clients.Values)
            {
                if (name == username) return true;
            }
            return false;
        }
    }
}