using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

namespace SocketServer
{
    public class Location
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("welcomeMsg")]
        public string WelcomeMessage { get; set; }

        [JsonPropertyName("quests")]
        public List<Quest> Quests { get; set; }
        public static Quest FindQuestInLocation(List<Quest> quests, string questName)   // return new Instance of Quest
        {
            return quests.Find(quest => quest.Name == questName)?.Clone();
        }

        [JsonPropertyName("enemies")]
        public List<Enemy> Enemies { get; set; }

        [JsonPropertyName("shop")]
        public List<Item> Shop { get; set; }
        public static Item FindItemInShop(List<Item> shop, string itemName)
        {
            return shop.Find(item => item.Name == itemName);
        }

        [JsonPropertyName("x")]
        public int x { get; set; }

        [JsonPropertyName("y")]
        public int y { get; set; }

        public List<User> Visitors { get; set; }

        [JsonConstructor]
        public Location()
        {
            Name = "Location name";
            Description = "description";
            WelcomeMessage = $"Welcome to {Name}";
            Quests = new List<Quest>();
            Enemies = new List<Enemy>();
            Visitors = new List<User>();
        }
        public Location CryoStation()
        {
            Name = "Cryo-Station";
            Level = 1;
            Description = "The start of your Adventure.";
            WelcomeMessage = $"Welcome to {Name}";
            Enemies = new List<Enemy>
            {
                new Enemy("", 1).RougeDroneStatic(Level),
                new Enemy("", 1).DroneMotherStatic(Level + 10)
            };
            Shop = new List<Item>
            {
                new Item().Drink()
            };
            Quests = new List<Quest>()
            {
                //new Quest().LoadFromJsonFile(Name, "Introduction"),
                //new Quest().LoadFromJsonFile(Name, "Drone-Slaughter")
                //new Quest().Introduction()
            };
            Quests = Quest.LoadAllFromFolder(Name);
            return this;
        }







        
        public static void AddVisitors(List<User> userOnline, List<Location> world)
        {
            foreach (var l in world)
            {
                foreach (var u in userOnline)
                {
                    if (u.CurrentLocation == l.Name)
                    {
                        l.Visitors.Add(u);
                    }
                }

            }
        }
        public static void AddVisitors(User user, List<Location> world)
        {
            foreach (var l in world)
            {
                if (user.CurrentLocation == l.Name)
                {
                    l.Visitors.Add(user);
                }
            }
        }
        public void AddVisitors(User user)
        {
            Visitors.Add(user);
        }
        public static void RemoveVisitors(User user, List<Location> world)
        {
            foreach (var l in world)
            {
                if (user.CurrentLocation == l.Name)
                {
                    l.Visitors.Remove(user);
                }
            }
        }
        public void RemoveVisitors(User user)
        {
            Visitors.Remove(user);
        }
        public void AddQuest(Quest quest)
        {
            Quests.Add(quest);
        }
        public void RemoveQuest(Quest quest)
        {
            Quests.Remove(quest);
        }
        public void AddEnemy(Enemy enemy)
        {
            Enemies.Add(enemy);
        }
        public void RemoveEnemy(Enemy enemy)
        {
            Enemies.Remove(enemy);
        }
        public static void CreateJsonFile(Location location)
        {
            if (File.Exists($"world/{location.Name}/{location.Name}.json"))
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
            string json = JsonSerializer.Serialize(location, options);
            // Write the json string to a file with the username as the file name
            Directory.CreateDirectory($"world/{location.Name}");
            File.WriteAllText($"world/{location.Name}/{location.Name}.json", json);
        }
        public void SaveToJsonFile(Location location)
        {
            // Create a json serializer options object with some settings
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true, // Write indented json for readability
                IgnoreNullValues = true // Ignore null values in the object
            };
            // Serialize the user object to json format using the options
            string json = JsonSerializer.Serialize(location, options);
            // Write the json string to a file with the username as the file name
            Directory.CreateDirectory($"world/{location.Name}");
            File.WriteAllText($"world/{location.Name}/{location.Name}.json", json);
        }
        public static Location LoadFromJsonFile(string name)
        {
            if (File.Exists($"world/{name}/{name}.json"))
            {
                return JsonSerializer.Deserialize<Location>(File.ReadAllText($"world/{name}/{name}.json"));
            }
            return new Location() { Name = "Location not Loaded" };
        }
    }
}
