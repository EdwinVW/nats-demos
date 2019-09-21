namespace Store.OrdersQueryService
{
    public class OrderedProduct
    {
        public string Id { get; set; }
        public string ProductNumber { get; set; }
        public decimal Price { get; set; }
        public Order Order { get; set; }
    }
}