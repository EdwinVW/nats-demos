namespace Store.ShippingService
{
    public class OrderedProduct
    {
        public string Id { get; set; }
        public string ProductNumber { get; set; }
        public Order Order { get; set; }
    }
}