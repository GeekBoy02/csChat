using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Text;
using System.Security.Cryptography;

namespace SocketServer
{
    // not yet used anywhere
    public enum UserStatus
    {
        Idle,
        Questing,
        Fighting
    }

    public class User
    {
        [JsonPropertyName("status")]
        public UserStatus Status { get; set; }
        [JsonPropertyName("isDead")]
        public bool IsDead { get; set; }
        [JsonPropertyName("name")] // Specify the name of the json property
        public string Name { get; set; }
        public string Name_Enclosed
        {
            get { return $"< {Name} >"; }
        }
        public string Name_Icon
        {
            get { return $"{Name} 👤"; }
        }
        [JsonPropertyName("address")] // Specify the name of the json property
        public string Address { get; set; }
        [JsonPropertyName("connection_count")] // Specify the name of the json property
        public int ConnectionCount { get; set; }
        [JsonPropertyName("message_count")] // Specify the name of the json property
        public int MessageCount { get; set; }

        //game
        [JsonPropertyName("class")] // Specify the name of the json property
        public string Class { get; set; }
        public string Class_Icon
        {
            get { return $"{Class} 🪪"; }
        }
        [JsonPropertyName("level")] // Specify the name of the json property
        public int Level { get; set; }
        public string Level_Icon
        {
            get { return $"{Level} 🎖️"; }
        }
        [JsonPropertyName("xp")] // Specify the name of the json property
        public int Xp { get; set; }
        public string Xp_Icon
        {
            get { return $"{Xp} ⭐"; }
        }
        [JsonPropertyName("max_hp")] // Specify the name of the json property
        public int MaxHp { get; set; }
        [JsonPropertyName("hp")] // Specify the name of the json property
        private int _hp;
        public int Hp
        {
            get { return _hp; }
            set
            {
                if (value < 0)
                {
                    _hp = 0;
                    IsDead = true;
                    ActiveQuest = new Quest().DefaultQuest();
                }
                else if (value > MaxHp)
                {
                    _hp = MaxHp;
                    IsDead = false;
                }
                else
                {
                    _hp = value;
                    IsDead = false;
                }
            }
        }
        public string Hp_Icon
        {
            get { return $"{_hp} ❤️"; }
        }
        [JsonPropertyName("credits")] // Specify the name of the json property
        private int _credits;
        public int Credits
        {
            get { return _credits; }
            set
            {
                if (value < 0)
                {
                    _credits = 0;
                }
                else if (value > 9999)
                {
                    _credits = 9999;
                }
                else
                {
                    _credits = value;
                }
            }
        }
        public string Credits_Icon
        {
            get { return $"{Credits} {Credits_Icon_Only}"; }
        }
        public string Credits_Icon_Only
        {
            get { return "📀"; }
        }
        [JsonPropertyName("speed")] // Specify the name of the json property
        public int Speed { get; set; }
        public string Speed_Icon
        {
            get { return $"{Speed} ⚡"; }
        }
        [JsonPropertyName("intellect")] // Specify the name of the json property
        public int Intellect { get; set; }
        public string Intellect_Icon
        {
            get { return $"{Intellect} 🧠"; }
        }
        [JsonPropertyName("luck")] // Specify the name of the json property
        public int Luck { get; set; }
        public string Luck_Icon
        {
            get { return $"{Luck} 🍀"; }
        }
        [JsonPropertyName("equippedItem")] // Specify the name of the json property
        private Item _EquippedItem;
        public Item EquippedItem
        {
            get { return _EquippedItem; }
            set
            {
                if (_EquippedItem != null)
                {
                    AddItemToInventory(_EquippedItem);
                }
                _EquippedItem = value;
            }
        }
        public string EquippedItem_Icon
        {
            get { return $"{EquippedItem.Name} {EquippedItem.Icon}"; }
        }
        [JsonPropertyName("freeAP")] // Specify the name of the json property
        public int FreeAP { get; set; }

        public string FreeAP_Icon
        {
            get { return $"{FreeAP} ➕"; }
        }
        [JsonPropertyName("inventory")] // Specify the name of the json property
        public List<Item> Inventory { get; set; } = new List<Item>();
        [JsonPropertyName("current_location")]
        public string CurrentLocation { get; set; }
        [JsonPropertyName("active_quest")]         //  ______________ WIP _____________
        //[JsonIgnore]
        public Quest ActiveQuest { get; set; }
        [JsonPropertyName("completed_quests")]
        public List<string> completedQuests { get; set; }

        /* 










        */

        // Define a constructor that takes two parameters: name and address
        public User(string name, string address)
        {
            Name = name;
            Address = address;
            ConnectionCount = 0; // Initialize connection count to 1
            MessageCount = 0; // Initialize message count to 0

            Class = "Soldier";
            Level = 1;
            Xp = 0;
            MaxHp = 100;
            Hp = 100;

            Speed = 15;
            Intellect = 7;
            Luck = 10;

            Credits = 0;
            EquippedItem = new Item();
            FreeAP = 5;
            Inventory = new List<Item>
            {
                new Item().Bandage()
            };
            ActiveQuest = null;
            completedQuests = new List<string>();
        }
        public static Item FindItemInInventory(List<Item> inventory, string itemName)
        {
            return inventory.Find(item => item.Name == itemName);
        }
        public Item FindItemInInventory(string itemName)
        {
            return Inventory.Find(item => item.Name == itemName);
        }
        public static int GetItemCountInInventory(List<Item> inventory, string itemName)
        {
            return inventory.Count(item => item.Name == itemName);
        }
        public int GetItemCountInInventory(string itemName)
        {
            return Inventory.Count(item => item.Name == itemName);
        }
        /// <summary>
        /// Adds an [newItem] to the users inventory and sends him a message
        /// </summary>
        /// <param name="client">The client the message is send to</param>
        /// <param name="newItem">The Item that is added</param>
        public void AddItemToInventory(TcpClient client, Item newItem, bool sendMsg)
        {
            if (newItem != null)
            {
                Inventory.Add(newItem);
                if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, "📦 " + Name + " recived " + newItem.Name + " \n");
            }
        }
        public void AddItemToInventory(Item newItem)
        {
            if (newItem != null)
            {
                Inventory.Add(newItem);
            }
        }
        public void RemoveItemFromInventory(TcpClient client, Item Item)
        {
            ServerCallbacks.SendMessage?.Invoke(client, "Remove " + Item.Name + " from Inventory. ");
            Inventory.Remove(Item);
        }
        public void RemoveItemFromInventory(Item Item)
        {
            Inventory.Remove(Item);
        }
        public Item DropRandomItemOnDeath()
        {
            if (Inventory.Count == 0)
            {
                return null; // No items to drop
            }

            Random random = new Random();
            int index = random.Next(0, Inventory.Count);
            Item droppedItem = Inventory[index];
            Inventory.RemoveAt(index);

            return droppedItem;
        }
        /// <summary>
        /// InspectItem is a method that allows the user to inspect an item in their inventory or equipped item,
        /// it takes in the item name and checks if the item is in the user's inventory or equipped, if it is it sends a message to the user with the item's name, icon,
        /// description and value, if the item is not found it sends a message asking the user to input a valid item name
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemName"></param>
        public void InspectItem(TcpClient client, string itemName)
        {
            Item i = FindItemInInventory(itemName);
            if (Inventory.Contains(i))
            {
                ServerCallbacks.SendMessage?.Invoke(client, $" {i.Icon} {i.Name} | {i.Description} | Value: {i.Value} ");
            }
            else if (EquippedItem.Name == itemName)
            {
                ServerCallbacks.SendMessage?.Invoke(client, $" {EquippedItem.Icon} {EquippedItem.Name} | {EquippedItem.Description} | Value: {EquippedItem.Value} ");
            }
            else
            {
                ServerCallbacks.SendMessage?.Invoke(client, "Input valid Item ");
            }
        }
        public void SellItem(TcpClient client, string itemName, bool displayMsg)
        {
            Item i = FindItemInInventory(itemName);
            if (Inventory.Contains(i))
            {
                if (displayMsg) ServerCallbacks.SendMessage?.Invoke(client, $" {i.Icon} {i.Name} sold for {i.Value}📀 ");
                Credits += i.Value;
                RemoveItemFromInventory(i);
            }
            else
            {
                if (displayMsg) ServerCallbacks.SendMessage?.Invoke(client, "Input valid Item ");
            }
        }
        public void SellAllofItem(TcpClient client, string itemName)
        {
            int cGain = 0;
            int amountSold = 0;
            while (Inventory.Contains(FindItemInInventory(itemName)))
            {
                Item i = FindItemInInventory(itemName);
                SellItem(client, itemName, false);
                cGain += i.Value;
                amountSold++;
            }
            ServerCallbacks.SendMessage?.Invoke(client, $"{amountSold} {itemName} sold for {cGain}📀 ");
        }
        public static void CreateJsonFile(User user)
        {
            if (File.Exists("users/" + user.Name + ".json"))
            {
                return;
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
            Directory.CreateDirectory("users");
            File.WriteAllText("users/" + user.Name + ".json", json);
        }
        public static void UpdateUserConnectionCount(User user)
        {
            if (File.Exists("users/" + user.Name + ".json"))
            {
                string address = user.Address;
                user = JsonSerializer.Deserialize<User>(File.ReadAllText("users/" + user.Name + ".json")); // If so, deserialize it to a user object
                user.ConnectionCount++;
                user.Address = address;
                JsonSerializerOptions options = new JsonSerializerOptions // Create a json serializer options object with some settings
                {
                    WriteIndented = true, // Write indented json for readability
                    IgnoreNullValues = true // Ignore null values in the object
                };
                string json = JsonSerializer.Serialize(user, options);// Serialize the user object to json format using the options
                Directory.CreateDirectory("users");
                File.WriteAllText("users/" + user.Name + ".json", json);
            }
        }
        public static void UpdateUserMessageCount(User user)
        {
            if (File.Exists("users/" + user.Name + ".json"))
            {
                user = JsonSerializer.Deserialize<User>(File.ReadAllText("users/" + user.Name + ".json")); // If so, deserialize it to a user object
                user.MessageCount++;
                JsonSerializerOptions options = new JsonSerializerOptions // Create a json serializer options object with some settings
                {
                    WriteIndented = true, // Write indented json for readability
                    IgnoreNullValues = true // Ignore null values in the object
                };
                string json = JsonSerializer.Serialize(user, options);// Serialize the user object to json format using the options
                Directory.CreateDirectory("users");
                File.WriteAllText("users/" + user.Name + ".json", json);
            }
        }
        public static void SaveUserToJsonFile(User user)
        {
            // Create a json serializer options object with some settings
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true, // Write indented json for readability
                IgnoreNullValues = true, // Ignore null values in the object
                IncludeFields = true
            };
            // Serialize the user object to json format using the options
            string json = JsonSerializer.Serialize(user, options);
            // Write the json string to a file with the username as the file name
            Directory.CreateDirectory("users");
            File.WriteAllText("users/" + user.Name + ".json", json);
        }
        public static User LoadUserFromJsonFile(string name)
        {
            if (File.Exists("users/" + name + ".json"))
            {
                return JsonSerializer.Deserialize<User>(File.ReadAllText("users/" + name + ".json"));
            }
            return new User("notLoaded", "");
        }
        public static void Fight(TcpClient client, User p1, User p2, string speedModNamesListPath, string intModNamesListPath, string luckModNamesListPath)
        {
            if (p1.IsDead || p2.IsDead)
            {
                ServerCallbacks.SendMessage?.Invoke(client, "Both parties must be alive");
                return;
            }

            User attacker = p1;
            User defender = p2;

            if (p1.Speed < p2.Speed)
            {
                attacker = p2;
                defender = p1;
            }

            string[] speedModItemNames = Item.GetItemNames(speedModNamesListPath);
            string[] intModItemNames = Item.GetItemNames(intModNamesListPath);
            string[] luckModItemNames = Item.GetItemNames(luckModNamesListPath);

            while (true)
            {
                Thread.Sleep(10);
                AttackEnemy(client, attacker, defender, speedModItemNames, intModItemNames, luckModItemNames);
                if (defender.Hp <= 0)
                {
                    LevelUp(client, attacker);
                    break;
                }
                // switch roles
                var temp = defender;
                defender = attacker;
                attacker = temp;
            }
            SaveUserToJsonFile(p1);
            SaveUserToJsonFile(p2);
        }

        // public static string[] GetStatModItemNames(string path)
        // {
        //     string[] nameList = new string[0];
        //     var ItemList = new List<Item>();
        //     try
        //     {
        //         if (!File.Exists(path)) ItemList = new List<Item>();
        //         var txt = File.ReadAllText(path);
        //         ItemList = JsonSerializer.Deserialize<List<Item>>(txt) ?? new List<Item>();
        //     }
        //     catch
        //     {
        //         ItemList = new List<Item>();
        //     }

        //     foreach (var item in ItemList)
        //     {
        //         nameList = nameList.Append(item.Name).ToArray();
        //         //nameList.Append(item.Name).ToArray();
        //     }

        //     return nameList ?? new string[0];
        // }

        /// <summary>
        /// AttackEnemy is a method that takes in an attacker and a defender and calculates the damage dealt by the 
        /// attacker to the defender based on their attributes and equipped items, the method also checks for critical hits and applies them if 
        /// they occur, after the attack it checks if the defender is dead and if so it gives the attacker XP and credits for defeating the defender, 
        /// it also sends messages to the client about the attack and its outcome
        /// </summary>
        /// <param name="client"> The TCP client associated with the user </param>
        /// <param name="Attacker"> The user who is attacking </param>
        /// <param name="Defender"> The user who is being attacked </param>
        public static void AttackEnemy(TcpClient client, User Attacker, User Defender, string[] speedModItemNamesList, string[] intModItemNamesList, string[] luckModItemNamesList)
        {
            // change attributes depending on Equipment for Attacker
            int trueSpeed = Item.Consider_Speed_Equipment(Attacker, speedModItemNamesList);
            int trueInt = Item.Consider_Int_Equipment(Defender, intModItemNamesList);
            int trueLuckATK = Item.Consider_Luck_Equipment(Attacker, luckModItemNamesList);
            int trueLuckDEF = Item.Consider_Luck_Equipment(Defender, luckModItemNamesList);

            string icon = "⚔️  ";
            int attackValue = Game.Randomize(trueSpeed);
            int defendValue = Game.Randomize(trueInt);

            int damage = attackValue - defendValue;

            if (damage > 0)
            {
                if (CritStrike(trueLuckATK, trueLuckDEF))
                {
                    damage *= 2;
                    icon = "🎯 ";
                }
            }
            else
            {
                damage = 1;
            }

            Defender.Hp -= damage; // deal damage
            ServerCallbacks.SendMessage?.Invoke(client, $"{icon} {Attacker.Name} hits {Defender.Name} for {damage} DMG ");

            if (Defender.Hp <= 0)
            {
                int xpgain = Defender.Level * 10;
                Attacker.Xp += xpgain;
                int creditDrop = Defender.Credits / 2;
                Attacker.Credits += creditDrop;
                Defender.Credits -= creditDrop;
                string msg = "\n🥇 " + Attacker.Name + " defeated 💀 " + Defender.Name + " with " + Attacker.Hp + "HP remaining! " + Environment.NewLine +
                            "⭐ " + Attacker.Name + " gained " + xpgain + " XP " + Environment.NewLine +
                            $"📀 {Attacker.Name} looted {creditDrop} CREDITS from {Defender.Name}";
                ServerCallbacks.SendMessage?.Invoke(client, msg);
                Game.LootDrop(client, Attacker, Defender);
            }
        }
        static bool CritStrike(int attackerLuck, int defenderLuck)
        {
            Random rand = new Random();
            // Base critical hit chance with diminishing returns for attacker's luck
            double baseCriticalHitChance = 100 * (1 - Math.Exp(-attackerLuck / 100.0));

            // Calculate the luck difference
            int luckDifference = attackerLuck - defenderLuck;

            // Apply diminishing returns to luck difference adjustment
            double luckAdjustment = 1 + (1 - Math.Exp(-Math.Abs(luckDifference) / 100.0)) * (luckDifference > 0 ? 1 : -1);

            // Adjust critical hit chance based on luck difference
            double adjustedCriticalHitChance = baseCriticalHitChance * luckAdjustment;

            // Ensure the critical hit chance does not exceed 100%
            adjustedCriticalHitChance = Math.Min(adjustedCriticalHitChance, 100);

            if (rand.NextDouble() * 100 <= adjustedCriticalHitChance)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Checks if the user has enough XP to level up, if so it increases the users level by 1, gives him 3 free AP to spend on his attributes and increases his HP by 100, when leveling up the user also gets a message with his new level, if the user has more XP than needed for another level up the process repeats until the user has less XP than needed for the next level up, when the user levels up his XP is reset to 0
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        public static void LevelUp(TcpClient client, User user)
        {
            //bool playerDidLvlUp = false;
            while (user.Xp >= (user.Level * 100))
            {
                //playerDidLvlUp = true;
                user.Xp -= user.Level * 100; // retain overflow XP for subsequent levels
                user.Level++;
                user.FreeAP += 3;
                user.Hp += 100;
                if (user.Class == "Soldier") user.Speed++;
                else if (user.Class == "Engineer") user.Intellect++;
                else if (user.Class == "Explorer") user.Luck++;
                ServerCallbacks.SendMessage?.Invoke(client, "🆙 " + user.Name + " leveld up to Level " + user.Level + " ");
            }

        }
        /// <summary>
        /// changes user attributes to match the Soldier class, when changing class all other attributes except name, address, connection count and message count are reset to default values for the chosen class
        /// </summary>
        public void ChangeTo_Soldier()
        {
            this.Class = "Soldier";
            Level = 1;
            Xp = 0;
            Hp = 100;

            Speed = 15;
            Intellect = 7;
            Luck = 10;
            FreeAP = 3;
        }
        /// <summary>
        /// changes user attributes to match the Engineer class, when changing class all other attributes except name, address, connection count and message count are reset to default values for the chosen class
        /// </summary>
        public void ChangeTo_Engineer()
        {
            this.Class = "Engineer";
            Level = 1;
            Xp = 0;
            Hp = 100;

            Speed = 7;
            Intellect = 20;
            Luck = 5;
            FreeAP = 3;
        }
        /// <summary>
        /// changes user attributes to match the Explorer class, when changing class all other attributes except name, address, connection count and message count are reset to default values for the chosen class
        /// </summary>
        public void ChangeTo_Explorer()
        {
            this.Class = "Explorer";
            Level = 1;
            Xp = 0;
            Hp = 100;

            Speed = 10;
            Intellect = 5;
            Luck = 15;
            FreeAP = 5;
        }
        /// <summary>
        /// heals the user for a specified amount and sends him a message with the heal amount and his new hp, when [sendMsg] is set to false no message will be send to the user, this can be used for example when healing offline users
        /// </summary>
        /// <param name="client">the tcp client of the user to heal</param>
        /// <param name="healAmount">the amount of HP to heal the user by</param>
        /// <param name="sendMsg">whether to send a message to the user about the healing</param>
        public void HealUser(TcpClient client, int healAmount, bool sendMsg)
        {
            Hp += healAmount;
            if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, "🩹 " + Name + " heals for " + healAmount + " HP and is now at " + Hp + " HP ");
        }
        /// <summary>
        /// heals the user for a specified amount and sends him a message with the heal amount and his new hp, when [sendMsg] is set to false no message will be send to the user, this can be used for example when healing offline users
        /// </summary>
        /// <param name="client">the tcp client of the user to heal</param>
        /// <param name="user">the user to heal</param>
        /// <param name="healAmount">the amount of HP to heal the user by</param>
        /// <param name="sendMsg">whether to send a message to the user about the healing</param>
        public static void HealUser(TcpClient client, User user, int healAmount, bool sendMsg)
        {
            user.Hp += healAmount;
            if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, "🩹 " + user.Name + " heals for " + healAmount + " HP and is now at " + user.Hp + " HP");
        }
        public static void HealOfflineUsers(object state)
        {
            if (!Directory.Exists("users"))
            {
                Directory.CreateDirectory("users");
            }
            string[] filePaths = Directory.GetFiles(@"users", "*.json");
            string[] fileNames = new string[filePaths.Length];

            for (int i = 0; i < filePaths.Length; i++)
            {
                fileNames[i] = Path.GetFileNameWithoutExtension(filePaths[i]);
            }

            foreach (string file in fileNames)
            {
                User u = ServerCallbacks.FindOnlineUser?.Invoke(file);
                if (u == null)
                {
                    Console.WriteLine("🩹 healing " + file);
                    u = LoadUserFromJsonFile(file);
                    u.Hp += 10;
                    SaveUserToJsonFile(u);
                }
            }
        }
        /// <summary>
        /// moves the user to a new location and sends him a message with the new location name and welcome message
        /// </summary>
        /// <param name="client">the tcp client of the user to move</param>
        /// <param name="locationName">the name of the location to move the user to</param>
        /// <param name="world">the list of all locations in the world</param>
        public void Move(TcpClient client, string locationName, List<Location> world)
        {
            if (!string.IsNullOrEmpty(CurrentLocation))
            {
                Location oldLoc = ServerCallbacks.FindLocation?.Invoke(CurrentLocation);
                try
                {
                    oldLoc.Visitors.Remove(this); // remove user from old location visitors
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error removing user from old location visitors: " + e.Message);
                }
                //oldLoc.Visitors.Remove(this);
            }
            Location loc = ServerCallbacks.FindLocation?.Invoke(locationName);
            loc.Visitors.Add(this);   // add user to location visitors
            CurrentLocation = locationName;
            ServerCallbacks.SendMessage?.Invoke(client, $"You arrive at {locationName} \n \n  << " + loc.WelcomeMessage + " >> ");
            SaveUserToJsonFile(this);
            //loc.SaveToJsonFile(loc);
        }
        public void BuyItem(TcpClient client, string itemName, List<Location> world)
        {
            List<Item> shop = ServerCallbacks.FindLocation?.Invoke(CurrentLocation)?.Shop;
            if (!string.IsNullOrEmpty(itemName) && shop.Contains(Location.FindItemInShop(shop, itemName)))
            {
                Item i = Location.FindItemInShop(shop, itemName);
                if ((Credits - i.Value) > 0)
                {
                    AddItemToInventory(i);
                    Credits -= i.Value;
                    ServerCallbacks.SendMessage?.Invoke(client, $"You bought {i.Name} for {i.Value} {Credits_Icon_Only} ");
                }
                else
                {
                    ServerCallbacks.SendMessage?.Invoke(client, "You bought don't have enough CREDITS ");
                }
            }
        }
        public void BuyItem(TcpClient client, string itemName, List<Location> world, int amount)
        {
            List<Item> shop = ServerCallbacks.FindLocation?.Invoke(CurrentLocation)?.Shop;
            if (!string.IsNullOrEmpty(itemName) && shop.Contains(Location.FindItemInShop(shop, itemName)))
            {
                Item item = Location.FindItemInShop(shop, itemName);
                int credsRequired = Credits * amount;
                if ((credsRequired - item.Value * amount) >= 0)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        AddItemToInventory(item);
                        Credits -= item.Value;
                    }
                    ServerCallbacks.SendMessage?.Invoke(client, $"You bought {amount} {item.Name} for {item.Value * amount} {Credits_Icon_Only} ");
                }
                else
                {
                    ServerCallbacks.SendMessage?.Invoke(client, "You don't have enough CREDITS ");
                }
            }
        }

        public void ModItem(TcpClient client, string itemName)
        {
            Item item = FindItemInInventory(itemName);
            Item newItem = Item.newItem(item);

            int itemAmount = Inventory.Count(i => i.Name == itemName);
            if (itemAmount >= 2)
            {
                Random rand = new Random();
                if (rand.Next(2) == 0)
                {
                    newItem.Value++;
                    // if (!newItem.Name.EndsWith("+"))
                    // {
                    newItem.Name = newItem.Name + "+";
                    // }
                    RemoveItemFromInventory(FindItemInInventory(itemName));
                    RemoveItemFromInventory(FindItemInInventory(itemName));
                    AddItemToInventory(newItem);
                    ServerCallbacks.SendMessage?.Invoke(client, $"You modified {itemName}");
                }
                else
                {
                    ServerCallbacks.SendMessage?.Invoke(client, "You failed to modify the item");
                    RemoveItemFromInventory(FindItemInInventory(itemName));
                    RemoveItemFromInventory(FindItemInInventory(itemName));
                    // SaveUserToJsonFile(this);
                }
            }
            else
            {
                ServerCallbacks.SendMessage?.Invoke(client, "You need at least 2 of the same item to modify it ");
            }
        }
    }
}