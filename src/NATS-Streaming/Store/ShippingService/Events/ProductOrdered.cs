namespace Store.ShippingService.Events
{
    public class ProductOrdered
    {
        public string OrderNumber { get; set; }
        public string ProductNumber { get; set; }
    }
}