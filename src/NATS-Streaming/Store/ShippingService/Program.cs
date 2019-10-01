using System;

namespace Store.ShippingService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            
            ShippingService service = new ShippingService();
            service.Start();

            Console.WriteLine("ShippingService online.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);

            service.Stop();
        }
    }
}
