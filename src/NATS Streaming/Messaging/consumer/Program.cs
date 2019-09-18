using System;
using System.Text;
using STAN.Client;

namespace consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string clientId = args[0];

            var cf = new StanConnectionFactory();
            using (var c = cf.CreateConnection("test-cluster", clientId))
            {
                var opts = StanSubscriptionOptions.GetDefaultOptions();

                //opts.DeliverAllAvailable();
                //opts.StartAt(15);
                //opts.StartAt(TimeSpan.FromSeconds(10));
                //opts.StartAt(new DateTime(2019, 9, 3, 9, 22, 0));
                //opts.StartWithLastReceived();
                //opts.DurableName = "durable";

                var s = c.Subscribe("nats.streaming.demo", opts, (obj, args) =>
                {
                    string message = Encoding.UTF8.GetString(args.Message.Data);
                    Console.WriteLine($"[#{args.Message.Sequence}] {message}");
                });

                Console.WriteLine($"Consumer with client id '{clientId}' started. Press any key to quit...");
                Console.ReadKey(true);

                //s.Unsubscribe();
                c.Close();
            }
        }
    }
}
