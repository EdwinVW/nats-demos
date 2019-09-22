using System;
using System.Text;
using System.Threading.Tasks;
using NATS.Client;

namespace consumer
{
    class Program
    {
        private static bool _exit = false;
        private static IConnection _connection;

        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();
            _connection = factory.CreateConnection();
            SubscribeInit();
            Task.Run(() => SubscribePubSub());
            SubscribeQueueGroups();
            SubscribeRequestResponse();
            SubscribeWildcards("nats.*.wildcards");
            SubscribeWildcards("nats.demo.wildcards.*");
            SubscribeWildcards("nats.demo.wildcards.>");

            Console.Clear();
            System.Console.WriteLine("Consumers started");
            Console.ReadKey(true);
            _exit = true;
            _connection.Close();
        }

        private static void SubscribeInit()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                Console.Clear();
            };

            IAsyncSubscription s = 
                _connection.SubscribeAsync("nats.demo.init", handler);

            s.Start();
        }

        private static void SubscribePubSub()
        {
            ISyncSubscription sub = _connection.SubscribeSync("nats.demo.pubsub");

            while (!_exit)
            {
                try
                {
                    var message = sub.NextMessage(5000);
                    string data = Encoding.UTF8.GetString(message.Data);
                    Console.WriteLine(message);
                }
                catch (TimeoutException)
                {
                    // do nothing, keep loop alive
                }
            }
        }

        private static void SubscribeQueueGroups()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                Console.WriteLine(data);
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.queuegroups", "load-balancing-queue", handler);

            s.Start();
        }

        private static void SubscribeRequestResponse()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                Console.WriteLine(data);

                string replySubject = args.Message.Reply;
                if (replySubject != null)
                {
                    byte[] responseData = Encoding.UTF8.GetBytes($"ACK for {data}");
                    _connection.Publish(replySubject, responseData);
                }
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.requestresponse", "request-response-queue", handler);
            
            s.Start();
        }

        private static void SubscribeWildcards(string subject)
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                Console.WriteLine($"{data} (received on subject {subject})");
            };

            IAsyncSubscription s = _connection.SubscribeAsync(subject, handler);

            s.Start();
        }
    }
}
