using System.Text.Json.Serialization;
using System.Text.Json;

namespace SocketServer
{
    public class Quest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("xp_reward")]
        public int XP_reward { get; set; }
        [JsonPropertyName("credit_reward")]
        public int Credit_reward { get; set; }
        [JsonPropertyName("prerequisite_LVL")]
        public int Prerequisite_lvl { get; set; }
        [JsonPropertyName("prerequisite_INT")]
        public int Prerequisite_int { get; set; }
        [JsonPropertyName("steps")]
        public List<QuestStep> Steps { get; set; }
        [JsonPropertyName("currentStageIndex")]
        public int CurrentStepIndex { get; set; }
        //[JsonPropertyName("IsComplete")]
        [JsonIgnore]
        public bool IsComplete => CurrentStepIndex >= Steps.Count;

        [JsonConstructor]
        public Quest()
        {

        }

        public Quest Clone()
        {
            // Deep-clone via JSON serialization to ensure Steps, Enemies and nested objects are copied.
            var json = JsonSerializer.Serialize(this);
            var copy = JsonSerializer.Deserialize<Quest>(json);
            if (copy != null) copy.CurrentStepIndex = 0;
            return copy ?? new Quest().DefaultQuest();
        }

        public Quest DefaultQuest()
        {
            Name = "No Quest";
            Level = 1;
            XP_reward = 0;
            Credit_reward = 0;
            Prerequisite_lvl = 1;
            Prerequisite_int = 1;
            Description = "You have no Quest";
            Steps = new List<QuestStep>()
            {
                new QuestStep() { Text = "You have no Quest" },
                new QuestStep() { Text = "You have no Quest" },
                new QuestStep() { Text = "You have no Quest" },
                new QuestStep() { Text = "You have no Quest" },
                new QuestStep() { Text = "You have no Quest" }
                //new QuestStep() { Enemies = new List<Enemy>(){ new Enemy("placeholder",1).RougeDroneStatic(1) } },
                //new QuestStep() { MoveTo = new Location().CryoStation().Name }
            };
            return this;
        }
        public Quest Introduction()
        {
            // Name = "Introduction";
            // Level = 1;
            // XP_reward = 12;
            // Prerequisite_lvl = 1;
            // Prerequisite_int = 1;
            // Description = "The first Quest";
            // Steps = new List<QuestStep>()
            // {
            //     new QuestStep() { Text = "You wake up from a long Sleep and find yourself in a Cryo-Pod." },
            //     new QuestStep() { Text = "You remember that you are on a Colony Ship that is supposed to colonize a Planet called MATIO." },
            //     new QuestStep() { Text = "You realize that while asleep, your Body has been tamperd with and modified \nyou can move faster than you remember, and a lot more precise." },
            //     new QuestStep() { Text = "You notice Instructions on the Wall: \n1) Take tools from the Locker \n2) Grab a Speed-Suit from the wardrobe \n3) Head to the Control Room and activate the Fabricator" },
            //     new QuestStep() { Text = "You open the locker and find a DATA-DAGGER, you take it, grab the Speed-Suit, and head to the Control room via an elevator." },
            //     new QuestStep() { Text = "As the elevator door opens you see a Worker-Drone, it turn hostile the moment it sees you." },
            //     new QuestStep() { Text = "You grab your D-D and charge to attack.", Enemies = new List<Enemy>(){ new Enemy("placeholder",1).RougeDroneStatic(1) } },
            //     new QuestStep() { Text = "You manage to kill the Drone by stabbing it really fast, the drone hit you a couple of times too and damaged your Suit and Body " },
            //     new QuestStep() { Items = new List<Item>(){ new Item().Drink()} },
            //     new QuestStep() { MoveTo = new Location().CryoStation().Name }
            // };
            if (File.Exists("world/intro.json"))
            {
                return LoadFromJsonFile("world/intro.json");
            }
            else
            {
                Console.WriteLine("Intro Quest not found");
            }
            return new Quest().DefaultQuest();
        }

        // to be changed or deleted
        public static void CreateJsonFile(string location, Quest quest)
        {
            if (File.Exists("world/" + location + "/quests/" + quest.Name + ".json"))
            {
                return;
            }
            // Create a json serializer options object with some settings
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true, // Write indented json for readability
                IgnoreNullValues = true // Ignore null values in the object
            };
            // Serialize the user object to json format using the options
            string json = JsonSerializer.Serialize(quest, options);
            // Write the json string to a file with the username as the file name
            Directory.CreateDirectory("world/" + location + "/quests");
            File.WriteAllText("world/" + location + "/quests/" + quest.Name + ".json", json);
        }

        // to be changed or deleted
        public void SaveToJsonFile(string location, Quest quest)
        {
            // Create a json serializer options object with some settings
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true, // Write indented json for readability
                IgnoreNullValues = true // Ignore null values in the object
            };
            // Serialize the user object to json format using the options
            string json = JsonSerializer.Serialize(quest, options);
            // Write the json string to a file with the username as the file name
            Directory.CreateDirectory("world/" + location + "/quests");
            File.WriteAllText("world/" + location + "/quests/" + quest.Name + ".json", json);
        }

        // to be changed or deleted
        public Quest LoadFromJsonFile(string location, string name)
        {
            if (File.Exists("world/" + location + "/quests/" + name + ".json"))
            {
                return JsonSerializer.Deserialize<Quest>(File.ReadAllText("world/" + location + "/quests/" + name + ".json"));
            }
            return new Quest() { Name = "Quest not loaded" };
        }

        public Quest LoadFromJsonFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                return JsonSerializer.Deserialize<Quest>(File.ReadAllText(filepath));
            }
            return new Quest() { Name = "Quest not loaded" };
        }

        // to be changed or deleted
        public static List<Quest> LoadAllFromFolder(string location)
        {
            List<Quest> ql = new List<Quest>();
            string[] files = Directory.GetFiles("world/" + location + "/quests");
            foreach (string file in files)
            {
                string jsonFile = Path.GetFileName(file);
                string path = Path.GetFullPath(file);
                Console.WriteLine(Path.GetFileName(file));

                if (File.Exists(path))
                {
                    ql.Add(JsonSerializer.Deserialize<Quest>(File.ReadAllText(path)));
                }
            }
            return ql;
        }
    }
}