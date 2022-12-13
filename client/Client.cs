using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ermetic.Client
{
    public interface IClient
    {
        void Start(int id);
        void Stop();
    }

    public class Client : IClient
    {
        private bool _running = true;

        private const string URL = "http://localhost:8080/?clientId={0}";
        private static readonly HttpClient client = new HttpClient();

        public void Start(int id)
        {
            Task.Run(async () =>
            {
                while (_running)
                {
                    Console.WriteLine($"client {id} is sending a request");

                    var response = await client.GetAsync(string.Format(URL, id));

                    Console.WriteLine($"client {id} got {response.StatusCode}");

                    var random = new Random();
                    int timeToWait = random.Next(0, 10);

                    Console.WriteLine($"client {id} is waiting for {timeToWait} seconds");
                    
                    Thread.Sleep(timeToWait * 1000);
                }
            });
        }

        public void Stop()
        {
            _running = false;
        }
    }
}
