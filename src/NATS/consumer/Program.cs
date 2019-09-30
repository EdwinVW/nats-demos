﻿using System;
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
            using (_connection = ConnectToNatsCluster())
            {
                SubscribePubSub();
                SubscribeQueueGroups();
                SubscribeRequestResponse();
                SubscribeWildcards("nats.*.wildcards");
                SubscribeWildcards("nats.demo.wildcards.*");
                SubscribeWildcards("nats.demo.wildcards.>");
                SubscribeClear();

                Console.Clear();
                Console.WriteLine($"Connected to {_connection.ConnectedUrl}.");
                Console.WriteLine("Consumers started");
                Console.ReadKey(true);
                _exit = true;

                _connection.Drain(5000);
            }
        }

        private static IConnection ConnectToNatsCluster()
        {
            ConnectionFactory factory = new ConnectionFactory();
            var options = ConnectionFactory.GetDefaultOptions();
            options.Servers = new string[] {
                "nats://localhost:4222",
                "nats://localhost:4223",
                "nats://localhost:4224"
            };

            options.AllowReconnect = true;
            options.ReconnectWait = 0;
            options.MaxReconnect = Options.ReconnectForever;

            options.DisconnectedEventHandler +=
                (sender, args) => Console.WriteLine($"Client disconnected!!");

            options.ReconnectedEventHandler +=
                (sender, args) => Console.WriteLine($"Client reconnected to {args.Conn.ConnectedUrl}.");

            return factory.CreateConnection(options);
        }

        private static void SubscribePubSub()
        {
            Task.Run(() =>
            {
                ISyncSubscription sub = _connection.SubscribeSync("nats.demo.pubsub");
                while (!_exit)
                {
                    var message = sub.NextMessage();
                    if (message != null)
                    {
                        string data = Encoding.UTF8.GetString(message.Data);
                        LogMessage(data);
                    }
                }
            });
        }

        private static void SubscribeQueueGroups()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage(data);
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.queuegroups", "load-balancing-queue", handler);
        }

        private static void SubscribeRequestResponse()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage(data);

                string replySubject = args.Message.Reply;
                if (replySubject != null)
                {
                    byte[] responseData = Encoding.UTF8.GetBytes($"ACK for {data}");
                    _connection.Publish(replySubject, responseData);
                }
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.requestresponse", "request-response-queue", handler);
        }

        private static void SubscribeWildcards(string subject)
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage($"{data} (subject {subject})");
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                subject, handler);
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffffff")} - {message}");
        }

        private static void SubscribeClear()
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                Console.Clear();
            };

            IAsyncSubscription s = _connection.SubscribeAsync(
                "nats.demo.clear", handler);
        }
    }
}
