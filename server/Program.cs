


namespace Ermetic.Server
{
    public class Excersize
    {
        public static void Main(string[] args)
        {
            IServer server = new Server();
            server.Start();

            do
            {
                while (!Console.KeyAvailable)
                {
                    //just prevent over-using the cpu
                    Thread.Sleep(500);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

            server.Stop();
        }
    }
}