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
        /// <summary>
        /// Finds a quest in a location's quest list by name and returns a clone of it. Returns null if not found.
        /// </summary>
        /// <param name="quests">The list of quests to search.</param>
        /// <param name="questName">The name of the quest to find.</param>
        /// <returns>A cloned Quest object, or null if not found.</returns>
        public static Quest FindQuestInLocation(List<Quest> quests, string questName)   // return new Instance of Quest
        {
            return quests.Find(quest => quest.Name == questName)?.Clone();
        }

        [JsonPropertyName("enemies")]
        public List<Enemy> Enemies { get; set; }

        [JsonPropertyName("shop")]
        public List<Item> Shop { get; set; }
        /// <summary>
        /// Finds an item in a shop's inventory by name.
        /// </summary>
        /// <param name="shop">The shop inventory to search.</param>
        /// <param name="itemName">The name of the item to find.</param>
        /// <returns>The Item object, or null if not found.</returns>
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
            Shop = new List<Item>();
            Visitors = new List<User>();
        }
        
        /// <summary>
        /// Adds all online users to their respective locations based on their current location.
        /// </summary>
        /// <param name="userOnline">The list of online users to add.</param>
        /// <param name="world">The list of all locations in the game world.</param>
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
        /// <summary>
        /// Adds a single user to their current location's visitor list.
        /// </summary>
        /// <param name="user">The user to add.</param>
        /// <param name="world">The list of all locations in the game world.</param>
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
        /// <summary>
        /// Adds a user to this location's visitor list.
        /// </summary>
        /// <param name="user">The user to add.</param>
        public void AddVisitors(User user)
        {
            Visitors.Add(user);
        }
        /// <summary>
        /// Removes a user from all location visitor lists across the game world.
        /// </summary>
        /// <param name="user">The user to remove.</param>
        /// <param name="world">The list of all locations in the game world.</param>
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
        /// <summary>
        /// Removes a user from this location's visitor list.
        /// </summary>
        /// <param name="user">The user to remove.</param>
        public void RemoveVisitors(User user)
        {
            Visitors.Remove(user);
        }
        /// <summary>
        /// Adds a quest to this location's quest list.
        /// </summary>
        /// <param name="quest">The quest to add.</param>
        public void AddQuest(Quest quest)
        {
            Quests.Add(quest);
        }
        /// <summary>
        /// Removes a quest from this location's quest list.
        /// </summary>
        /// <param name="quest">The quest to remove.</param>
        public void RemoveQuest(Quest quest)
        {
            Quests.Remove(quest);
        }
        /// <summary>
        /// Adds an enemy to this location's enemy list.
        /// </summary>
        /// <param name="enemy">The enemy to add.</param>
        public void AddEnemy(Enemy enemy)
        {
            Enemies.Add(enemy);
        }
        /// <summary>
        /// Removes an enemy from this location's enemy list.
        /// </summary>
        /// <param name="enemy">The enemy to remove.</param>
        public void RemoveEnemy(Enemy enemy)
        {
            Enemies.Remove(enemy);
        }
        /// <summary>
        /// Creates and saves a location to a JSON file. Only creates the file if it doesn't already exist.
        /// </summary>
        /// <param name="location">The location to save.</param>
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
        }        /// <summary>
        /// Saves a location to a JSON file, overwriting any existing file.
        /// </summary>
        /// <param name="location">The location to save.</param>        /// <summary>
        /// Saves a location to a JSON file, overwriting any existing file.
        /// </summary>
        /// <param name="location">The location to save.</param>
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
        /// <summary>
        /// Loads a location from a JSON file by name. Returns a default location if the file is not found.
        /// </summary>
        /// <param name="name">The name of the location (used to construct the file path).</param>
        /// <returns>A Location object deserialized from the JSON file, or a default location if not found.</returns>
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
