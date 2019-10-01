using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;

namespace MultiUserSimulator
{
    public class UserSimulation
    {
        private readonly int _simulationId;
        private readonly Random _random;
        private static IConnection _natsConnection;

        public UserSimulation(int simulationId, IConnection connection)
        {
            _simulationId = simulationId;
            _random = new Random(DateTime.Now.Millisecond);
            _natsConnection = connection;
        }

        public Task Start(CancellationToken cancellationToken)
        {
            return Task.Run(() => SimulationWorker(cancellationToken));
        }

        private void SimulationWorker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                WaitRandom();

                string orderNumber = $"{_simulationId}{_random.Next(1000, 9999)}";
                CreateOrder(orderNumber);
                WaitRandom();

                for (int i = 0; i < _random.Next(3, 15); i++)
                {
                    OrderProducts(orderNumber);
                    WaitRandom();
                }

                for (int i = 0; i < _random.Next(0, 2); i++)
                {
                    RemoveProducts(orderNumber);
                    WaitRandom();
                }

                FinalizeOrder(orderNumber);
            }
        }

        private void CreateOrder(string orderNumber)
        {
            Console.WriteLine($"Create order #{orderNumber}");

            string messageType = "CreateOrder";
            string message = $"{orderNumber}";
            string subject = $"store.commands.{messageType}";
            _natsConnection.Publish(subject, Encoding.UTF8.GetBytes(message));
        }

        private void OrderProducts(string orderNumber)
        {
            int productNumber = _random.Next(1, 7);

            Console.WriteLine($"Order product #{productNumber} for order #{orderNumber}");

            string messageType = "OrderProduct";
            string message = $"{orderNumber}|{productNumber}";
            string subject = $"store.commands.{messageType}";
            _natsConnection.Publish(subject, Encoding.UTF8.GetBytes(message));
        }

        private void RemoveProducts(string orderNumber)
        {
            int productNumber = _random.Next(1, 7);

            Console.WriteLine($"Remove product #{productNumber} from order #{orderNumber}");

            string messageType = "RemoveProduct";
            string message = $"{orderNumber}|{productNumber}";
            string subject = $"store.commands.{messageType}";
            _natsConnection.Publish(subject, Encoding.UTF8.GetBytes(message));
        }

        private void FinalizeOrder(string orderNumber)
        {
            if (_random.Next(10) <= 8)
            {
                Console.WriteLine($"Complete order #{orderNumber}");

                string shippingAddress =
                    $"Mainstreet {_random.Next(120, 220)}, {_random.Next(1011, 1019)} AS, Amsterdam";
                string messageType = "CompleteOrder";
                string message = $"{orderNumber}|{shippingAddress}";
                string subject = $"store.commands.{messageType}";
                _natsConnection.Publish(subject, Encoding.UTF8.GetBytes(message));

                Console.WriteLine($"Ship order #{orderNumber}");

                messageType = "ShipOrder";
                message = $"{orderNumber}";
                subject = $"store.commands.{messageType}";
                _natsConnection.Publish(subject, Encoding.UTF8.GetBytes(message));
            }
            else
            {
                Console.WriteLine($"Cancel order #{orderNumber}");

                string messageType = "CancelOrder";
                string message = $"{orderNumber}";
                string subject = $"store.commands.{messageType}";
                _natsConnection.Publish(subject, Encoding.UTF8.GetBytes(message));
            }
        }

        private void WaitRandom()
        {
            Thread.Sleep(_random.Next(100, 1000));
        }
    }
}