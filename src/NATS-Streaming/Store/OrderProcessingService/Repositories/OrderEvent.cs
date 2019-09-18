using System;

namespace Store.OrderProcessingService.Repositories
{
    public class OrderEvent
    {
        public string Id { get; set; }
        public string OrderNumber { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } 
        public string EventData { get; set; }

        public OrderAggregate Order { get; set; }

        public OrderEvent()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}