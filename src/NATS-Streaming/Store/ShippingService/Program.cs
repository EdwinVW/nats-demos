using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Store.Messaging;
using Store.ShippingService.Events;

namespace Store.ShippingService
{
    class Program
    {
        private static STANMessageBroker _eventsMessageBroker;

        static void Main(string[] args)
        {
            Console.Clear();

            using (_eventsMessageBroker = new STANMessageBroker("nats://localhost:4223", "ShippingService"))
            {
                ulong? lastSeqNr = GetLastSequenceNumber();
                if (lastSeqNr != null)
                {
                    lastSeqNr++;
                    Console.WriteLine($"Replaying from seq# {lastSeqNr}");
                }
                else
                {
                    Console.WriteLine("Replaying all messages.");
                }

                _eventsMessageBroker.StartRegularMessageConsumer("store.events", EventReceived, true, lastSeqNr);

                Console.WriteLine("ShippingService online.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                _eventsMessageBroker.StopMessageConsumers();
            }
        }

        private static void EventReceived(string messageType, string messageData, ulong sequenceNumber)
        {
            try
            {
                ulong? lastSeqNr = GetLastSequenceNumber();
                if (lastSeqNr.HasValue)
                {
                    // skip already handled events based on last handled sequence-number
                    if (sequenceNumber <= lastSeqNr)
                    {
                        return;
                    }
                }

                string eventTypeName = $"Store.ShippingService.Events.{messageType}";
                Type eventType = Type.GetType(eventTypeName);
                dynamic e = JsonSerializer.Deserialize(messageData, eventType);
                Handle(e);
                UpdateLastSequenceNumber(sequenceNumber);
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

        private static MethodInfo DetermineHandlerMethod(string messageType)
        {
            return typeof(Store.ShippingService.Program)
                .GetMethod(messageType, BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static async void Handle(OrderCreated e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} created.");

            using (var dbContext = new StoreDBContext())
            {
                dbContext.Orders.Add(new Order { OrderNumber = e.OrderNumber, Shipped = false });
                await dbContext.SaveChangesAsync();
            }
        }

        private static async void Handle(ProductOrdered e)
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
                        ProductNumber = e.ProductNumber
                    });
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private static async void Handle(ProductRemoved e)
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
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }

        private static async void Handle(OrderCompleted e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} completed.");

            using (var dbContext = new StoreDBContext())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);
                if (order != null)
                {
                    order.ShippingAddress = e.ShippingAddress;
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private static async void Handle(OrderShipped e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} shipped.");

            using (var dbContext = new StoreDBContext())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == e.OrderNumber);
                if (order != null)
                {
                    // here shipping should be handled
                    // e.g. a shipping manifest for the shipping provider is printed

                    order.Shipped = true;
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private static async void Handle(OrderCancelled e)
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

        private static ulong? GetLastSequenceNumber()
        {
            using (var dbContext = new StoreDBContext())
            {
                var info = dbContext.ShippingInfo.FirstOrDefault();
                if (info == null)
                {
                    return null;
                }
                return info.LastSeqNr;
            }
        }

        private static async void UpdateLastSequenceNumber(ulong sequenceNumber)
        {
            using (var dbContext = new StoreDBContext())
            {
                var info = dbContext.ShippingInfo.FirstOrDefault();
                if (info == null)
                {
                    dbContext.ShippingInfo.Add(new ShippingInfo { Id = 0, LastSeqNr = sequenceNumber });
                }
                else
                {
                    info.LastSeqNr = sequenceNumber;
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
