using System;
using System.Text;
using STAN.Client;

namespace BasicMessaging.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string clientId = $"producer-{Guid.NewGuid().ToString()}";

            var cf = new StanConnectionFactory();
            StanOptions options = StanOptions.GetDefaultOptions();
            options.NatsURL = "nats://localhost:4223";

            using (var c = cf.CreateConnection("test-cluster", clientId, options))
            {
                for (int i = 1; i <= 25; i++)
                {
                    string message = $"[{DateTime.Now.ToString("hh:mm:ss:fffffff")}] Message {i}";
                    Console.WriteLine($"Sending {message}");
                    
                    c.Publish("nats.streaming.demo", Encoding.UTF8.GetBytes(message));
                }
            }
        }
    }
}
