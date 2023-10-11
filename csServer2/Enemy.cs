using System;
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
        public User userObj;
        public string Name { get; set; }
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
        public int HP { get; set; }
        public int Speed { get; set; }
        public int Intellect { get; set; }
        public int Luck { get; set; }
        public int Credits { get; set; }
        public Enemy(string name, int level)
        {
            Name = name;
            Level = level;

            HP = 100;
            Speed = Game.Randomize(5 + (level * 2));
            Intellect = Game.Randomize(5 + (level * 2));
            Luck = 10;
            Credits = Game.Randomize(level * 2);

            userObj = new User(name, "")
            {
                //userObj.Name = name;
                Class = "Mob",
                Level = level,
                Hp = HP,
                Speed = Speed,
                Intellect = Intellect,
                Luck = Luck,
                Credits = Credits,
            };
        }
        public Enemy DroneMother(int lvl)
        {
            Enemy e = new Enemy("Rouge Drone Mother", lvl);
            e.userObj.Class = "Boss";
            e.userObj.AddItemToInventory(new Item().Boots());
            e.userObj.AddItemToInventory(new Item().Glasses());
            e.userObj.AddItemToInventory(new Item().Scanner());
            return e;
        }
        public Enemy RougeDrone(int lvl)
        {
            Enemy e = new Enemy("Rouge Drone", lvl);
            e.userObj.FreeAP = 0;
            e.userObj.AddItemToInventory(new Item().Bandage());
            e.userObj.AddItemToInventory(new Item().Drink());
            return e;
        }
    }
}