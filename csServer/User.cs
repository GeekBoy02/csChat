using System.Text.Json.Serialization;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent; // Import concurrent namespace

namespace SocketChatServer
{
    public class User
    {
        [JsonPropertyName("name")] // Specify the name of the json property
        public string Name { get; set; }
        [JsonPropertyName("address")] // Specify the name of the json property
        public string Address { get; set; }
        [JsonPropertyName("connection_count")] // Specify the name of the json property
        public int ConnectionCount { get; set; }
        [JsonPropertyName("message_count")] // Specify the name of the json property
        public int MessageCount { get; set; }

        //game
        [JsonPropertyName("class")] // Specify the name of the json property
        public string Class { get; set; }
        [JsonPropertyName("level")] // Specify the name of the json property
        public int Level { get; set; }
        [JsonPropertyName("xp")] // Specify the name of the json property
        public int xp { get; set; }
        [JsonPropertyName("healthpoints")] // Specify the name of the json property
        public int hp { get; set; }

        [JsonPropertyName("speed")] // Specify the name of the json property
        public int speed { get; set; }
        [JsonPropertyName("intellect")] // Specify the name of the json property
        public int intellect { get; set; }
        [JsonPropertyName("luck")] // Specify the name of the json property
        public int luck { get; set; }
        [JsonPropertyName("freeAP")] // Specify the name of the json property
        public int freeAP { get; set; }

        // Define a constructor that takes two parameters: name and address
        public User(string name, string address)
        {
            Name = name;
            Address = address;
            ConnectionCount = 1; // Initialize connection count to 1
            MessageCount = 0; // Initialize message count to 0

            Class = "Soldier";
            Level = 1;
            xp = 0;
            hp = 100;

            speed = 15;
            intellect = 7;
            luck = 10;
            freeAP = 5;
        }
    }


}
