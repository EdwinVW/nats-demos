namespace Store.OrdersQueryService.Events
{
    public class ProductOrdered
    {
        public string OrderNumber { get; set; }
        public string ProductNumber { get; set; }
        public decimal Price { get; set; }
    }
}