using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Store.Messaging;

namespace MultiUserSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            int numOfSimulationThreads = 10;
            if (args.Length == 1)
            {
                numOfSimulationThreads = Convert.ToInt32(args[0]);
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            Task[] simulationTasks = StartSimulation(numOfSimulationThreads, cts.Token);

            Console.WriteLine("Simulation tasks started. Press any key to stop ...");
            Console.ReadLine();

            cts.Cancel();
            Task.WaitAll(simulationTasks, 5000);
        }

        private static Task[] StartSimulation(int numOfSimulationThreads, CancellationToken cancellationToken)
        {
            var messageBroker = new NATSMessageBroker("nats://localhost:4222");
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < numOfSimulationThreads; i++)
            {
                UserSimulation userSimulation = new UserSimulation(i, messageBroker);
                taskList.Add(userSimulation.Start(cancellationToken));
            }
            return taskList.ToArray();
        }
    }
}
