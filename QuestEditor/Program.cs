using SocketServer;

namespace QuestEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--------");
            Quest q = new Quest("test", "test 123", 1)
            {
                Steps = new List<QuestStep>()
            {
                new QuestStep()
                {
                    Text = "hello test 1"
                },
                new QuestStep()
                {
                    Text = "hello test 2"
                },
                new QuestStep()
                {
                    Text = "hello test 3",
                    Enemies = new List<Enemy>()
                    {
                        new Enemy("test emeny", 2),
                        new Enemy("",1).RougeDrone(1)
                 },
                    Items = new List<Item>(){
                        new Item().Bandage()
                    }
}
            }
            };

            q.SaveToJsonFile(q);

            q = new Quest("", "", 1).Introduction();
            q.SaveToJsonFile(q);
        }
    }
}

