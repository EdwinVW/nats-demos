namespace Store.OrderProcessingService.Domain.Events
{
    public class ProductOrdered : BusinessEvent
    {
        public string OrderNumber { get; set; }
        public string ProductNumber { get; set; }
        public decimal Price { get; set; }

        public ProductOrdered()
        {
            
        }

        public ProductOrdered(string orderNumber, string productNumber, decimal price)
        {
            this.OrderNumber = orderNumber;
            this.ProductNumber = productNumber;
            this.Price = price;

        }
    }
}