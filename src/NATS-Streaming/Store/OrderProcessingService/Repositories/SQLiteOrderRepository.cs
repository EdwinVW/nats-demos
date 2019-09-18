using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Store.Messaging.Events;
using Store.OrderProcessingService.Domain;

namespace Store.OrderProcessingService.Repositories
{
    public class SQLiteOrderRepository : IOrderRepository
    {
        public Order GetByOrderNumber(string orderNumber)
        {
            using (var dbContext = new EventStoreDBContext())
            {

                var order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
                if (order == null)
                {
                    return null;
                }

                List<BusinessEvent> events = new List<BusinessEvent>();
                dbContext.Entry(order).Collection(o => o.Events).Load();
                foreach (OrderEvent orderEvent in order.Events.OrderBy(e => e.Version))
                {
                    string eventTypeDescriptor = orderEvent.EventType;
                    Type eventType = Type.GetType(eventTypeDescriptor);
                    string eventData = orderEvent.EventData;
                    dynamic e = JsonSerializer.Deserialize(eventData, eventType);
                    events.Add(e);
                }

                return new Order(events);
            }
        }

        public async void Update(string orderNumber, BusinessEvent e)
        {
            string eventType = e.GetType().AssemblyQualifiedName;
            string eventData = JsonSerializer.Serialize(e, e.GetType());

            using (var dbContext = new EventStoreDBContext())
            {
                OrderAggregate order = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
                int nextVersion = 1;
                if (order == null)
                {
                    order = new OrderAggregate { OrderNumber = orderNumber };
                    dbContext.Orders.Add(order);
                }
                else
                {
                    await dbContext.Entry(order).Collection(o => o.Events).LoadAsync();
                    nextVersion = order.Events.Max(e => e.Version) + 1;
                }
                order.CurrentVersion = nextVersion;
                order.Events.Add(new OrderEvent
                {
                    OrderNumber = orderNumber,
                    Version = nextVersion,
                    Timestamp = DateTime.Now,
                    EventType = eventType,
                    EventData = eventData
                });
                await dbContext.SaveChangesAsync();
            }
        }

        public void Close()
        {
        }
    }
}