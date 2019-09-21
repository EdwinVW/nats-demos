namespace Store.OrdersQueryService.Events
{
    public class ProductRemoved
    {
        public string OrderNumber { get; set; }
        public string ProductNumber { get; set; }
    }
}