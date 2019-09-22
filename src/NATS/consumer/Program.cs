using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;

namespace consumer
{
    class Program
    {
        private static bool _exit = false;

        static void Main(string[] args)
        {
            var tasks = new List<Task>();
            Task.Run(SubscribeInit);
            Task.Run(SubscribePubSub);
            Task.Run(SubscribeQueueGroups);
            Task.Run(SubscribeRequestResponse);
            Task.Run(() => SubscribeWildcards("nats.*.wildcards"));
            Task.Run(() => SubscribeWildcards("nats.demo.wildcards.*"));
            Task.Run(() => SubscribeWildcards("nats.demo.wildcards.>"));

            Console.Clear();
            System.Console.WriteLine("Consumers started");
            Console.ReadKey(true);
            _exit = true;
        }

        private static void SubscribeInit()
        {
            ConnectionFactory factory = new ConnectionFactory();
            using (IConnection conn = factory.CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
                {
                    Console.Clear();
                };

                IAsyncSubscription s = 
                    conn.SubscribeAsync("nats.demo.init", handler);

                s.Start();
                
                Wait();
            }
        }

        private static void SubscribePubSub()
        {
            ConnectionFactory factory = new ConnectionFactory();
            using (IConnection conn = factory.CreateConnection())
            {
                ISyncSubscription sub = conn.SubscribeSync("nats.demo.pubsub");

                while (!_exit)
                {
                    var message = sub.NextMessage();
                    
                    string data = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine(message);
                }
            }
        }

        private static void SubscribeQueueGroups()
        {
            ConnectionFactory factory = new ConnectionFactory();
            using (IConnection conn = factory.CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
                {
                    string data = Encoding.UTF8.GetString(args.Message.Data);
                    Console.WriteLine(data);
                };

                IAsyncSubscription s = conn.SubscribeAsync(
                    "nats.demo.queuegroups", "load-balancing-queue", handler);

                s.Start();

                Wait();
            }
        }

        private static void SubscribeRequestResponse()
        {
            ConnectionFactory factory = new ConnectionFactory();
            using (IConnection conn = factory.CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
                {
                    string data = Encoding.UTF8.GetString(args.Message.Data);
                    Console.WriteLine(data);

                    byte[] responseData = Encoding.UTF8.GetBytes($"ACK for {data}");
                    conn.Publish(args.Message.Reply, responseData);
                };

                IAsyncSubscription s = conn.SubscribeAsync(
                    "nats.demo.requestresponse", "request-response-queue", handler);
                
                s.Start();

                Wait();
            }
        }

        private static void SubscribeWildcards(string subject)
        {
            ConnectionFactory factory = new ConnectionFactory();
            using (IConnection conn = factory.CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
                {
                    string data = Encoding.UTF8.GetString(args.Message.Data);
                    Console.WriteLine($"{data} (received on subject {subject})");
                };

                IAsyncSubscription s = conn.SubscribeAsync(subject, handler);

                s.Start();
            }
        }

        private static void Wait()
        {
            while(!_exit)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
