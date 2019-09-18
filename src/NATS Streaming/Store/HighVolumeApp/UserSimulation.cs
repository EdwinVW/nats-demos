using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Store.Messaging;

namespace HighVolumeApp
{
    public class UserSimulation
    {
        private readonly int _simulationId;
        private readonly Random _random;
        private static NATSMessageBroker _messageBroker;

        public UserSimulation(int simulationId)
        {
            _simulationId = simulationId;
            _random = new Random(DateTime.Now.Millisecond);
            _messageBroker = new NATSMessageBroker("nats://localhost:4222");
        }

        public Task Start(CancellationToken cancellationToken)
        {
            return Task.Run(() => SimulationWorker(cancellationToken));
        }

        private void SimulationWorker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string orderNumber = $"{_simulationId}{_random.Next(1000, 9999)}";
                CreateOrder(orderNumber);
                for (int i = 0; i < _random.Next(3, 15); i++)
                {
                    OrderProducts(orderNumber);
                }
                for (int i = 0; i < _random.Next(0, 2); i++)
                {
                    RemoveProducts(orderNumber);
                }
                FinalizeOrder(orderNumber);
            }
        }

        private void CreateOrder(string orderNumber)
        {
            Console.WriteLine($"Create order #{orderNumber}");

            string messageType = "CreateOrder";
            string message = $"{orderNumber}";
            _messageBroker.Publish("store.commands", messageType, message);
        }

        private void OrderProducts(string orderNumber)
        {
            int productNumber = _random.Next(1, 7);

            Console.WriteLine($"Order product #{productNumber} for order #{orderNumber}");

            string messageType = "OrderProduct";
            string message = $"{orderNumber}|{productNumber}";
            _messageBroker.Publish("store.commands", messageType, message);
        }

        private void RemoveProducts(string orderNumber)
        {
            int productNumber = _random.Next(1, 7);

            Console.WriteLine($"Remove product #{productNumber} from order #{orderNumber}");

            string messageType = "RemoveProduct";
            string message = $"{orderNumber}|{productNumber}";
            _messageBroker.Publish("store.commands", messageType, message);
        }

        private void FinalizeOrder(string orderNumber)
        {
            if (_random.Next(10) <= 8)
            {
                Console.WriteLine($"Complete order #{orderNumber}");

                string shippingAddress = $"Mainstreet {_random.Next(120, 220)}, {_random.Next(1011, 1019)} AS, Amsterdam";
                string messageType = "CompleteOrder";
                string message = $"{orderNumber}|{shippingAddress}";
                _messageBroker.Publish("store.commands", messageType, message);

                Console.WriteLine($"Ship order #{orderNumber}");

                messageType = "ShipOrder";
                message = $"{orderNumber}";
                _messageBroker.Publish("store.commands", messageType, message);

            }
            else
            {
                Console.WriteLine($"Cancel order #{orderNumber}");

                string messageType = "CancelOrder";
                string message = $"{orderNumber}";
                _messageBroker.Publish("store.commands", messageType, message);
            }
        }
    }
}