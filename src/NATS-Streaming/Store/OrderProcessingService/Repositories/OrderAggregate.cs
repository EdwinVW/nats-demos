using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Store.OrderProcessingService.Repositories
{
    public class OrderAggregate
    {
        [Key]
        public string OrderNumber { get; set; }
        public int CurrentVersion { get; set; }
        public List<OrderEvent> Events { get; set; }

        public OrderAggregate()
        {
            Events = new List<OrderEvent>();
        }
    }
}