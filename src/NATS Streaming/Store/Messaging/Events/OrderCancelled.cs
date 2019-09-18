namespace Store.Messaging.Events
{
    public class OrderCancelled : BusinessEvent
    {
        public string OrderNumber { get; set; }
        
        public OrderCancelled()
        {
            
        }
        
        public OrderCancelled(string orderNumber)
        {
            this.OrderNumber = orderNumber;

        }
    }
}