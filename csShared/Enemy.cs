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
        /// <summary>
        /// Creates a deep copy of the current enemy with all stats preserved. Useful for creating independent enemy instances for battles.
        /// </summary>
        /// <returns>A cloned Enemy object with identical properties.</returns>
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
        /// <summary>
        /// RandomizeStats is a method that takes an Enemy object and two boolean parameters to determine whether to randomize the enemy's luck and credits. 
        /// It creates a new Enemy object based on the provided one and randomizes its speed, intellect, luck (if randLuck is true), and credits (if randCredits is true) 
        /// using a Game.Randomize method. The method then returns the modified Enemy object with the randomized stats.
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="randLuck"></param>
        /// <param name="randCredits"></param>
        /// <returns></returns>
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
        /// <summary>
        /// ReviveEnemy is a method that takes an Enemy object as a parameter and resets its HP to 100, effectively reviving the enemy. 
        /// It also updates the HP of the associated userObj to 100 to ensure consistency between the enemy's health and its user representation. 
        /// This method can be used to bring an enemy back to life after it has been defeated in combat.
        /// </summary>
        /// <param name="enemy"></param>
        public static void ReviveEnemy(Enemy enemy)
        {
            enemy.HP = 100;
            enemy.userObj.Hp = 100;
        }
        /// <summary>
        /// ReviveEnemies is a method that takes a list of Enemy objects and iterates through each enemy in the list, calling the ReviveEnemy method on each one.
        /// </summary>
        /// <param name="enemies"></param>
        public static void ReviveEnemies(List<Enemy> enemies)
        {
            foreach (var enemy in enemies)
            {
                ReviveEnemy(enemy);
            }
        }
        /// <summary>
        /// ReviveEnemiesIfDead is a method that takes a list of Enemy objects and iterates through each enemy in the list. For each enemy,
        /// it checks if the enemy's HP is less than or equal to 0, indicating that the enemy is dead. If the enemy is dead, it calls the ReviveEnemy 
        /// method to reset its HP and revive it. This method allows for selectively reviving only those enemies that are currently defeated, rather than 
        /// reviving all enemies in the list regardless of their current state.
        /// </summary>
        /// <param name="enemies"></param>
        public static void ReviveEnemiesIfDead(List<Enemy> enemies)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.userObj.Hp <= 0) ReviveEnemy(enemy);
            }
        }
        /// <summary>
        /// ReviveEnemiesInWorld is a method that takes a list of Location objects representing the game world and iterates through each location. For each location, it calls the
        /// </summary>
        /// <param name="world"></param>
        public static void ReviveEnemiesInWorld(List<Location> world)
        {
            foreach (var location in world)
            {
                ReviveEnemies(location.Enemies);
            }
        }
        /// <summary>
        /// ReviveDeadEnemiesInWorld is a method that takes a list of Location objects representing the game world and iterates through each location. For each location, it calls the
        /// ReviveEnemiesIfDead method to selectively revive only those enemies that are currently defeated (HP <= 0) within that location. This allows for maintaining the 
        /// state of alive enemies while reviving only those that have been defeated across the entire game world.
        /// </summary>
        /// <param name="world"></param>
        public static void ReviveDeadEnemiesInWorld(List<Location> world)
        {
            foreach (var location in world)
            {
                ReviveEnemiesIfDead(location.Enemies);
            }
        }
        /// <summary>
        /// ResetEnemyLvlUserObj is a method that takes an Enemy object as a parameter and resets the level of the enemy's associated userObj to match the enemy's current level.
        /// </summary>
        /// <param name="enemy"></param>
        public static void ResetEnemyLvlUserObj(Enemy enemy)
        {
            enemy.userObj.Level = enemy.Level;
            enemy.userObj.Xp = 0;
        }
        /// <summary>
        /// ResetEnemyLvlUserObj is a method that takes a list of Enemy objects as a parameter and iterates through each enemy in the list, calling the 
        /// ResetEnemyLvlUserObj method on each one to reset their associated userObj levels to match their current levels.
        /// </summary>
        /// <param name="enemies"></param>
        public static void ResetEnemyLvlUserObj(List<Enemy> enemies)
        {
            foreach (var enemy in enemies)
            {
                ResetEnemyLvlUserObj(enemy);
            }
        }
        /// <summary>
        /// ResetEnemyLvlUserObjInWorld is a method that takes a list of Location objects representing the game world and iterates through each location. For each location, it calls the
        /// ResetEnemyLvlUserObj method to reset the levels of all enemies' associated userObjs within that location to match their current levels. This ensures that the enemy's user representation is consistent with the enemy's actual level across the entire game world.
        /// </summary>
        /// <param name="world"></param>
        public static void ResetEnemyLvlUserObjInWorld(List<Location> world)
        {
            foreach (var location in world)
            {
                ResetEnemyLvlUserObj(location.Enemies);
            }
        }
        /// <summary>
        /// Creates a boss enemy (Drone Mother) with enhanced stats and special loot. The boss has higher health and a separate inventory.
        /// </summary>
        /// <param name="lvl">The level of the boss enemy.</param>
        /// <returns>A configured boss enemy with randomized stats and special equipment.</returns>
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
        /// <summary>
        /// Creates a static (non-randomized) boss enemy (Drone Mother) with fixed stats for consistent encounters.
        /// </summary>
        /// <param name="lvl">The level of the boss enemy.</param>
        /// <returns>A configured boss enemy with static stats and special equipment.</returns>
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
        /// <summary>
        /// Creates a standard enemy (Rouge Drone) with randomized stats, equipment, and loot drops.
        /// </summary>
        /// <param name="lvl">The level of the rouge drone enemy.</param>
        /// <returns>A configured rouge drone enemy with randomized stats and starting inventory.</returns>
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
    }
}