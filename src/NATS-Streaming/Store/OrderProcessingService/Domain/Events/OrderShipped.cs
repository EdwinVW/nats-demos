namespace Store.OrderProcessingService.Domain.Events
{
    public class OrderShipped : BusinessEvent
    {
        public string OrderNumber { get; set; }

        public OrderShipped()
        {
            
        }

        public OrderShipped(string orderNumber)
        {
            this.OrderNumber = orderNumber;

        }
    }
}