using System.Globalization;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SocketServer
{
    public class Item
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("icon")]
        public string Icon { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("value")]
        public int Value { get; set; }
        [JsonPropertyName("onUseMessage")]
        public string OnUseMessage { get; set; }
        [JsonPropertyName("equippable")]
        public bool isEquippable { get; set; }

        // [JsonPropertyName("canEquip")]
        // public bool CanEquip { get; set; }
        // [JsonPropertyName("canUse")]
        // public bool CanUse { get; set; }

        [JsonConstructor]
        public Item()
        {

        }

        /// <summary>
        /// Initializes this item as a Bandage, a consumable that restores health.
        /// </summary>
        /// <returns>This Item object with Bandage properties applied.</returns>
        public Item Bandage()
        {
            Name = "Bandage";
            Icon = "🩹";
            Description = "Restores some health.";
            Value = 1;
            return this;
        }
        /// <summary>
        /// Initializes this item as a Drink, a consumable that restores health.
        /// </summary>
        /// <returns>This Item object with Drink properties applied.</returns>
        public Item Drink()
        {
            Name = "Drink";
            Icon = "🧃";
            Description = "Tasty Drink that restores some health.";
            Value = 2;
            return this;
        }
        /// <summary>
        /// Initializes this item as Boots, an equipment that increases Speed stat in combat.
        /// </summary>
        /// <returns>This Item object with Boots properties applied.</returns>
        public Item Boots()
        {
            Name = "Boots";
            Icon = "👢";
            Value = 10;
            Description = $"Increases your SPEED by {Value}% in a Fight";
            return this;
        }
        /// <summary>
        /// Initializes this item as Glasses, an equipment that increases Intellect stat in combat.
        /// </summary>
        /// <returns>This Item object with Glasses properties applied.</returns>
        public Item Glasses()
        {
            Name = "Glasses";
            Icon = "👓";
            Value = 10;
            Description = $"Increases your INTELLECT by {Value}% in a Fight";
            return this;
        }
        /// <summary>
        /// Initializes this item as a Scanner, an equipment that increases Luck stat in combat.
        /// </summary>
        /// <returns>This Item object with Scanner properties applied.</returns>
        public Item Scanner()
        {
            Name = "Scanner";
            Icon = "📡";
            Value = 10;
            Description = $"Increases your LUCK by {Value}% in a Fight";
            return this;
        }

        /// <summary>
        /// Uses an item from a user's inventory. Handles consumables (healing) and equipment (equipping). Removes the item from inventory after use.
        /// </summary>
        /// <param name="client">The TCP client connection of the user.</param>
        /// <param name="user">The user using the item.</param>
        /// <param name="item">The item to use.</param>
        /// <param name="sendMsg">Whether to send a message to the user confirming the item use.</param>
        /// <param name="amount">The quantity of consumable items to use.</param>
        public static void UseItem(TcpClient client, User user, Item item, bool sendMsg, int amount)
        {
            if (item == null)
            {
                ServerCallbacks.SendMessage?.Invoke(client, "Item not found in Inventory.");
                return;
            }

            if (!user.Inventory.Any(n => string.Equals(n?.Name, item?.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ServerCallbacks.SendMessage?.Invoke(client, "Item not found in Inventory.");
                return;
            }

            int maxItemCount = user.Inventory.Count(n => string.Equals(n?.Name, item?.Name, StringComparison.OrdinalIgnoreCase));     // count items by name (case-insensitive)
            if (maxItemCount < amount)
            {
                ServerCallbacks.SendMessage?.Invoke(client, "Enter correct amount, you only have " + maxItemCount + " amount of " + item.Name);
                return;
            }

            int usedCount = 0;
            bool endLoop = false;
            for (int i = 0; i < amount; i++)
            {
                // Find the next matching item in the inventory each iteration (by name)
                Item? current = user.Inventory.Find(n => string.Equals(n?.Name, item?.Name, StringComparison.OrdinalIgnoreCase));
                if (current == null) break; // nothing left to remove

                switch (current.Name)
                {
                    // case "Bandage":
                        //if (sendMsg) Program.SendMessage(client, "You used a Bandage.");
                        //user.Hp += current.Value * 5;
                        // user.HealUser(client, current.Value * 5, sendMsg: false);
                        // if (user.Inventory.Remove(current)) usedCount++;
                        // break;

                    // case "Drink":
                        //if (sendMsg) Program.SendMessage(client, "You drank a Drink.");
                        //user.Hp += current.Value * 5;
                        // user.HealUser(client, current.Value * 5, sendMsg: false);
                        // if (user.Inventory.Remove(current)) usedCount++;
                        // break;

                    // case "Boots":
                    //     if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, "You equipped some Boots.");
                    //     user.EquippedItem = current;
                    //     // remove the specific instance that was equipped
                    //     user.Inventory.Remove(current);
                    //     endLoop = true; // Exit loop after equipping boots
                    //     break;

                    // case "Glasses":
                    //     if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, "You equipped Glasses.");
                    //     user.EquippedItem = current;
                    //     user.Inventory.Remove(current);
                    //     endLoop = true; // Exit loop after equipping glasses
                    //     break;

                    // case "Scanner":
                    //     if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, "You equipped a Scanner.");
                    //     user.EquippedItem = current;
                    //     user.Inventory.Remove(current);
                    //     endLoop = true; // Exit loop after equipping scanner
                    //     break;

                    // Add more cases for other items as needed

                    default:
                        bool ihi = isHealItem(current, GetItemNames("world/ItemDB/healItems.json")); // implementation can be improved 
                        if (ihi)
                        {
                            user.HealUser(client, current.Value * 5, sendMsg: false);
                            if (user.Inventory.Remove(current)) usedCount++;
                            break;
                        }
                        if (item.isEquippable)
                        {
                            if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, $"You equipped {item.Name}.");
                            if (sendMsg && !string.IsNullOrEmpty(item.OnUseMessage)) ServerCallbacks.SendMessage?.Invoke(client, item.OnUseMessage);
                            user.EquippedItem = current;
                            user.Inventory.Remove(current);
                            endLoop = true; // Exit loop after equipping
                            break;
                        }
                        else if (!string.IsNullOrEmpty(item.OnUseMessage))
                        {
                            //if (sendMsg) ServerCallbacks.SendMessage?.Invoke(client, $"You used {item.Name}.");
                            //if (!string.IsNullOrEmpty(item.OnUseMessage)) ServerCallbacks.SendMessage?.Invoke(client, item.OnUseMessage);
                            user.Inventory.Remove(current);
                            usedCount++;
                        }
                        else
                        {
                            ServerCallbacks.SendMessage?.Invoke(client, "Unknown item.");
                        }
                        break;
                }
                if (endLoop) break; // Exit the loop if an item was equipped
            }

            if (sendMsg && usedCount > 0)
            {
                ServerCallbacks.SendMessage?.Invoke(client, $"You used {usedCount} {item.Name}(s).");
                if (!string.IsNullOrEmpty(item.OnUseMessage)) { ServerCallbacks.SendMessage?.Invoke(client, item.OnUseMessage); }
            }
        }

        /// <summary>
        /// Calculates the effective Speed attribute considering equipped gear (e.g., Boots increase Speed).
        /// </summary>
        /// <param name="user">The user whose Speed is being calculated.</param>
        /// <returns>The effective Speed value, including equipment bonuses.</returns>
        public static int Consider_Speed_Equipment(User user)
        {
            switch (user.EquippedItem.Name)
            {
                case "Boots":
                    double increasePercentage = (double)user.EquippedItem.Value / 100; // Convert percentage to decimal
                    return (int)(user.Speed * (1 + increasePercentage));

                default:
                    break;
            }
            return user.Speed;
        }
        public static int Consider_Speed_Equipment(User user, string[] itemNamesList)
        {
            if (itemNamesList.Contains(user.EquippedItem.Name))
            {
                double increasePercentage = (double)user.EquippedItem.Value / 100; // Convert percentage to decimal
                return (int)(user.Speed * (1 + increasePercentage));
            }
            return user.Speed;
        }
        /// <summary>
        /// Calculates the effective Intellect attribute considering equipped gear (e.g., Glasses increase Intellect).
        /// </summary>
        /// <param name="user">The user whose Intellect is being calculated.</param>
        /// <returns>The effective Intellect value, including equipment bonuses.</returns>
        public static int Consider_Int_Equipment(User user)
        {
            switch (user.EquippedItem.Name)
            {
                case "Glasses":
                    double increasePercentage = (double)user.EquippedItem.Value / 100; // Convert percentage to decimal
                    return (int)(user.Intellect * (1 + increasePercentage)); // Apply percentage increase to Intellect

                default:
                    break;
            }
            return user.Intellect;
        }
        public static int Consider_Int_Equipment(User user, string[] itemNamesList)
        {
            if (itemNamesList.Contains(user.EquippedItem.Name))
            {
                double increasePercentage = (double)user.EquippedItem.Value / 100; // Convert percentage to decimal
                return (int)(user.Intellect * (1 + increasePercentage));
            }
            return user.Intellect;
        }
        /// <summary>
        /// Calculates the effective Luck attribute considering equipped gear (e.g., Scanner increases Luck).
        /// </summary>
        /// <param name="user">The user whose Luck is being calculated.</param>
        /// <returns>The effective Luck value, including equipment bonuses.</returns>
        public static int Consider_Luck_Equipment(User user)
        {
            switch (user.EquippedItem.Name)
            {
                case "Scanner":
                    double increasePercentage = (double)user.EquippedItem.Value / 100;
                    return (int)(user.Luck * (1 + increasePercentage));

                default:
                    break;
            }
            return user.Luck;
        }
        public static int Consider_Luck_Equipment(User user, string[] itemNamesList)
        {
            if (itemNamesList.Contains(user.EquippedItem.Name))
            {
                double increasePercentage = (double)user.EquippedItem.Value / 100; // Convert percentage to decimal
                return (int)(user.Luck * (1 + increasePercentage));
            }
            return user.Luck;
        }
        public static bool isHealItem(Item item, string[] itemNamesList)
        {
            if (itemNamesList.Contains(item.Name))
            {
                return true;
            }
            return false;
        }
        public static string[] GetItemNames(string path)
        {
            string[] nameList = new string[0];
            var ItemList = new List<Item>();
            try
            {
                if (!File.Exists(path)) ItemList = new List<Item>();
                var txt = File.ReadAllText(path);
                ItemList = JsonSerializer.Deserialize<List<Item>>(txt) ?? new List<Item>();
            }
            catch
            {
                ItemList = new List<Item>();
            }

            foreach (var item in ItemList)
            {
                nameList = nameList.Append(item.Name).ToArray();
                //nameList.Append(item.Name).ToArray();
            }

            return nameList ?? new string[0];
        }
    }
}