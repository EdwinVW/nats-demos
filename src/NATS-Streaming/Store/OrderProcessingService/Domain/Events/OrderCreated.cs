namespace Store.OrderProcessingService.Domain.Events
{
    public class OrderCreated : BusinessEvent
    {
        public string OrderNumber { get; set; }

        public OrderCreated()
        {

        }
        
        public OrderCreated(string orderNumber)
        {
            this.OrderNumber = orderNumber;

        }
    }
}