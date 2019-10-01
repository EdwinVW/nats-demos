using System;
using System.Text;
using NATS.Client;

namespace Store.App
{
    public class BookStoreApp
    {
        private string VALID_INPUT = "1234567qQ";

        private IConnection _natsConnection;

        private string[] _products = new string[]
        {
            "Clean Code",
            "The Pragmatic Programmer",
            "DDD: Tackling complexity in the heart of the software",
            "Patterns of Enterprise Application Architecture",
            "Building Microservices",
            "Patterns, Principles and Practices od Domain Driven Design"
        };

        public void Start()
        {
            // connect to NATS
            var natsConnectionFactory = new ConnectionFactory();
            _natsConnection = natsConnectionFactory.CreateConnection("nats://localhost:4222");

            bool exit = false;
            while (!exit)
            {
                Console.Clear();

                Console.WriteLine("Store app");
                Console.WriteLine("=========");
                Console.WriteLine("Choose your activity:");
                Console.WriteLine("1) Create order");
                Console.WriteLine("2) Order product");
                Console.WriteLine("3) Remove product");
                Console.WriteLine("4) Complete order");
                Console.WriteLine("5) Ship order");
                Console.WriteLine("6) Cancel order");
                Console.WriteLine("7) Show orders overview");
                Console.WriteLine("Q) Quit");
                ConsoleKeyInfo input;
                do
                {
                    input = Console.ReadKey(true);
                } while (!VALID_INPUT.Contains(input.KeyChar));

                Console.Clear();

                try
                {
                    switch (input.KeyChar)
                    {
                        case '1':
                            CreateOrder();
                            break;
                        case '2':
                            OrderProduct();
                            break;
                        case '3':
                            RemoveProduct();
                            break;
                        case '4':
                            CompleteOrder();
                            break;
                        case '5':
                            ShipOrder();
                            break;
                        case '6':
                            CancelOrder();
                            break;
                        case '7':
                            ShowOrdersOverview();
                            break;
                        case 'q':
                        case 'Q':
                            exit = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                    Console.WriteLine("\nPress any key to return to the main menu.");
                    Console.ReadKey(true);
                }
            }
            _natsConnection.Close();
        }

        private void CreateOrder()
        {
            Console.WriteLine("Create a new order");
            Console.WriteLine("==================");
            Console.Write("Order-number: ");
            string orderNumber = Console.ReadLine();

            string messageType = "CreateOrder";
            string message = $"{orderNumber}";
            string subject = $"store.commands.{messageType}";
            var response = _natsConnection.Request(subject, Encoding.UTF8.GetBytes(message), 5000);
            Console.WriteLine(Encoding.UTF8.GetString(response.Data));

            Console.WriteLine("\nDone. Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        private void OrderProduct()
        {
            Console.WriteLine("Order a product");
            Console.WriteLine("===============");
            Console.Write("Order-number: ");
            string orderNumber = Console.ReadLine();
            ShowProductCatalog();
            Console.Write("Product-number: ");
            string productNumber = Console.ReadLine();

            string messageType = "OrderProduct";
            string message = $"{orderNumber}|{productNumber}";
            string subject = $"store.commands.{messageType}";
            var response = _natsConnection.Request(subject, Encoding.UTF8.GetBytes(message), 5000);
            Console.WriteLine(Encoding.UTF8.GetString(response.Data));

            Console.WriteLine("\nDone. Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        private void RemoveProduct()
        {
            Console.WriteLine("Remove a product");
            Console.WriteLine("================");
            Console.Write("Order-number: : ");
            string orderNumber = Console.ReadLine();
            ShowProductCatalog();
            Console.Write("Product-number: ");
            string productNumber = Console.ReadLine();

            string messageType = "RemoveProduct";
            string message = $"{orderNumber}|{productNumber}";
            string subject = $"store.commands.{messageType}";
            var response = _natsConnection.Request(subject, Encoding.UTF8.GetBytes(message), 5000);
            Console.WriteLine(Encoding.UTF8.GetString(response.Data));

            Console.WriteLine("\nDone. Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        private void CompleteOrder()
        {
            Console.WriteLine("Complete an order");
            Console.WriteLine("=================");
            Console.Write("Order-number: ");
            string orderNumber = Console.ReadLine();
            Console.Write("Shipping address: ");
            string shippingAddress = Console.ReadLine();

            string messageType = "CompleteOrder";
            string message = $"{orderNumber}|{shippingAddress}";
            string subject = $"store.commands.{messageType}";
            var response = _natsConnection.Request(subject, Encoding.UTF8.GetBytes(message), 5000);
            Console.WriteLine(Encoding.UTF8.GetString(response.Data));

            Console.WriteLine("\nDone. Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        private void ShipOrder()
        {
            Console.WriteLine("Ship an order");
            Console.WriteLine("=============");
            Console.Write("Order-number: ");
            string orderNumber = Console.ReadLine();

            string messageType = "ShipOrder";
            string message = $"{orderNumber}";
            string subject = $"store.commands.{messageType}";
            var response = _natsConnection.Request(subject, Encoding.UTF8.GetBytes(message), 5000);
            Console.WriteLine(Encoding.UTF8.GetString(response.Data));

            Console.WriteLine("\nDone. Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        private void CancelOrder()
        {
            Console.WriteLine("Cancel an order");
            Console.WriteLine("===============");
            Console.Write("Order-number: ");
            string orderNumber = Console.ReadLine();

            string messageType = "CancelOrder";
            string message = $"{orderNumber}";
            string subject = $"store.commands.{messageType}";
            var response = _natsConnection.Request(subject, Encoding.UTF8.GetBytes(message), 5000);
            Console.WriteLine(Encoding.UTF8.GetString(response.Data));

            Console.WriteLine("\nDone. Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        private void ShowOrdersOverview()
        {
            Console.WriteLine("Orders Overview");
            Console.WriteLine("===============");

            string messageType = "OrdersOverview";
            string subject = $"store.queries.{messageType}";
            var response = _natsConnection.Request(subject, new byte[0], 5000);
            Console.WriteLine(Encoding.UTF8.GetString(response.Data));

            Console.WriteLine("\nDone. Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        private void ShowProductCatalog()
        {
            Console.WriteLine("Product Catalog");
            Console.WriteLine("---------------");
            for (int i = 1; i < _products.Length; i++)
            {
                Console.WriteLine($"#{i} - {_products[i]}");
            }
        }
    }
}