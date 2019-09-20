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
            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> h = (sender, args) =>
                {
                    Console.Clear();
                };

                IAsyncSubscription s = c.SubscribeAsync("nats.demo.init", h);
                s.Start();
                
                Wait();
            }
        }

        private static void SubscribePubSub()
        {
            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                ISyncSubscription sub = c.SubscribeSync("nats.demo.pubsub");

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
            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> h = (sender, args) =>
                {
                    string data = Encoding.UTF8.GetString(args.Message.Data);
                    Console.WriteLine(data);
                };

                IAsyncSubscription s = c.SubscribeAsync("nats.demo.queuegroups", "load-balancing-queue", h);
                s.Start();

                Wait();
            }
        }

        private static void SubscribeRequestResponse()
        {
            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> h = (sender, args) =>
                {
                    string data = Encoding.UTF8.GetString(args.Message.Data);
                    Console.WriteLine(data);

                    byte[] responseData = Encoding.UTF8.GetBytes($"ACK for {data}");
                    c.Publish(args.Message.Reply, responseData);
                };

                IAsyncSubscription s = c.SubscribeAsync("nats.demo.requestresponse", "request-response-queue", h);
                s.Start();

                Wait();
            }
        }

        private static void SubscribeWildcards(string subject)
        {
            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                EventHandler<MsgHandlerEventArgs> h = (sender, args) =>
                {
                    string data = Encoding.UTF8.GetString(args.Message.Data);
                    Console.WriteLine($"{data} (received on subject {subject})");
                };

                IAsyncSubscription s = c.SubscribeAsync(subject, h);
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
