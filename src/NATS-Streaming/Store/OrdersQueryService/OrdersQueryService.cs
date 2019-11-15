using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using NATS.Client;
using STAN.Client;
using Store.OrdersQueryService.Events;

namespace Store.OrdersQueryService
{
    public class OrdersQueryService
    {
        private IConnection _natsConnection;
        private IStanConnection _stanConnection;

        public void Start()
        {
            // connect to NATS
            var natsConnectionFactory = new ConnectionFactory();
            _natsConnection = natsConnectionFactory.CreateConnection("nats://localhost:4222");

            // connect to STAN
            var cf = new StanConnectionFactory();
            var natsOptions = StanOptions.GetDefaultOptions();
            natsOptions.NatsURL = "nats://localhost:4223";
            _stanConnection = cf.CreateConnection("test-cluster", "OrdersQueryService", natsOptions);

            // create queries subscription
            _natsConnection.SubscribeAsync("store.queries.*", QueryReceived);

            // create events subscription
            StanSubscriptionOptions stanOptions = StanSubscriptionOptions.GetDefaultOptions();
            stanOptions.DurableName = "OrdersQueryService";
            _stanConnection.Subscribe("store.events", stanOptions, EventReceived);
        }

        public void Stop()
        {
            _natsConnection.Close();
            _stanConnection.Close();
        }

        private void EventReceived(object sender, StanMsgHandlerArgs args)
        {
            try
            {
                // extract event-type and payload from the message
                // Event-type is embedded in the message:
                //   <event-type>#<value>|<value>|<value>
                string message = Encoding.UTF8.GetString(args.Message.Data);
                string eventTypeName = message.Split('#').First();
                string eventData = message.Substring(message.IndexOf('#') + 1);

                // determine event CLR type
                string fullEventTypeName = $"Store.OrdersQueryService.Events.{eventTypeName}";
                Type eventType = Type.GetType(fullEventTypeName, true);

                // Call event-handler
                dynamic e = JsonSerializer.Deserialize(eventData, eventType);
                Handle(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Error: {ex.InnerException.Message}");
                }
            }
        }

        private async void Handle(OrderCreated e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} created.");

            using (var dbContext = new StoreDBContext())
            {
                dbContext.Orders.Add(new Order { OrderNumber = e.OrderNumber, Status = "In progress" });
                await dbContext.SaveChangesAsync();
            }
        }

        private async void Handle(ProductOrdered e)
        {
            Console.WriteLine($"Product #{e.ProductNumber} added to order #{e.OrderNumber}.");

            using (var dbContext = new StoreDBContext())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);
                if (order != null)
                {
                    order.Products.Add(new OrderedProduct
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ProductNumber = e.ProductNumber,
                        Price = e.Price
                    });
                    order.TotalPrice += e.Price;
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private async void Handle(ProductRemoved e)
        {
            Console.WriteLine($"Product #{e.ProductNumber} removed from order #{e.OrderNumber}.");

            using (var dbContext = new StoreDBContext())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);
                if (order != null)
                {
                    dbContext.Entry(order).Collection(o => o.Products).Load();
                    var product = order.Products.FirstOrDefault(p => p.ProductNumber == e.ProductNumber);
                    if (product != null)
                    {
                        order.Products.Remove(product);
                        order.TotalPrice -= product.Price;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }

        private async void Handle(OrderCompleted e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} completed.");

            using (var dbContext = new StoreDBContext())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);
                if (order != null)
                {
                    order.ShippingAddress = e.ShippingAddress;
                    order.Status = "Completed";
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private async void Handle(OrderShipped e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} shipped.");

            using (var dbContext = new StoreDBContext())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);
                if (order != null)
                {
                    order.Status = "Shipped";
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private async void Handle(OrderCancelled e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} cancelled.");

            using (var dbContext = new StoreDBContext())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);
                if (order != null)
                {
                    dbContext.Orders.Remove(order);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private void QueryReceived(object sender, MsgHandlerEventArgs args)
        {
            Console.WriteLine("Query received.");

            // messageType is ignored for now - only 1 query supported

            try
            {
                using (var dbContext = new StoreDBContext())
                {
                    StringBuilder ordersList = new StringBuilder($"Order#\t| Status\t| Total amount\n");
                    ordersList.AppendLine($"--------|---------------|----------------");
                    foreach (Order order in dbContext.Orders)
                    {
                        ordersList.AppendLine($"{order.OrderNumber}\t| {order.Status}\t| {order.TotalPrice}");
                    }
                    if (args.Message.Reply != null)
                    {
                        string message = ordersList.ToString();
                        _natsConnection.Publish(args.Message.Reply, Encoding.UTF8.GetBytes(message));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Error: {ex.InnerException.Message}");
                }
            }
        }
    }
}