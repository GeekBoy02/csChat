
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using SocketServer;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SocketServer
{
    public class QuestManager
    {
        public Quest CurrentQuest { get; set; }
        //public User user { get; set; }

        public void StartQuest(Quest quest, TcpClient client, User user)
        {
            CurrentQuest = quest;
            AdvanceQuestStep(client, user);
        }

        public void AdvanceQuestStep(TcpClient client, User user)
        {
            if (CurrentQuest.IsComplete)
            {
                user.Xp += CurrentQuest.XP_reward;
                User.SaveToJsonFile(user);              // not tested
                return;
            }

            QuestStep currentStep = CurrentQuest.Steps[CurrentQuest.CurrentStepIndex];

            // Display dialogue
            string Text = currentStep.Text;
            Program.SendMessage(client, Text);

            // Handle enemy encounters
            List<Enemy> es = currentStep.Enemies;
            if (es != null)
            {
                foreach (Enemy enemy in currentStep.Enemies)
                {
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
            // Move to the next stage
            CurrentQuest.CurrentStepIndex++;
            Thread.Sleep(2000);
            AdvanceQuestStep(client, user);
        }
    }
}