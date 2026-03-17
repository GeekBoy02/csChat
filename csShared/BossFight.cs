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

        public void JoinFight(User user)
        {
            if (IsActive && !ActiveUsers.Contains(user))
            {
                ActiveUsers.Add(user);
                Console.WriteLine($"{user.Name} joined the fight against {BossEnemy.Name}!");
            }
        }

        public void LeaveFight(User user)
        {
            ActiveUsers.Remove(user);
            Console.WriteLine($"{user.Name} left the fight.");
        }

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

        public void EndFight()
        {
            IsActive = false;
            Console.WriteLine($"{BossEnemy.Name} has been defeated!");
            Console.WriteLine($"Victory! {ActiveUsers.Count} players defeated the boss.");
        }
    }
}