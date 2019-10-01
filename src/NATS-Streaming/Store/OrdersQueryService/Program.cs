﻿using System;

namespace Store.OrdersQueryService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            
            OrdersQueryService service = new OrdersQueryService();
            service.Start();

            Console.WriteLine("OrdersQueryService online.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);

            service.Stop();
        }
    }
}
