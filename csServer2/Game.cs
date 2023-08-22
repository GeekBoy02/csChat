using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Reflection.Metadata.Ecma335;

namespace SocketServer
{
    class Game
    {
        public static int Randomize(int value) // returns an int that ranges from -20% up to +20% of [value]
        {
            var random = new Random();
            var percentage = random.NextDouble() * 0.4 - 0.2;
            return (int)Math.Round(value * (1 + percentage));
        }
        public static int Randomize(int value, double minPercentage, double maxPercentage)
        {
            var random = new Random();
            var range = maxPercentage - minPercentage;
            var percentage = random.NextDouble() * range + minPercentage;
            return (int)Math.Round(value * (1 + percentage));
        }

        public static void AllocateAP(TcpClient client, User user, int sp, int ip, int lp)
        {
            if ((sp + ip + lp) <= user.FreeAP)
            {
                user.Speed += sp;
                user.Intellect += ip;
                user.Luck += lp;
                user.FreeAP -= sp + ip + lp;
                Program.SendMessage(client, user.Name_Enclosed + " allocated " + sp + " points to SPEED, " + ip + " points to INTELLECT, and " + lp + " points to LUCK.");
            }
            else
            {
                Program.SendMessage(client, "Input a valid number of free AP you want to use, for example with: !aa 4 2 1, you will increase speed by 4, intellect by 2, and luck by 1");
            }
        }
        public static void DisplayProfile(TcpClient client, User user)
        {
            string name = user.Name_Icon;
            string className = user.Class_Icon;
            string level = user.Level_Icon;
            string xp = user.Xp_Icon;
            string hp = user.Hp_Icon;
            string speed = user.Speed_Icon;
            string intellect = user.Intellect_Icon;
            string luck = user.Luck_Icon;
            string equipment = user.EquippedItem_Icon;
            string freeAP = user.FreeAP_Icon;
            string credits = user.Credits_Icon;

            string UserAttributes = Environment.NewLine +
            $"╔═══\n" +
            $"║ {name} \n" +
            $"║ {className} \n" +
            $"╠═══\n" +
            $"║ Level: {level}   \n" +
            $"║ XP: {xp}      \n" +
            $"║ HP: {hp}    \n" +
            $"╟──\n" +
            $"║ Speed: {speed} \n" +
            $"║ Intellect: {intellect} \n" +
            $"║ Luck: {luck} \n" +
            $"╟──\n" +
            $"║ Credits: {credits} \n" +
            $"║ Equipment: {equipment} \n" +
            $"║ Free AP: {freeAP} \n" +
            $"╚═══\n";

            Program.SendMessage(client, UserAttributes);
        }

        public static void ItemDrop(TcpClient client, User user, int probability, Item item)
        {
            Random rand = new Random();
            if (rand.Next(1, 101) <= probability)
            {
                user.AddItemToInventory(client, item);
            }
        }
        public static void LootDrop(TcpClient client, User attacker, int probability, User defender)
        {
            Random rand = new Random();
            if (rand.Next(1, 101) <= probability)
            {
                attacker.AddItemToInventory(client, defender.DropRandomItemOnDeath());
            }
        }
    }
}