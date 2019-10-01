using System;

namespace Store.OrderProcessingService
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            
            OrderProcessingService service = new OrderProcessingService();
            service.Start();

            Console.WriteLine("OrderProcessingService online.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);

            service.Stop();
        }
    }
}
