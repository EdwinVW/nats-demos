using System.Collections.Generic;
using System.Linq;
using Store.OrderProcessingService.Domain.Events;

namespace Store.OrderProcessingService.Domain
{
    public class Order
    {
        public string OrderNumber { get; private set; }
        public OrderStatus Status { get; private set; }
        public List<OrderedProduct> Products { get; private set; }
        public decimal TotalPrice => Products.Sum(p => p.Price);
        public string ShippingAddress { get; private set; }

        public Order()
        {
            Products = new List<OrderedProduct>();
        }

        public Order(IEnumerable<BusinessEvent> events)
        {
            Products = new List<OrderedProduct>();

            // restore state by replaying all events in the specified event-stream
            foreach (dynamic e in events)
            {
                Handle(e);
            }
        }

        public CommandHandlingResult Create(string orderNumber)
        {
            if (this.OrderNumber != null)
            {
                return CommandHandlingResult.Fail("Error: an order with the specified order-number already exists.");
            }

            var e = new OrderCreated(orderNumber);
            Handle(e);

            return CommandHandlingResult.Success(e);
        }

        public CommandHandlingResult OrderProduct(string productNumber)
        {
            if (this.Status != OrderStatus.InProgress)
            {
                return CommandHandlingResult.Fail("Error: products can only be added to in progress orders.");
            }

            var e = new ProductOrdered(this.OrderNumber, productNumber, GetProductPrice(productNumber));
            Handle(e);

            return CommandHandlingResult.Success(e);
        }        

        public CommandHandlingResult RemoveProduct(string productNumber)
        {
            if (!Products.Any(p => p.ProductNumber == productNumber))
            {
                return CommandHandlingResult.Fail("Error: the order does not contain the specified product.");
            }

            if (this.Status != OrderStatus.InProgress)
            {
                return CommandHandlingResult.Fail("Error: products can only be removed from in progress orders.");
            }

            var e = new ProductRemoved(this.OrderNumber, productNumber);
            Handle(e);

            return CommandHandlingResult.Success(e);
        } 

        public CommandHandlingResult Complete(string shippingAddress)
        {
            if (!Products.Any())
            {
                return CommandHandlingResult.Fail("Error: the order does not contain any products.");
            }

            if (this.Status != OrderStatus.InProgress)
            {
                return CommandHandlingResult.Fail("Error: only in progress orders can be completed.");
            }

            var e = new OrderCompleted(this.OrderNumber, shippingAddress);
            Handle(e);

            return CommandHandlingResult.Success(e);
        } 

        public CommandHandlingResult Ship()
        {
            if (this.Status != OrderStatus.Completed)
            {
                return CommandHandlingResult.Fail("Error: only completed orders can be shipped.");
            }

            var e = new OrderShipped(this.OrderNumber);
            Handle(e);

            return CommandHandlingResult.Success(e);
        }   

        public CommandHandlingResult Cancel()
        {
            if (this.Status != OrderStatus.InProgress && this.Status != OrderStatus.Completed)
            {
                return CommandHandlingResult.Fail("Error: only in progress and completed orders can be cancelled.");
            }

            var e = new OrderCancelled(this.OrderNumber);
            Handle(e);

            return CommandHandlingResult.Success(e);
        }                         

        private void Handle(OrderCreated e)
        {
            OrderNumber = e.OrderNumber;
            Status = OrderStatus.InProgress;
        }

        private void Handle(ProductOrdered e)
        {
            Products.Add(new OrderedProduct(e.ProductNumber, e.Price));
        }      

        private void Handle(ProductRemoved e)
        {
            Products.Remove(Products.Find(p => p.ProductNumber == e.ProductNumber));
        }    

        private void Handle(OrderCompleted e)
        {
            this.ShippingAddress = e.ShippingAddress;
            this.Status = OrderStatus.Completed;
        }    

        private void Handle(OrderShipped e)
        {
            this.Status = OrderStatus.Shipped;
        } 

        private void Handle(OrderCancelled e)
        {
            this.Status = OrderStatus.Cancelled;
        }                                       

        private decimal GetProductPrice(string productNumber)
        {
            switch (productNumber)
            {
                case "1":
                    return 31.95M;
                case "2":
                    return 31.45M;
                case "3":
                    return 57.15M;
                case "4":
                    return 35;
                case "5":
                    return 30.40M;             
                case "6":
                    return 44.80M;             
            }
            return 0;
        }
    }
}