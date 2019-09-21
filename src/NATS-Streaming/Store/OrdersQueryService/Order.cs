using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Store.OrdersQueryService
{
    public class Order
    {
        [Key]
        public string OrderNumber { get; set; } 
        public string Status { get; set; }  
        public List<OrderedProduct> Products { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalPrice { get; set; }

        public Order()
        {
            Products = new List<OrderedProduct>();
        }
    }
}