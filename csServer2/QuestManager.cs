using System.Net.Sockets;
using System.Runtime.CompilerServices;
using SocketServer;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SocketServer
{
    public class QuestManager
    {
        //public Quest CurrentQuest { get; set; }
        //public User user { get; set; }

        public void StartQuest(Quest quest, TcpClient client, User user)
        {
            user.ActiveQuest = quest;
            //CurrentQuest = user.ActiveQuest;
            AdvanceQuestStep(client, user, true);
        }
        public void FinishQuest(TcpClient client, User user)
        {
            user.Xp += user.ActiveQuest.XP_reward;
            user.completedQuests.Add(user.ActiveQuest.Name);
            Program.SendMessage(client, "You completed the <" + user.ActiveQuest.Name + "> Quest and gained " + user.ActiveQuest.XP_reward + " XP ");
            user.ActiveQuest = new Quest().DefaultQuest();
            User.SaveToJsonFile(user);
        }

        public void AdvanceQuestStep(TcpClient client, User user, bool autoAdvance)
        {
            if (user.ActiveQuest.IsComplete)
            {
                FinishQuest(client, user);
                return;
            }

            QuestStep currentStep = user.ActiveQuest.Steps[user.ActiveQuest.CurrentStepIndex];

            // Display dialogue
            string Text = currentStep.Text;
            Program.SendMessage(client, Text);

            // Handle enemy encounters
            List<Enemy> es = currentStep.Enemies;
            if (es != null)
            {
                foreach (Enemy enemy in currentStep.Enemies)
                {
                    Game.DisplayProfile(client, enemy.userObj);
                    User.Fight(client, user, enemy.userObj);
                }
            }
            // Give the player items
            List<Item> i = currentStep.Items;
            if (i != null)
            {
                foreach (Item item in currentStep.Items)
                {
                    user.AddItemToInventory(item);
                }
            }
            // Move the player to a new Location
            Location l = currentStep.MoveTo;
            if (l != null)
            {
                user.Move(client, l.Name, Program.world);
            }
            // Move to the next stage
            user.ActiveQuest.CurrentStepIndex++;
            if (autoAdvance)
            {
                Thread.Sleep(1000);
                AdvanceQuestStep(client, user, true);
            }
        }
    }
}