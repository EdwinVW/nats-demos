using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;

namespace producer
{
    class Program
    {
        private static int _messageCount = 25;
        private static int _sendIntervalMs = 100;
        private const string ALLOWED_OPTIONS = "123456qQ";

        private static IConnection _connection;

        static void Main(string[] args)
        {
            bool exit = false;

            using (_connection = ConnectToNats())
            {
                while (!exit)
                {
                    Console.Clear();

                    Console.WriteLine("NATS demo producer");
                    Console.WriteLine("==================");
                    Console.WriteLine("Select mode:");
                    Console.WriteLine("1) Pub / Sub");
                    Console.WriteLine("2) Load-balancing (queue groups)");
                    Console.WriteLine("3) Request / Response (explicit)");
                    Console.WriteLine("4) Request / Response (implicit)");
                    Console.WriteLine("5) Wildcards");
                    Console.WriteLine("6) Continuous pub/sub");
                    Console.WriteLine("q) Quit");

                    // get input
                    ConsoleKeyInfo input;
                    do
                    {
                        input = Console.ReadKey(true);
                    } while (!ALLOWED_OPTIONS.Contains(input.KeyChar));

                    switch (input.KeyChar)
                    {
                        case '1':
                            PubSub();
                            break;
                        case '2':
                            QueueGroups();
                            break;
                        case '3':
                            RequestResponseExplicit();
                            break;
                        case '4':
                            RequestResponseImplicit();
                            break;
                        case '5':
                            Wildcards();
                            break;
                        case '6':
                            ContinuousPubSub();
                            break;
                        case 'q':
                        case 'Q':
                            exit = true;
                            continue;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Done. Press any key to continue...");
                    Console.ReadKey(true);
                    Clear();
                }

                _connection.Drain(5000);
            }
        }

        private static IConnection ConnectToNats()
        {
            ConnectionFactory factory = new ConnectionFactory();

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = "nats://localhost:4222";
            
            return factory.CreateConnection(options);
        }

        private static void PubSub()
        {
            Console.Clear();
            Console.WriteLine("Pub/Sub demo");
            Console.WriteLine("============");

            for (int i = 1; i <= _messageCount; i++)
            {
                string message = $"Message {i}";

                Console.WriteLine($"Sending: {message}");

                byte[] data = Encoding.UTF8.GetBytes(message);

                _connection.Publish("nats.demo.pubsub", data);

                Thread.Sleep(_sendIntervalMs);
            }
        }

        private static void QueueGroups()
        {
            Console.Clear();
            Console.WriteLine("Load-balancing demo");
            Console.WriteLine("===================");

            for (int i = 1; i <= _messageCount; i++)
            {
                string message = $"Message {i}";

                Console.WriteLine($"Sending: {message}");

                byte[] data = Encoding.UTF8.GetBytes(message);

                _connection.Publish("nats.demo.queuegroups", data);

                Thread.Sleep(_sendIntervalMs);
            }
        }

        private static void RequestResponseExplicit()
        {
            Console.Clear();
            Console.WriteLine("Request/Response (explicit) demo");
            Console.WriteLine("================================");

            for (int i = 1; i <= _messageCount; i++)
            {
                string replySubject = $"_INBOX.{Guid.NewGuid().ToString("N")}";
                ISyncSubscription subscription = _connection.SubscribeSync(replySubject);
                subscription.AutoUnsubscribe(1);

                // client also has a convenience-method to do this in line:
                //string replySubject = conn.NewInbox();

                string message = $"Message {i}";

                Console.WriteLine($"Sending: {message}");

                // send with reply subject
                byte[] data = Encoding.UTF8.GetBytes(message);

                _connection.Publish("nats.demo.requestresponse", replySubject, data);

                // wait for response in reply subject
                var response = subscription.NextMessage(5000);

                string responseMsg = Encoding.UTF8.GetString(response.Data);
                Console.WriteLine($"Response: {responseMsg}");

                Thread.Sleep(_sendIntervalMs);
            }
        }

        private static void RequestResponseImplicit()
        {
            Console.Clear();
            Console.WriteLine("Request/Response (implicit) demo");
            Console.WriteLine("================================");

            for (int i = 1; i <= _messageCount; i++)
            {
                string message = $"Message {i}";

                Console.WriteLine($"Sending: {message}");

                byte[] data = Encoding.UTF8.GetBytes(message);

                var response = _connection.Request("nats.demo.requestresponse", data, 5000);

                var responseMsg = Encoding.UTF8.GetString(response.Data);

                Console.WriteLine($"Response: {responseMsg}");

                Thread.Sleep(_sendIntervalMs);
            }
        }

        private static void Wildcards()
        {
            Console.Clear();
            Console.WriteLine("Wildcards demo");
            Console.WriteLine("==============");

            Console.WriteLine("Available subjects:");
            Console.WriteLine("- nats.*.wildcards");
            Console.WriteLine("- nats.demo.wildcards.*");
            Console.WriteLine("- nats.demo.wildcards.>");

            int messageCounter = 1;
            while (true)
            {
                Console.Write("\nSubject: ");
                string subject = Console.ReadLine();
                if (string.IsNullOrEmpty(subject))
                {
                    return;
                }

                string message = $"Message {messageCounter++}";

                Console.WriteLine($"Sending: {message} to {subject}");

                byte[] data = Encoding.UTF8.GetBytes(message);

                _connection.Publish(subject, data);
            }
        }

        private static void ContinuousPubSub()
        {
            Console.Clear();
            Console.WriteLine("Continuous Pub/Sub demo");
            Console.WriteLine("=======================");

            CancellationTokenSource cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var task = Task.Run(() =>
            {
                int messageCounter = 1;
                while (!cancellationToken.IsCancellationRequested)
                {
                    string message = $"Message {messageCounter++}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _connection.Publish("nats.demo.pubsub", data);
                    Thread.Sleep(_sendIntervalMs);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }, cancellationToken);

            Console.WriteLine("Started sending messages. Press any key to stop.");
            Console.ReadKey();
            cts.Cancel();
        }

        private static void Clear()
        {
            Console.Clear();
            _connection.Publish("nats.demo.clear", null);
        }
    }
}
