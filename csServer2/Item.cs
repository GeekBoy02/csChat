using System.Globalization;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
namespace SocketServer
{
    public class Item
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }

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
            Description = $"Increases your SPEED by {Value}% in a Fight";
            Value = 10;
            return this;
        }
        public Item Glasses()
        {
            Name = "Glasses";
            Icon = "👓";
            Description = $"Increases your INTELLECT by {Value}% in a Fight";
            Value = 10;
            return this;
        }
        public Item Scanner()
        {
            Name = "Scanner";
            Icon = "📡";
            Description = $"Increases your LUCK by {Value}% in a Fight";
            Value = 10;
            return this;
        }

        public static void UseItem(TcpClient client, User user, Item item, bool sendMsg, int amount)
        {
            if (!user.Inventory.Contains(item))
            {
                Program.SendMessage(client, "Item not found in Inventory.");
                return;
            }

            int iii = user.Inventory.Count(n => n == item);     // check if you have less items than you want to use
            if (iii < amount)
            {
                Program.SendMessage(client, "Enter correct amount, you only have " + iii + " amount of " + item.Name);
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                switch (item.Name)
                {
                    case "Bandage":
                        if (sendMsg) Program.SendMessage(client, "You used a Bandage.");
                        user.Hp += item.Value * 5;
                        user.RemoveItemFromInventory(item);
                        break;

                    case "Drink":
                        if (sendMsg) Program.SendMessage(client, "You drank a Drink.");
                        user.Hp += item.Value * 5;
                        user.RemoveItemFromInventory(item);
                        break;

                    case "Boots":
                        if (sendMsg) Program.SendMessage(client, "You equipped some Boots.");
                        user.EquippedItem = item;
                        user.RemoveItemFromInventory(item);
                        break;

                    case "Glasses":
                        if (sendMsg) Program.SendMessage(client, "You equipped Glasses.");
                        user.EquippedItem = item;
                        user.RemoveItemFromInventory(item);
                        break;

                    case "Scanner":
                        if (sendMsg) Program.SendMessage(client, "You equipped a Scanner.");
                        user.EquippedItem = item;
                        user.RemoveItemFromInventory(item);
                        break;

                    // Add more cases for other items as needed

                    default:
                        Program.SendMessage(client, "Unknown item.");
                        break;
                }
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