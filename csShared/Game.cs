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
    public class Game
    {
        /// <summary>
        /// Applies a random variation to a value, returning a result within +/- 20% of the original value.
        /// </summary>
        /// <param name="value">The base value to randomize.</param>
        /// <returns>A randomized integer ranging from -20% to +20% of the input value.</returns>
        public static int Randomize(int value) // returns an int that ranges from -20% up to +20% of [value]
        {
            var random = new Random();
            var percentage = random.NextDouble() * 0.4 - 0.2;
            return (int)Math.Round(value * (1 + percentage));
        }
        /// <summary>
        /// Applies a random variation to a value within a specified percentage range.
        /// </summary>
        /// <param name="value">The base value to randomize.</param>
        /// <param name="minPercentage">The minimum percentage adjustment (e.g., -0.2 for -20%).</param>
        /// <param name="maxPercentage">The maximum percentage adjustment (e.g., 0.2 for +20%).</param>
        /// <returns>A randomized integer within the specified percentage range.</returns>
        public static int Randomize(int value, double minPercentage, double maxPercentage)
        {
            var random = new Random();
            var range = maxPercentage - minPercentage;
            var percentage = random.NextDouble() * range + minPercentage;
            return (int)Math.Round(value * (1 + percentage));
        }

        /// <summary>
        /// Allocates free attribute points to a user's stats. Validates that the user has enough free AP before applying the changes.
        /// </summary>
        /// <param name="client">The TCP client connection of the user.</param>
        /// <param name="user">The user allocating attribute points.</param>
        /// <param name="sp">Speed points to allocate.</param>
        /// <param name="ip">Intellect points to allocate.</param>
        /// <param name="lp">Luck points to allocate.</param>
        public static void AllocateAP(TcpClient client, User user, int sp, int ip, int lp)
        {
            if ((sp + ip + lp) <= user.FreeAP)
            {
                user.Speed += sp;
                user.Intellect += ip;
                user.Luck += lp;
                user.FreeAP -= sp + ip + lp;
                ServerCallbacks.SendMessage?.Invoke(client, user.Name_Enclosed + " allocated " + sp + " points to SPEED, " + ip + " points to INTELLECT, and " + lp + " points to LUCK.");
            }
            else
            {
                ServerCallbacks.SendMessage?.Invoke(client, "Input a valid number of free AP you want to use, for example with: !aa 4 2 1, you will increase speed by 4, intellect by 2, and luck by 1");
            }
        }
        /// <summary>
        /// Displays a formatted user profile with all character stats and equipment to the client.
        /// </summary>
        /// <param name="client">The TCP client connection to send the profile to.</param>
        /// <param name="user">The user whose profile should be displayed.</param>
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

            ServerCallbacks.SendMessage?.Invoke(client, UserAttributes);
        }

        /// <summary>
        /// Drops a random item from a defeated user to a victorious user based on a probability check.
        /// </summary>
        /// <param name="client">The TCP client connection of the attacker.</param>
        /// <param name="attacker">The user receiving the dropped item.</param>
        /// <param name="probability">The percentage chance (1-100) for the item to drop.</param>
        /// <param name="item">The item to potentially drop.</param>
        public static void ItemDrop(TcpClient client, User user, int probability, Item item)
        {
            Random rand = new Random();
            if (rand.Next(1, 101) <= probability)
            {
                user.AddItemToInventory(client, item, true);
            }
        }
        /// <summary>
        /// Drops a random item from a defeated user to a victorious user based on a probability check.
        /// </summary>
        /// <param name="client">The TCP client connection of the attacker.</param>
        /// <param name="attacker">The user receiving the looted item.</param>
        /// <param name="probability">The percentage chance (1-100) for an item to drop.</param>
        /// <param name="defender">The defeated user who drops the item.</param>
        public static void LootDrop(TcpClient client, User attacker, int probability, User defender)
        {
            Random rand = new Random();
            if (rand.Next(1, 101) <= probability)
            {
                Item i = defender.DropRandomItemOnDeath();
                attacker.AddItemToInventory(client, i, false);
                ServerCallbacks.SendMessage?.Invoke(client, "📦 " + attacker.Name + " recived " + i.Name + " from " + defender.Name + " ");
            }
        }
        /// <summary>
        /// Drops a random item from a defeated user to a victorious user, with critical loot chance based on the attacker's luck stat.
        /// </summary>
        /// <param name="client">The TCP client connection of the attacker.</param>
        /// <param name="attacker">The user receiving the looted item.</param>
        /// <param name="defender">The defeated user who drops the item.</param>
        public static void LootDrop(TcpClient client, User attacker, User defender)
        {
            Random rand = new Random();
            // Base critical hit chance with diminishing returns for attacker's luck
            double baseCriticalHitChance = 100 * (1 - Math.Exp(-attacker.Luck / 100.0));

            // Calculate the luck difference
            int luckDifference = attacker.Luck - defender.Luck;

            // Apply diminishing returns to luck difference adjustment
            double luckAdjustment = 1 + (1 - Math.Exp(-Math.Abs(luckDifference) / 100.0)) * (luckDifference > 0 ? 1 : -1);

            // Adjust critical hit chance based on luck difference
            double adjustedCriticalHitChance = baseCriticalHitChance * luckAdjustment;

            // Ensure the critical hit chance does not exceed 100%
            adjustedCriticalHitChance = Math.Min(adjustedCriticalHitChance, 100);

            if (rand.NextDouble() * 100 <= adjustedCriticalHitChance)
            {
                Item i = defender.DropRandomItemOnDeath();
                attacker.AddItemToInventory(client, i, false);
                ServerCallbacks.SendMessage?.Invoke(client, "📦 " + attacker.Name + " recived " + i.Name + " from " + defender.Name + " ");
            }
        }
    }
}