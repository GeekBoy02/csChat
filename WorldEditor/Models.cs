using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace WorldEditor
{
    public class Location
    {
        public string name { get; set; } = "";
        public int level { get; set; }
        public string description { get; set; } = "";
        public string welcomeMsg { get; set; } = "";
        public List<Quest> quests { get; set; } = new();
        public List<Enemy> enemies { get; set; } = new();
        public List<Item> shop { get; set; } = new();
        public int x { get; set; }
        public int y { get; set; }
        public List<string> Visitors { get; set; } = new();
    }

    public class Quest
    {
        public string name { get; set; } = "";
        public int level { get; set; }
        public string description { get; set; } = "";
        public int xp_reward { get; set; }
        public int credit_reward { get; set; }
        public int prerequisite_LVL { get; set; }
        [JsonPropertyName("prerequisite_INT")]
        public int prerequisite_INT { get; set; }
        public List<JsonElement> steps { get; set; } = new();
        public int currentStageIndex { get; set; }
    }

    public class Enemy
    {
        public string name { get; set; } = "";
        public int Level { get; set; }
        public int hp { get; set; }
        public int speed { get; set; }
        [JsonPropertyName("int")]
        public int intellect { get; set; }
        public int luck { get; set; }
        public int credits { get; set; }
        public List<Item> loot { get; set; } = new();
    }

    public class Item
    {
        public string name { get; set; } = "";
        public string icon { get; set; } = "";
        public string description { get; set; } = "";
        public int value { get; set; }
    }
}