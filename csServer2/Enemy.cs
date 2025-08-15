using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Reflection.Metadata.Ecma335;

namespace SocketServer
{
    public class Enemy
    {
        [JsonPropertyName("userObj")]
        public User userObj;
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("level")]
        private int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                if (value < 0)
                {
                    _level = 0;
                }
                else
                {
                    _level = value;
                }
            }
        }
        [JsonPropertyName("hp")]
        public int HP { get; set; }
        [JsonPropertyName("speed")]
        public int Speed { get; set; }
        [JsonPropertyName("int")]
        public int Intellect { get; set; }
        [JsonPropertyName("luck")]
        public int Luck { get; set; }
        [JsonPropertyName("credits")]
        public int Credits { get; set; }

        [JsonConstructor]
        public Enemy(string name, int level)
        {
            Name = name;
            Level = level;

            HP = 100;
            Speed = 5 + (level * 2);
            Intellect = 5 + (level * 2);
            Luck = 10;
            Credits = level * 2;

            userObj = new User(name, "")
            {
                //userObj.Name = name;
                Class = "Mob",
                Level = level,
                Hp = HP,
                Speed = Speed,
                Intellect = Intellect,
                Luck = Luck,
                Credits = Credits
            };
        }
        public Enemy Clone()
        {
            string n = Name;
            int l = Level;
            return new Enemy(n, l)
            {
                HP = HP,
                Credits = Credits,

                Speed = Speed,
                Intellect = Intellect,
                Luck = Luck
            };
        }
        public static Enemy RandomizeStats(Enemy enemy, bool randLuck, bool randCredits)
        {
            Enemy e = enemy;

            // e.Speed = Game.Randomize(enemy.Speed);
            // e.Intellect = Game.Randomize(enemy.Intellect);
            // if (randLuck) e.Luck = Game.Randomize(enemy.Luck);
            // if (randCredits) e.Credits = Game.Randomize(enemy.Credits);

            e.userObj.Speed = Game.Randomize(e.userObj.Speed);
            e.userObj.Intellect = Game.Randomize(e.userObj.Intellect);
            if (randLuck) e.userObj.Luck = Game.Randomize(enemy.userObj.Luck);
            if (randCredits) e.userObj.Credits = Game.Randomize(e.userObj.Credits);

            return e;
        }
        public Enemy DroneMother(int lvl)
        {
            Enemy e = new Enemy("Rouge Drone Mother", lvl);

            e.userObj.Speed = Game.Randomize(5 + (lvl * 2));
            e.userObj.Intellect = Game.Randomize(5 + (lvl * 2));
            e.userObj.Luck = 20;
            e.userObj.Credits = Game.Randomize(lvl * 2);

            e.userObj.Class = "Boss";
            e.userObj.Inventory = new List<Item>()
            {
                new Item().Boots(),
                new Item().Glasses(),
                new Item().Scanner()
            };
            return e;
        }
        public Enemy DroneMotherStatic(int lvl)
        {
            Enemy e = new Enemy("Rouge Drone Mother", lvl);

            e.userObj.Speed = 5 + (lvl * 2);
            e.userObj.Intellect = 5 + (lvl * 2);
            e.userObj.Luck = 20;
            e.userObj.Credits = lvl * 2;

            e.userObj.Class = "Boss";
            e.userObj.Inventory = new List<Item>()
            {
                new Item().Boots(),
                new Item().Glasses(),
                new Item().Scanner()
            };
            return e;
        }
        public Enemy RougeDrone(int lvl)
        {
            Enemy e = new Enemy("Rouge Drone", lvl);

            e.userObj.Speed = Game.Randomize(5 + (lvl * 2));
            e.userObj.Intellect = Game.Randomize(5 + (lvl * 2));
            e.userObj.Luck = 10;
            e.userObj.Credits = Game.Randomize(lvl * 2);

            e.userObj.FreeAP = 0;
            e.userObj.Inventory = new List<Item>()
            {
                new Item().Bandage(),
                new Item().Scanner()
            };
            return e;
        }
        public Enemy RougeDroneStatic(int lvl)  // used in quests
        {
            Enemy e = new Enemy("Rouge Drone", lvl);

            e.userObj.Speed = 5 + (lvl * 2);
            e.userObj.Intellect = 5 + (lvl * 2);
            e.userObj.Luck = 10;
            e.userObj.Credits = lvl * 2;

            e.userObj.FreeAP = 0;
            e.userObj.Inventory = new List<Item>()
            {
                new Item().Bandage(),
                new Item().Scanner()
            };
            return e;
        }
    }
}