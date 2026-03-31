using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;

//Dictionary Lookup for Client Commands

namespace SocketServer
{
    public static class CommandHandler
    {
        public delegate void CommandDelegate(TcpClient client, User user, string command, string[] args);

        public static Dictionary<string, CommandDelegate> Commands = new Dictionary<string, CommandDelegate>(StringComparer.OrdinalIgnoreCase);

        // Command Dict
        /// <summary>
        /// Static constructor that initializes the Commands dictionary with all available command mappings.
        /// </summary>
        static CommandHandler()
        {
            Commands["help"] = Help;
            Commands["h"] = Help;

            Commands["users"] = Users;
            Commands["u"] = Users;

            Commands["look"] = Look;
            Commands["la"] = Look;

            Commands["locals"] = Locals;
            Commands["l"] = Locals;

            Commands["move"] = Move;
            Commands["m"] = Move;

            Commands["quest"] = Quest;
            Commands["q"] = Quest;
            Commands["qg"] = Quest;
            Commands["qc"] = Quest;
            Commands["qa"] = Quest;

            Commands["shop"] = Shop;

            Commands["revive"] = Revive;

            Commands["use"] = UseItem;

            Commands["i"] = Inventory;
            Commands["inventory"] = Inventory;
            Commands["ir"] = Inventory;
            Commands["ii"] = Inventory;
            Commands["is"] = Inventory;
            Commands["isa"] = Inventory;

            Commands["class"] = Class;

            Commands["duel"] = Duel;
            Commands["d"] = Duel;

            Commands["fight"] = Fight;
            Commands["f"] = Fight;

            Commands["listEnemies"] = Listenemies;
            Commands["le"] = Listenemies;

            Commands["attackEnemy"] = AttackLocalEnemy;
            Commands["ae"] = AttackLocalEnemy;

            Commands["allocateAttributes"] = AllocateAttributes;
            Commands["aa"] = AllocateAttributes;

            Commands["attributes"] = Attributes;
            Commands["a"] = Attributes;
        }

        //Command Functions

        /// <summary>
        /// Displays a help message with all available commands and their usage.
        /// </summary>
        private static void Help(TcpClient client, User user, string cmd, string[] args)
        {
            string help = "Available commands:\n" +
                "!class [class_name] - Sets the user's class to [class_name], which must be one of: Soldier, Engineer, Explorer\n" +
                "!quest - Advance in you active quest\n" +
                "!quest completed - Shows all quest you completed\n" +
                "!quest abandon - Abandon your active quest\n!q" +
                "!qg [Questname] - (!quest get [Questname]) - Display/Accept a local Quest\n" +
                "!inventory - Displays your Inventory\n" +
                "!use [item name] - Use item from your inventory\n" +
                "!i [item name] - Use item from your inventory\n" +
                "!i [item name] [amount] - Use [amount] of [item name] from your inventory\n" +
                "!ir [item name] - Remove item from your inventory\n" +
                "!ii [item name] - Inspect item in your inventory\n" +
                "!is [item name] - Sell item from your inventory\n" +
                "!isa [item name] - Sell all [item name] from your inventory\n" +
                "!shop - Displays the local shop if there is one \n" +
                "!shop [item name] - Buy [Item name] from the Shop \n" +
                "!shop [item name] [amount]- Buy [amount] of [Item name] from the Shop \n" +
                "!move [location name] - move to [location name] \n" +
                "!look around - Reveals info about your current location \n" +
                "!revive - Revives you for a price\n" +
                "!fight [enemy_level] - Initiates a battle with an enemy of the specified level\n" +
                "!listEnemies - Displays a list of available Enemies in the current location\n" +
                "!attackEnemy [enemy_index] - Attack the enemy at the specified index\n" +
                "!duel [username] - Initiates a battle with another User if he/she is online\n" +
                "!allocateAttributes [speed] [intellect] [luck] - Increases the user's speed, intellect, and luck attributes by the specified amounts\n" +
                "!attributes - Displays the user's current attributes\n" +
                "!users - Displays a list of all connected users\n" +
                "!locals - Displays a list of all users in current location\n" +
                "![username] [message] - Send a private message to a user  \n";
            Program.SendMessage(client, help);
        }

        /// <summary>
        /// Displays a list of all connected users.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        private static void Users(TcpClient client, User user, string cmd, string[] args)
        {
            StringBuilder sb = new StringBuilder();

            // foreach (string name in clients.Values)
            // {
            //     sb.Append(name);
            //     sb.Append(", ");
            // }
            // string userlist = sb.ToString().TrimEnd();
            // userlist.TrimEnd();

            foreach (User u in Program.onlineUserList)
            {
                sb.Append("< " + u.Name + " > Level: " + u.Level + " hp: " + u.Hp + " \n");
            }

            string userlist = sb.ToString();

            Program.SendMessage(client, sb.ToString());
        }
        /// <summary>
        /// Displays information about the user's current location, including its description and any relevant details. 
        /// If the location cannot be found, a default message is shown instead.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        private static void Look(TcpClient client, User user, string cmd, string[] args)
        {
            Location loc = Program.FindLocation(user.CurrentLocation);
            if (loc != null)
            {
                Program.SendMessage(client, $"You are in {user.CurrentLocation} | {loc.Description}");
            }
            else
            {
                Program.SendMessage(client, "You are in nowhere. The sound of Nigh̶̑ẗ̸m̴͝a̸͒res surrounds you.");
            }
        }
        /// <summary>
        /// Locals command that retrieves the current location of the user and lists all other users present in the same location. 
        /// If the location is not found, a default message is displayed.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        private static void Locals(TcpClient client, User user, string cmd, string[] args)
        {
            //StringBuilder sb = new StringBuilder();
            Location loc = Program.FindLocation(user.CurrentLocation);
            if (loc == null)
            {
                Program.SendMessage(client, $"You are currently in nowhere | The sound of Nigh̶̑ẗ̸m̴͝a̸͒res surrounds you. ");
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (User u in loc.Visitors)
            {
                sb.Append("< " + u.Name + " > LVL: " + u.Level + " || HP: " + u.Hp + " \n");
            }

            string locals = sb.ToString();

            Program.SendMessage(client, locals);
        }
        /// <summary>
        /// Moves the user to a specified location if it exists. The location name is parsed from the command arguments, and the user's current location is updated accordingly.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        private static void Move(TcpClient client, User user, string cmd, string[] args)
        {
            /*string[] parts = message.Split(' ', 2);
            if (parts.Length > 1)
            {
                string locationName = message.Split()[1];*/
            //idgi

            if (args.Length == 0)
            {
                return;
            }

            string locationName = string.Join(" ", args);
            if (Program.FindLocation(locationName) != null)
            {
                user.Move(client, locationName, Program.world);
            }
            else
            {
                Program.SendMessage(client, "Input a valid Location  ");
            }
        }

        // QUEST START
        private static void Quest(TcpClient client, User user, string cmd, string[] args)
        {
            string action = cmd.ToLower();
            if (action == "quest" || action == "q")
            {
                if (args.Length == 0)
                {
                    // if (user.ActiveQuest == null)
                    // {
                    //     // Quest q = new Quest().Introduction();
                    //     // Quest q = new Quest().DefaultQuest();
                    //     // user.ActiveQuest = q;
                    //     // new QuestManager().StartIntroQuest(q, client, user);
                    // }
                    // else
                    // {
                    new QuestManager().AdvanceQuestStep(client, user, false);
                    // }
                    return;
                }

                action = args[0].ToLower();
                args = args.Skip(1).ToArray();
            }

            // Quest substrings/args
            switch (action)
            {
                case "get":
                case "qg":
                    HandleQuestGet(client, user, args);
                    break;

                case "completed":
                case "qc":
                    HandleQuestCompleted(client, user);
                    break;

                case "abandon":
                case "qa":
                    HandleQuestAbandon(client, user);
                    break;

                default:
                    Program.SendMessage(client, $"Unknown quest command. Use: !quest [get|completed|abandon] or !qg/!qc/!qa.");
                    break;
            }
        }

        /// <summary>
        /// Handles quest retrieval and acceptance. Displays available quests or accepts a specific quest if the user meets prerequisites.
        /// </summary>
        private static void HandleQuestGet(TcpClient client, User user, string[] args)
        {
            Location loc = Program.FindLocation(user.CurrentLocation);
            if (loc == null)
            {
                Program.SendMessage(client, $"You are currently in nowhere | The sound of Nigh̶̑ẗ̸m̴͝a̸͒res surrounds you. ");
                return;
            }

            if (args.Length == 0)
            {
                foreach (Quest q in loc.Quests)
                    if (user.Intellect >= q.Prerequisite_int) // user only sees quests if Intellect is high enough
                    {
                        Program.SendMessage(client, " <" + q.Name + "> " + q.Description + " | LVL: " + q.Level + " | XP: " + q.XP_reward);
                    }
                return;
            }

            string qName = string.Join(" ", args);
            Quest quest = Location.FindQuestInLocation(loc.Quests, qName);
            if (quest == null)
            {
                Program.SendMessage(client, "There is no Quest with that name here");
                return;
            }

            if (user.Level >= quest.Prerequisite_lvl && user.Intellect >= quest.Prerequisite_int)
            {
                user.ActiveQuest = quest;
                User.SaveUserToJsonFile(user);
                Program.SendMessage(client, "You accepted the " + user.ActiveQuest.Name + " Quest");
            }
            else
            {
                Program.SendMessage(client, "Your level or intellect is too low for this Quest  ");
            }
        }

        /// <summary>
        /// Displays all quests that the user has completed.
        /// </summary>
        private static void HandleQuestCompleted(TcpClient client, User user)
        {
            Program.SendMessage(client, "Quests you completed: ");
            foreach (string s in user.completedQuests)
            {
                Program.SendMessage(client, " " + s);
            }
        }

        /// <summary>
        /// Handles quest abandonment. Removes the user's active quest if they have one.
        /// </summary>
        private static void HandleQuestAbandon(TcpClient client, User user)
        {
            if (user.ActiveQuest != null)
            {
                Program.SendMessage(client, Environment.NewLine + "You abandoned your active quest:" + user.ActiveQuest.Name);
                user.ActiveQuest = new Quest().DefaultQuest();
            }
            else
            {
                Program.SendMessage(client, Environment.NewLine + "You have no active quest.");
            }
        }
        //QUEST END

        /// <summary>
        /// Handles shop interactions. Displays shop items or purchases items from the local shop.
        /// </summary>
        private static void Shop(TcpClient client, User user, string cmd, string[] args)
        {
            // Check if shop
            Location loc = Program.FindLocation(user.CurrentLocation);
            if (loc?.Shop == null || loc.Shop.Count == 0)
            {
                Program.SendMessage(client, "There is no shop around gere ");
                return;
            }

            if (args.Length == 0)
            {
                // display shop items
                StringBuilder sb = new StringBuilder();
                foreach (Item i in loc.Shop)
                {
                    sb.Append(i.Icon + "-" + i.Name + " | " + i.Value + "📀  \n");
                }
                Program.SendMessage(client, sb.ToString());
                return;
            }

            // Buy
            if (args.Length == 1)
            {
                user.BuyItem(client, args[0], Program.world);
            }
            // Buy Multiple
            else if (args.Length >= 2 && int.TryParse(args[1], out int amount))
            {
                user.BuyItem(client, args[0], Program.world, amount);
            }
            else
            {
                Program.SendMessage(client, "Invalid shop command. Usage: !shop [item] [amount]");
            }
        }

        /// <summary>
        /// Revives a dead user by restoring health at the cost of a quarter of their current credits.
        /// </summary>
        private static void Revive(TcpClient client, User user, string cmd, string[] args)
        {
            if (user.Hp == 0)
            {
                user.HealUser(client, 100, false);
                user.Credits -= user.Credits / 4;
                Program.SendMessage(client, $"You revived for {user.Credits / 4}📀 and now have {user.Hp} HP ");
            }
            else
            {
                Program.SendMessage(client, "You can only revive when DEAD ");
            }
        }

        /// <summary>
        /// Uses an item from the user's inventory with an optional quantity parameter.
        /// </summary>
        private static void UseItem(TcpClient client, User user, string cmd, string[] args)
        {
            if (user.IsDead)
            {
                Program.SendMessage(client, "You cannot use your inventory while dead  ");
                return;
            }

            if (args.Length == 0)
            {
                Program.SendMessage(client, "Input a valid amount, for example: !i Drink 3");
                return;
            }

            string itemName = args[0];
            Item item = user.FindItemInInventory(itemName);
            int amount = 1;
            if (args.Length > 1 && int.TryParse(args[1], out amount)) //<- retarded
            {
                //LOL
            }
            if (item != null)
                Item.UseItem(client, user, item, true, amount);
            else
                Program.SendMessage(client, "Item " + itemName + " not found.");
        }

        /// <summary>
        /// Manages inventory operations including displaying items, removing items, inspecting items, and selling items.
        /// </summary>
        private static void Inventory(TcpClient client, User user, string cmd, string[] args)
        {
            // <- 
            switch (cmd.ToLower())
            {
                case "inventory":
                case "i" when args.Length == 0:
                    // Show full inventory list
                    StringBuilder sb = new StringBuilder();
                    foreach (Item i in user.Inventory)
                        sb.Append($"{i.Icon}-{i.Name}, ");
                    Program.SendMessage(client, sb.ToString().TrimEnd(',', ' '));
                    break;

                case "ir":
                    // Remove item
                    if (args.Length == 0)
                        Program.SendMessage(client, "Usage: !ir [item]");
                    else
                    {
                        string itemName = string.Join(" ", args);
                        Item item = user.FindItemInInventory(itemName);
                        if (item != null)
                            user.RemoveItemFromInventory(client, item);
                        else
                            Program.SendMessage(client, $"Item '{itemName}' not found in inventory.");
                    }
                    break;

                case "ii":
                    if (args.Length == 0)
                        Program.SendMessage(client, "Usage: !ii [item]");
                    else
                        user.InspectItem(client, string.Join(" ", args));
                    break;

                case "is":
                    if (args.Length == 0)
                        Program.SendMessage(client, "Usage: !is [item]");
                    else
                        user.SellItem(client, string.Join(" ", args), true);
                    break;

                case "isa":
                    if (args.Length == 0)
                        Program.SendMessage(client, "Usage: !isa [item]");
                    else
                        user.SellAllofItem(client, string.Join(" ", args));
                    break;

                default:
                    if (user.IsDead)
                    {
                        Program.SendMessage(client, "You cannot use your inventory while dead.");
                        return;
                    }

                    string itemNameUse = string.Join(" ", args);
                    int amount = 1;
                    if (args.Length > 1 && int.TryParse(args.Last(), out int parsed))
                    {
                        amount = parsed;
                        itemNameUse = string.Join(" ", args.Take(args.Length - 1));
                    }

                    Item useItem = user.FindItemInInventory(itemNameUse);
                    if (useItem != null)
                        Item.UseItem(client, user, useItem, true, amount);
                    else
                        Program.SendMessage(client, $"Item '{itemNameUse}' not found.");
                    break;
            }
        }

        /// <summary>
        /// Changes the user's class to Soldier, Engineer, or Explorer.
        /// </summary>
        private static void Class(TcpClient client, User user, string cmd, string[] args)
        {
            if (args.Length == 0)
            {
                Program.SendMessage(client, "Usage: !class [Soldier|Engineer|Explorer]");
                return;
            }

            string className = args[0].ToLower();
            if (className == "soldier") user.ChangeTo_Soldier();
            else if (className == "engineer") user.ChangeTo_Engineer();
            else if (className == "explorer") user.ChangeTo_Explorer();
            else
            {
                Program.SendMessage(client, "Invalid class. Valid classes are Soldier, Engineer and Explorer.");
                return;
            }

            Program.SendMessage(client, $"You are now a {className}!");
            User.SaveUserToJsonFile(user);
        }
        /// <summary>
        /// Initiates a duel between the user and another online user specified by their username. If the opponent is found, the User.Fight method is called to start the duel; otherwise, a message is sent indicating that the specified user is not online.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param> 
        private static void Duel(TcpClient client, User user, string cmd, string[] args)
        {
            if (args.Length == 0)
            {
                Program.SendMessage(client, "Usage: !duel [username]");
                return;
            }

            string opponentName = args[0];
            User opponent = Program.FindOnlineUser(opponentName);
            if (opponent != null)
                User.Fight(client, user, opponent, Program.speedModItemNamesListPath, Program.intModItemNamesListPath, Program.luckModItemNamesListPath);
            else
                Program.SendMessage(client, "That user is not online.");
        }

        /// <summary>
        /// Initiates a fight with an enemy. Can fight a random location enemy or a specific level enemy if provided.
        /// </summary>
        private static void Fight(TcpClient client, User user, string cmd, string[] args)
        {
            Enemy enemy;
            if (args.Length > 0 && int.TryParse(args[0], out int level))
            {
                enemy = new Enemy("", 1).RougeDrone(level);
            }
            else
            {
                Location loc = Program.FindLocation(user.CurrentLocation);
                if (loc?.Enemies == null || loc.Enemies.Count == 0)
                {
                    Program.SendMessage(client, "No enemies here.");
                    return;
                }
                enemy = loc.Enemies[0].Clone();
                enemy = Enemy.RandomizeStats(enemy, false, true);
            }

            Program.SendMessage(client, "Your Opponent:");
            Game.DisplayProfile(client, enemy.userObj);
            User.Fight(client, user, enemy.userObj, Program.speedModItemNamesListPath, Program.intModItemNamesListPath, Program.luckModItemNamesListPath);
        }

        /// <summary>
        /// Displays a list of all enemies available in the user's current location.
        /// </summary>
        private static void Listenemies(TcpClient client, User user, string cmd, string[] args)
        {
            Location loc = Program.FindLocation(user.CurrentLocation);
            if (loc?.Enemies == null || loc.Enemies.Count == 0)
            {
                Program.SendMessage(client, "No enemies here.");
                return;
            }

            StringBuilder sb = new();
            for (int i = 0; i < loc.Enemies.Count; i++)
            {
                Enemy e = loc.Enemies[i];
                sb.Append($"[{i + 1}] < {e.userObj.Name} > LVL: {e.userObj.Level} HP: {e.userObj.Hp} \n");
            }
            Program.SendMessage(client, sb.ToString());
        }

        /// <summary>
        /// Attacks a specific enemy in the current location by index. Handles combat between player and enemy.
        /// </summary>
        private static void AttackLocalEnemy(TcpClient client, User user, string cmd, string[] args)
        {
            if (args.Length == 0)
            {
                Program.SendMessage(client, "Usage: !attackenemies [enemy_index]");
                Listenemies(client, user, cmd, args);
                return;
            }
            if (user.Hp <= 0)
            {
                Program.SendMessage(client, "You are incapacitated, use !revive or items to heal up.");
                return;
            }
            Location loc = Program.FindLocation(user.CurrentLocation);
            if (loc?.Enemies == null || loc.Enemies.Count == 0)
            {
                Program.SendMessage(client, "No enemies here.");
                return;
            }

            if (!int.TryParse(args[0], out int index) || index < 1 || index > loc.Enemies.Count)
            {
                Program.SendMessage(client, "Invalid enemy index.");
                return;
            }

            Enemy enemy = loc.Enemies[index - 1];
            bool eDead = enemy.userObj.Hp <= 0;
            if (eDead)
            {
                Program.SendMessage(client, "This enemy is already defeated.");
                return;
            }

            //enemy = Enemy.RandomizeStats(enemy, false, true);
            Program.SendMessage(client, "Your Opponent:");
            Game.DisplayProfile(client, enemy.userObj);
            User.AttackEnemy(client, user, enemy.userObj, Item.GetItemNames(Program.speedModItemNamesListPath),Item.GetItemNames(Program.intModItemNamesListPath), Item.GetItemNames(Program.luckModItemNamesListPath));                          // player attacks enemy
            if (eDead)
            {
                User.LevelUp(client, user);                                         // check if player can lvl up after attack
            }
            else
            {
                User.AttackEnemy(client, enemy.userObj, user, Item.GetItemNames(Program.speedModItemNamesListPath), Item.GetItemNames(Program.intModItemNamesListPath), Item.GetItemNames(Program.luckModItemNamesListPath));                      // enemy attacks player
                if (user.Hp <= 0) { User.LevelUp(client, enemy.userObj); }          // check if enemy can lvl up after attack (remove if enemy should not be able to lvl up)
            }
        }

        /// <summary>
        /// Allocates attribute points to the user's speed, intellect, and luck stats.
        /// </summary>
        private static void AllocateAttributes(TcpClient client, User user, string cmd, string[] args)
        {
            if (args.Length < 3 ||
                !int.TryParse(args[0], out int speed) ||
                !int.TryParse(args[1], out int intellect) ||
                !int.TryParse(args[2], out int luck))
            {
                Program.SendMessage(client, "Usage: !allocate_attributes [speed] [intellect] [luck]");
                return;
            }

            Game.AllocateAP(client, user, speed, intellect, luck);
        }

        /// <summary>
        /// Displays the user's current attributes and profile information.
        /// </summary>
        private static void Attributes(TcpClient client, User user, string cmd, string[] args)
        {
            Game.DisplayProfile(client, user);
        }
    }
}
