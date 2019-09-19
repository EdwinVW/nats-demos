using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Store.ShippingService
{
    public class Order
    {
        [Key]
        public string OrderNumber { get; set; }
        public List<OrderedProduct> Products { get; set; }
        public string ShippingAddress { get; set; }
        public bool Shipped { get; set; }

    public Order()
        {
            Products = new List<OrderedProduct>();
        }
    }
}