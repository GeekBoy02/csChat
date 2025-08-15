using SocketServer;

namespace QuestEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            // gen Cryo-Station
            Quest q = new Quest().Introduction();
            q.SaveToJsonFile("Cryo-Station", q);
            q.Level = 2;
            q = new Quest
            {
                Name = "Drone-Slaughter",
                Level = 2,
                XP_reward = 24,
                Credit_reward = 20,
                Prerequisite_lvl = q.Level,
                Prerequisite_int = 10,
                Description = "You need to kill 9 Rouge Drones and their Mother Drone",
                Steps = new List<QuestStep>()
                {
                    new QuestStep()
                    {
                        Enemies = new List<Enemy>()
                        {
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1),
                            new Enemy("placeholder",1).RougeDroneStatic(1)
                        }
                    },
                    new QuestStep()
                    {
                        Enemies = new List<Enemy>()
                        {
                            new Enemy("placeholder",1).DroneMotherStatic(6)
                        }
                    },
                    new QuestStep() { Items = new List<Item>(){ new Item().Drink()} }
                }
            };
            q.SaveToJsonFile("Cryo-Station", q);
            Location loc = new Location().CryoStation();
            loc.SaveToJsonFile(loc);

            // gen landing bay
            loc.Level = 2;
            loc = new Location()
            {
                Name = "Landing-Bay",
                Level = 2,
                Description = "Ships park here.",
                Enemies = new List<Enemy>
                {
                    new Enemy("", 1).RougeDrone(loc.Level),
                    new Enemy("", 1).DroneMother(loc.Level)
                },
                Shop = new List<Item>
                {
                    new Item().Glasses(),
                    new Item().Scanner(),
                    new Item().Boots()
                },
                Quests = Quest.LoadAllFromFolder("Landing-Bay")
            };
            loc.SaveToJsonFile(loc);
            q.SaveToJsonFile("Landing-Bay", q);

            // replace locations dir in csServer2
            if (Directory.Exists("../csServer2/locations"))
            {
                Directory.Delete("../csServer2/locations", true);
            }
            Directory.Move("locations", "../csServer2/locations");
        }
    }
}