using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Text;

namespace SocketServer
{
    public class User
    {
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
        [JsonPropertyName("active_quest")]
        public Quest ActiveQuest { get; set; }
        [JsonPropertyName("quest_progress")]
        public int QuestProgress { get; set; }

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
            QuestProgress = 0;
        }
        public static Item FindItemInInventory(List<Item> inventory, string itemName)
        {
            return inventory.Find(item => item.Name == itemName);
        }
        public Item FindItemInInventory(string itemName)
        {
            return Inventory.Find(item => item.Name == itemName);
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
                if (sendMsg) Program.SendMessage(client, "📦 " + Name + " recived " + newItem.Name + " \n");
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
            Program.SendMessage(client, "Remove " + Item.Name + " from Inventory. ");
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
        public void InspectItem(TcpClient client, string itemName)
        {
            Item i = FindItemInInventory(itemName);
            if (Inventory.Contains(i))
            {
                Program.SendMessage(client, $" {i.Icon} {i.Name} | {i.Description} | Value: {i.Value} ");
            }
            else if (EquippedItem == i)
            {
                Program.SendMessage(client, $" {i.Icon} {i.Name} | {i.Description} | Value: {i.Value} ");
            }
            else
            {
                Program.SendMessage(client, "Input valid Item ");
            }
        }
        public void SellItem(TcpClient client, string itemName, bool displayMsg)
        {
            Item i = FindItemInInventory(itemName);
            if (Inventory.Contains(i))
            {
                if (displayMsg) Program.SendMessage(client, $" {i.Icon} {i.Name} sold for {i.Value}📀 ");
                Credits += i.Value;
                RemoveItemFromInventory(i);
            }
            else
            {
                if (displayMsg) Program.SendMessage(client, "Input valid Item ");
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
            Program.SendMessage(client, $"{amountSold} {itemName} sold for {cGain}📀 ");
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
        public static void SaveToJsonFile(User user)
        {
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
        public static User LoadFromJsonFile(string name)
        {
            if (File.Exists("users/" + name + ".json"))
            {

                return JsonSerializer.Deserialize<User>(File.ReadAllText("users/" + name + ".json"));

            }
            return new User("notLoaded", "");
        }
        public static void Fight(TcpClient client, User p1, User p2)
        {
            if (p1.IsDead || p2.IsDead)
            {
                Program.SendMessage(client, "Both parties must be alive");
                return;
            }

            User attacker = p1;
            User defender = p2;

            if (p1.Speed < p2.Speed)
            {
                attacker = p2;
                defender = p1;
            }

            while (true)
            {
                Thread.Sleep(10);
                AttackEnemy(client, attacker, defender);
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
            SaveToJsonFile(p1);
            SaveToJsonFile(p2);
        }
        public static void AttackEnemy(TcpClient client, User Attacker, User Defender)
        {
            // change attributes depending on Equipment for Attacker
            int trueSpeed = Item.Consider_Speed_Equipment(Attacker);
            int trueInt = Item.Consider_Int_Equipment(Defender);
            int trueLuck = Item.Consider_Luck_Equipment(Attacker);

            string icon = "⚔️  ";
            int attackValue = Game.Randomize(trueSpeed);
            int defendValue = Game.Randomize(trueInt);

            int damage = attackValue - defendValue;

            if (damage > 0)
            {
                if (CritStrike(trueLuck))
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
            Program.SendMessage(client, $"{icon} {Attacker.Name} hits {Defender.Name} for {damage} DMG ");

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
                Program.SendMessage(client, msg);
                Game.LootDrop(client, Attacker, Attacker.Luck, Defender);
            }
        }
        static bool CritStrike(int luck)
        {
            Random rand = new Random();
            int criticalHitChance = luck;
            if (rand.Next(1, 101) <= criticalHitChance)
            {
                return true;
            }
            return false;
        }

        static void LevelUp(TcpClient client, User user)
        {
            bool playerDidLvlUp = false;
            while (user.Xp > (user.Level * 100))
            {
                playerDidLvlUp = true;
                user.Level++;
                user.FreeAP += 3;
                user.Hp += 100;
                if (user.Class == "Soldier") user.Speed++;
                else if (user.Class == "Engineer") user.Intellect++;
                else if (user.Class == "Explorer") user.Luck++;
                Program.SendMessage(client, "🆙 " + user.Name + " leveld up to Level " + user.Level + " ");
            }
            if (playerDidLvlUp)
            {
                user.Xp = 0;
            }
        }
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
            Inventory = new List<Item>
            {
                new Item().Bandage()
            };
        }
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
            Inventory = new List<Item>
            {
                new Item().Drink()
            };
        }
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
            Inventory = new List<Item>
            {
                new Item().Drink()
            };
        }
        public void HealUser(TcpClient client, int healAmount, bool sendMsg)
        {
            Hp += healAmount;
            if (sendMsg) Program.SendMessage(client, "🩹 " + Name + " heals for " + healAmount + " HP and is now at " + Hp + " HP ");
        }
        public static void HealUser(TcpClient client, User user, int healAmount, bool sendMsg)
        {
            user.Hp += healAmount;
            if (sendMsg) Program.SendMessage(client, "🩹 " + user.Name + " heals for " + healAmount + " HP and is now at " + user.Hp + " HP");
        }
        public static void HealOfflineUsers(object state)
        {
            Directory.CreateDirectory("users");
            string[] filePaths = Directory.GetFiles(@"users\", "*.json");
            string[] fileNames = new string[filePaths.Length];

            for (int i = 0; i < filePaths.Length; i++)
            {
                fileNames[i] = Path.GetFileNameWithoutExtension(filePaths[i]);
            }

            foreach (string file in fileNames)
            {
                User u = Program.FindOnlineUser(Program.onlineUserList, file);
                if (u == null)
                {
                    Console.WriteLine("🩹 healing " + file);
                    u = LoadFromJsonFile(file);
                    u.Hp += 10;
                    SaveToJsonFile(u);
                }
            }
        }
        public void Move(TcpClient client, string locationName, List<Location> world)
        {
            Location loc = Program.FindLocation(world, locationName);
            loc.Visitors.Add(this);   // add user to location visitors
            CurrentLocation = locationName;
            Program.SendMessage(client, $"You travel to {locationName} \n \n  << " + loc.WelcomeMessage + " >> ");
            SaveToJsonFile(this);
            //loc.SaveToJsonFile(loc);
        }
        public void BuyItem(TcpClient client, string itemName, List<Location> world)
        {
            List<Item> shop = Program.FindLocation(world, CurrentLocation).Shop;
            if (!string.IsNullOrEmpty(itemName) && shop.Contains(Location.FindItemInShop(shop, itemName)))
            {
                Item i = Location.FindItemInShop(shop, itemName);
                if ((Credits - i.Value) > 0)
                {
                    AddItemToInventory(i);
                    Credits -= i.Value;
                    Program.SendMessage(client, $"You bought {i.Name} for {i.Value} {Credits_Icon_Only} ");
                }
                else
                {
                    Program.SendMessage(client, "You bought don't have enough CREDITS ");
                }
            }
        }
        public void BuyItem(TcpClient client, string itemName, List<Location> world, int amount)
        {
            List<Item> shop = Program.FindLocation(world, CurrentLocation).Shop;
            if (!string.IsNullOrEmpty(itemName) && shop.Contains(Location.FindItemInShop(shop, itemName)))
            {
                Item item = Location.FindItemInShop(shop, itemName);
                int credsRequired = Credits * amount;
                if ((credsRequired - item.Value) > 0)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        AddItemToInventory(item);
                        Credits -= item.Value;
                    }
                    Program.SendMessage(client, $"You bought {amount} {item.Name} for {item.Value * amount} {Credits_Icon_Only} ");
                }
                else
                {
                    Program.SendMessage(client, "You bought don't have enough CREDITS ");
                }
            }
        }
    }
}