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

        /// <summary>
        /// StartIntroQuest is a method that initializes a quest for a user, sets it as the user's active quest, and then advances the quest steps automatically. 
        /// It takes in a Quest object, a TcpClient for communication, and a User object representing the player. The method assigns the provided quest to the user's
        /// ActiveQuest property and then calls AdvanceQuestStep to progress through the quest steps, passing in the client, user, and a boolean indicating that the 
        /// steps should advance automatically.
        /// </summary>
        /// <param name="quest"></param>
        /// <param name="client"></param>
        /// <param name="user"></param>
        public void StartIntroQuest(Quest quest, TcpClient client, User user)
        {
            user.ActiveQuest = quest;
            //CurrentQuest = user.ActiveQuest;
            AdvanceQuestStep(client, user, true);
        }
        /// <summary>
        /// FinishQuest is a method that handles the completion of a quest for a user. It awards the user with experience points (XP) and credits based on the rewards defined in 
        /// the active quest.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        public void FinishQuest(TcpClient client, User user)
        {
            user.Xp += user.ActiveQuest.XP_reward;
            user.Credits += user.ActiveQuest.Credit_reward;
            user.ActiveQuest.CurrentStepIndex = 0;
            if (!user.completedQuests.Contains(user.ActiveQuest.Name))
            {
                user.completedQuests.Add(user.ActiveQuest.Name);
            }
            Program.SendMessage(client, "You completed the <" + user.ActiveQuest.Name + "> Quest");
            Program.SendMessage(client, "You gained " + user.ActiveQuest.XP_reward + " XP and " + user.ActiveQuest.Credit_reward + " CREDITS");
            user.ActiveQuest = new Quest().DefaultQuest();
            User.SaveUserToJsonFile(user);
        }
        /// <summary>
        /// AdvanceQuestStep is a method that progresses the user's active quest by one step. It checks if the current step is complete, if the user is alive, 
        /// and then processes the current step's
        /// </summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="autoAdvance"></param>
        public void AdvanceQuestStep(TcpClient client, User user, bool autoAdvance)
        {
            if (user.ActiveQuest.IsComplete)
            {
                FinishQuest(client, user);
                return;
            }

            if (user.IsDead)
            {
                Program.SendMessage(client, "You need to be alive to progress your Quest");
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
                    User.Fight(client, user, enemy.userObj, Program.speedModItemNamesListPath, Program.intModItemNamesListPath, Program.luckModItemNamesListPath);
                    if (user.IsDead)
                    {
                        Program.SendMessage(client, "You died during the last Fight and abandoned the Quest");
                        break;
                    }
                }
            }
            // Give the player items
            List<Item> i = currentStep.Items;
            if (i != null)
            {
                foreach (Item item in currentStep.Items)
                {
                    user.AddItemToInventory(client, item, true);
                }
            }
            // Move the player to a new Location
            if (currentStep.MoveTo != null)
            {
                user.Move(client, currentStep.MoveTo, Program.world);
            }
            // Move to the next stage
            user.ActiveQuest.CurrentStepIndex++;
            if (autoAdvance)
            {
                Thread.Sleep(2000);
                AdvanceQuestStep(client, user, true);
            }
        }
    }
}