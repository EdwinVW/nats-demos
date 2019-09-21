namespace Store.ShippingService.Events
{
    public class OrderCompleted
    {
        public string OrderNumber { get; set; }
        public string ShippingAddress { get; set; }
    }
}