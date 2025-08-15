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

        [JsonConstructor]
        public Item()
        {

        }

        public Item Bandage()
        {
            Name = "Bandage";
            Icon = "🩹";
            Description = "Restores some health.";
            Value = 1;
            return this;
        }
        public Item Drink()
        {
            Name = "Drink";
            Icon = "🧃";
            Description = "Tasty Drink that restores some health.";
            Value = 2;
            return this;
        }
        public Item Boots()
        {
            Name = "Boots";
            Icon = "👢";
            Value = 10;
            Description = $"Increases your SPEED by {Value}% in a Fight";
            return this;
        }
        public Item Glasses()
        {
            Name = "Glasses";
            Icon = "👓";
            Value = 10;
            Description = $"Increases your INTELLECT by {Value}% in a Fight";
            return this;
        }
        public Item Scanner()
        {
            Name = "Scanner";
            Icon = "📡";
            Value = 10;
            Description = $"Increases your LUCK by {Value}% in a Fight";
            return this;
        }

        public static void UseItem(TcpClient client, User user, Item item, bool sendMsg, int amount)
        {
            if (item == null)
            {
                Program.SendMessage(client, "Item not found in Inventory.");
                return;
            }

            if (!user.Inventory.Any(n => string.Equals(n?.Name, item?.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Program.SendMessage(client, "Item not found in Inventory.");
                return;
            }

            int maxItemCount = user.Inventory.Count(n => string.Equals(n?.Name, item?.Name, StringComparison.OrdinalIgnoreCase));     // count items by name (case-insensitive)
            if (maxItemCount < amount)
            {
                Program.SendMessage(client, "Enter correct amount, you only have " + maxItemCount + " amount of " + item.Name);
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
                    case "Bandage":
                        //if (sendMsg) Program.SendMessage(client, "You used a Bandage.");
                        user.Hp += current.Value * 5;
                        if (user.Inventory.Remove(current)) usedCount++;
                        break;

                    case "Drink":
                        //if (sendMsg) Program.SendMessage(client, "You drank a Drink.");
                        user.Hp += current.Value * 5;
                        if (user.Inventory.Remove(current)) usedCount++;
                        break;

                    case "Boots":
                        if (sendMsg) Program.SendMessage(client, "You equipped some Boots.");
                        user.EquippedItem = current;
                        // remove the specific instance that was equipped
                        user.Inventory.Remove(current);
                        endLoop = true; // Exit loop after equipping boots
                        break;

                    case "Glasses":
                        if (sendMsg) Program.SendMessage(client, "You equipped Glasses.");
                        user.EquippedItem = current;
                        user.Inventory.Remove(current);
                        endLoop = true; // Exit loop after equipping glasses
                        break;

                    case "Scanner":
                        if (sendMsg) Program.SendMessage(client, "You equipped a Scanner.");
                        user.EquippedItem = current;
                        user.Inventory.Remove(current);
                        endLoop = true; // Exit loop after equipping scanner
                        break;

                    // Add more cases for other items as needed

                    default:
                        Program.SendMessage(client, "Unknown item.");
                        break;
                }
                if (endLoop) break; // Exit the loop if an item was equipped
            }

            if (sendMsg && usedCount > 0)
            {
                Program.SendMessage(client, $"You used {usedCount} {item.Name}(s).");
            }
        }

        public static int Consider_Speed_Equipment(User user)
        {
            switch (user.EquippedItem.Name)
            {
                case "Boots":
                    double increasePercentage = (double)user.EquippedItem.Value / 100;
                    return (int)(user.Speed * (1 + increasePercentage));

                default:
                    break;
            }
            return user.Speed;
        }
        public static int Consider_Int_Equipment(User user)
        {
            switch (user.EquippedItem.Name)
            {
                case "Glasses":
                    double increasePercentage = (double)user.EquippedItem.Value / 100;
                    return (int)(user.Intellect * (1 + increasePercentage));

                default:
                    break;
            }
            return user.Intellect;
        }
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
    }
}