namespace Store.OrderProcessingService.Domain.Events
{
    public class ProductRemoved : BusinessEvent
    {
        public string OrderNumber { get; set; }
        public string ProductNumber { get; set; }

        public ProductRemoved()
        {

        }
        
        public ProductRemoved(string orderNumber, string productNumber)
        {
            this.OrderNumber = orderNumber;
            this.ProductNumber = productNumber;

        }
    }
}