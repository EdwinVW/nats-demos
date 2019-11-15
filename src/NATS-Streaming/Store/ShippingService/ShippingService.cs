using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using STAN.Client;
using Store.ShippingService.Events;

namespace Store.ShippingService
{
    public class ShippingService
    {
        private IStanConnection _stanConnection;
        public void Start()
        {
            // connect to STAN
            var cf = new StanConnectionFactory();
            var natsOptions = StanOptions.GetDefaultOptions();
            natsOptions.NatsURL = "nats://localhost:4223";
            _stanConnection = cf.CreateConnection("test-cluster", "ShippingService", natsOptions);

            // create events subscription
            StanSubscriptionOptions stanOptions = StanSubscriptionOptions.GetDefaultOptions();
            stanOptions.DurableName = "ShippingService";

            // determine where to start reading in the event-stream
            ulong? lastSeqNr = GetLastSequenceNumber();
            if (lastSeqNr != null)
            {
                lastSeqNr++;
                stanOptions.StartAt(lastSeqNr.Value);
             
                Console.WriteLine($"Replaying from seq# {lastSeqNr}");
            }
            else
            {
                stanOptions.DeliverAllAvailable();

                Console.WriteLine("Replaying all messages.");
            }

            _stanConnection.Subscribe("store.events", stanOptions, EventReceived);
        }

        public void Stop()
        {
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
                
                // skip already handled events based on last handled sequence-number
                ulong sequenceNumber = args.Message.Sequence;
                ulong? lastSeqNr = GetLastSequenceNumber();
                if (lastSeqNr.HasValue)
                {
                    if (sequenceNumber <= lastSeqNr)
                    {
                        return;
                    }
                }

                // determine event CLR type
                string fullEventTypeName = $"Store.ShippingService.Events.{eventTypeName}";
                Type eventType = Type.GetType(fullEventTypeName, true);

                // handle event
                dynamic e = JsonSerializer.Deserialize(eventData, eventType);
                Handle(e);

                // update progress
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

        private async void Handle(OrderCreated e)
        {
            Console.WriteLine($"Order #{e.OrderNumber} created.");

            using (var dbContext = new StoreDBContext())
            {
                dbContext.Orders.Add(new Order { OrderNumber = e.OrderNumber, Shipped = false });
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
                        ProductNumber = e.ProductNumber
                    });
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
                    // here shipping should be handled
                    // e.g. a shipping manifest for the shipping provider is printed

                    order.Shipped = true;
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

        private ulong? GetLastSequenceNumber()
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

        private async void UpdateLastSequenceNumber(ulong sequenceNumber)
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