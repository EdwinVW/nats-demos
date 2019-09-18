namespace Store.OrderProcessingService.Domain
{
    public class OrderedProduct
    {
        public string ProductNumber { get; private set; }
        public decimal Price { get; private set; }

        public OrderedProduct(string productNumber, decimal price)
        {
            this.ProductNumber = productNumber;
            this.Price = price;
        }
    }
}