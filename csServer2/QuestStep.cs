using System.Text.Json.Serialization;
using System.Text.Json;

namespace SocketServer
{
    public class QuestStep
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("enemies")]
        public List<Enemy> Enemies { get; set; }
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
        [JsonPropertyName("moveTo")]
        public Location MoveTo { get; set; }
    }
}