

namespace Ermetic.Client
{
    public class Excersize
    {
        public static void Main(string[] args)
        {
            int numOfClients;

            Console.WriteLine("please enter a single integer");

            do
            {
                string number = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(number))
                {
                    Console.WriteLine("please enter a single integer");
                    continue;
                }

                bool success = int.TryParse(number, out numOfClients);

                if (!success)
                {
                    Console.WriteLine("please enter a single integer");
                    continue;
                }

                break;
            } while (true);

            List < IClient > clients = new List<IClient>();

            for (int i = 0; i< numOfClients; i++)
            {
                IClient client = new Client();
                client.Start(i);

                clients.Add(client);
            }

            do
            {
                while (!Console.KeyAvailable)
                {
                    //just prevent over-using the cpu
                    Thread.Sleep(500);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

            foreach (var client in clients)
            {
                client.Stop();
            }

        }
    }
}