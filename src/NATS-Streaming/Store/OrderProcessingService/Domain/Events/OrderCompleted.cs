namespace Store.OrderProcessingService.Domain.Events
{
    public class OrderCompleted : BusinessEvent
    {
        public string OrderNumber { get; set; }
        public string ShippingAddress { get; set; }

        public OrderCompleted()
        {

        }
        
        public OrderCompleted(string orderNumber, string shippingAddress)
        {
            this.OrderNumber = orderNumber;
            this.ShippingAddress = shippingAddress;

        }
    }
}