using System;
using System.Text;
using System.Threading;
using NATS.Client;

namespace producer
{
    class Program
    {
        private const int MESSAGE_COUNT = 25;
        private const int SEND_INTERVAL = 500;
        private const string ALLOWED_OPTIONS = "0123456789qQ";

        static void Main(string[] args)
        {
            bool exit = false;
            
            while (!exit)
            {
                Console.Clear();

                Console.WriteLine("NATS demo producer");
                Console.WriteLine("==================");
                Console.WriteLine("Select mode:");
                Console.WriteLine("0) Pub / Sub");
                Console.WriteLine("1) Load-balancing (queue groups)");
                Console.WriteLine("2) Request / Response (explicit)");
                Console.WriteLine("3) Request / Response (implicit)");
                Console.WriteLine("4) Wildcards");
                Console.WriteLine("q) Quit");

                // get input
                ConsoleKeyInfo input;
                do
                {
                    input = Console.ReadKey(true);
                } while (!ALLOWED_OPTIONS.Contains(input.KeyChar));

                Console.Clear();
                InitializeSubscribers();

                switch (input.KeyChar)
                {
                    case '0':
                        PubSub();
                        break;
                    case '1':
                        QueueGroups();
                        break;
                    case '2':
                        RequestResponseExplicit();
                        break;
                    case '3':
                        RequestResponseImplicit();
                        break;
                    case '4':
                        Wildcards();
                        break;
                    case 'q':
                    case 'Q':
                        exit = true;
                        continue;
                }

                Console.WriteLine();
                Console.WriteLine("Done. Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        private static void InitializeSubscribers()
        {
            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                c.Publish("nats.demo.init", null);
                c.Flush();
            }
        }

        private static void PubSub()
        {
            Console.WriteLine("Pub/Sub demo");
            Console.WriteLine("============");

            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                for (int i = 1; i <= MESSAGE_COUNT; i++)
                {
                    string message = $"Message {i}";

                    Console.WriteLine($"Sending: {message}");

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    c.Publish("nats.demo.pubsub", data);
                    
                    Thread.Sleep(SEND_INTERVAL);
                }
                c.Flush();
            }
        }

        private static void QueueGroups()
        {
            Console.WriteLine("Load-balancing demo");
            Console.WriteLine("===================");

            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                for (int i = 1; i <= MESSAGE_COUNT; i++)
                {
                    string message = $"Message {i}";

                    Console.WriteLine($"Sending: {message}");

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    c.Publish("nats.demo.queuegroups", data);
                    
                    Thread.Sleep(SEND_INTERVAL);
                }
            }
        }

        private static void RequestResponseExplicit()
        {
            Console.WriteLine("Request/Response (explicit) demo");
            Console.WriteLine("================================");

            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                string replySubject = Guid.NewGuid().ToString();
                ISyncSubscription subscription = c.SubscribeSync(replySubject);

                for (int i = 1; i <= MESSAGE_COUNT; i++)
                {
                    string message = $"Message {i}";

                    Console.WriteLine($"Sending: {message}");

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    c.Publish("nats.demo.requestresponse", replySubject, data);
                    var response = subscription.NextMessage(1000);

                    string responseMsg = Encoding.UTF8.GetString(response.Data);
                    Console.WriteLine($"Response: {responseMsg}");
                    
                    Thread.Sleep(SEND_INTERVAL);
                }
                c.Flush();
            }
        }

        private static void RequestResponseImplicit()
        {
            Console.WriteLine("Request/Response (implicit) demo");
            Console.WriteLine("================================");

            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                for (int i = 1; i <= MESSAGE_COUNT; i++)
                {
                    string message = $"Message {i}";

                    Console.WriteLine($"Sending: {message}");
                    
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    var response = c.Request("nats.demo.requestresponse", data);
                    var responseMsg = Encoding.UTF8.GetString(response.Data);

                    Console.WriteLine($"Response: {responseMsg}");
                    
                    Thread.Sleep(SEND_INTERVAL);
                }
                c.Flush();
            }
        }

        private static void Wildcards()
        {
            Console.WriteLine("Wildcards demo");
            Console.WriteLine("==============");

            Console.WriteLine("Available subjects:");
            Console.WriteLine("- nats.*.wildcards");
            Console.WriteLine("- nats.demo.wildcards.*");
            Console.WriteLine("- nats.demo.wildcards.>");

            using (IConnection c = new ConnectionFactory().CreateConnection())
            {
                while (true)
                {
                    Console.Write("\nSubject: ");
                    string subject = Console.ReadLine();
                    if (string.IsNullOrEmpty(subject))
                    {
                        return;
                    }

                    string message = DateTime.Now.ToString("hh:mm:ss");

                    Console.WriteLine($"Sending: {message} to {subject}");
                    
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    c.Publish(subject, data);

                    c.Flush();
                }
            }
        }        
    }
}
