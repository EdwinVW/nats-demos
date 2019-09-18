using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Store.QueryService
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