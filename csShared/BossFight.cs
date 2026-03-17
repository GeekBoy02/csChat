using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace SocketServer
{
    public class BossFight
    {
        public Enemy BossEnemy { get; set; }
        public List<User> ActiveUsers { get; set; }
        public bool IsActive { get; set; }

        public BossFight(Enemy BossEnemy)
        {
            this.BossEnemy = BossEnemy;
            ActiveUsers = new List<User>();
            IsActive = true;
        }

        /// <summary>
        /// Allows a user to join an active boss fight. The user is added to the active users list if the fight is still active and the user is not already participating.
        /// </summary>
        /// <param name="user">The user joining the boss fight.</param>
        public void JoinFight(User user)
        {
            if (IsActive && !ActiveUsers.Contains(user))
            {
                ActiveUsers.Add(user);
                Console.WriteLine($"{user.Name} joined the fight against {BossEnemy.Name}!");
            }
        }

        /// <summary>
        /// Removes a user from the active boss fight.
        /// </summary>
        /// <param name="user">The user leaving the boss fight.</param>
        public void LeaveFight(User user)
        {
            ActiveUsers.Remove(user);
            Console.WriteLine($"{user.Name} left the fight.");
        }

        /// <summary>
        /// Executes an attack by a user against the boss enemy. If the boss is defeated, the fight ends automatically.
        /// </summary>
        /// <param name="client">The TCP client of the attacker.</param>
        /// <param name="attacker">The user attacking the boss.</param>
        /// <param name="bossEnemy">The boss enemy being attacked.</param>
        public void AttackBoss(TcpClient client, User attacker, Enemy bossEnemy)
        {
            if (!IsActive)
            {
                Console.WriteLine("The boss fight has ended.");
                return;
            }

            User.AttackEnemy(client, attacker, bossEnemy.userObj);

            //BossHealth -= attacker.Speed;

            if (BossEnemy.HP <= 0)
            {
                EndFight();
            }
        }

        /// <summary>
        /// Ends the boss fight and marks it as inactive. Displays victory messages to the console.
        /// </summary>
        public void EndFight()
        {
            IsActive = false;
            Console.WriteLine($"{BossEnemy.Name} has been defeated!");
            Console.WriteLine($"Victory! {ActiveUsers.Count} players defeated the boss.");
        }
    }
}