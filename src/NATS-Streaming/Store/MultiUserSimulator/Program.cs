using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;

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

            // connect to nats
            var natsConnectionFactory = new ConnectionFactory();
            IConnection connection = natsConnectionFactory.CreateConnection("nats://localhost:4222");

            CancellationTokenSource cts = new CancellationTokenSource();
            Task[] simulationTasks = StartSimulation(
                numOfSimulationThreads, cts.Token, connection);

            Console.WriteLine("Simulation tasks started. Press any key to stop ...");
            Console.ReadLine();

            cts.Cancel();
            Task.WaitAll(simulationTasks, 5000);
            connection.Close();
        }

        private static Task[] StartSimulation(
            int numOfSimulationThreads, CancellationToken cancellationToken, IConnection connection)
        {
            // start simulation tasks
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < numOfSimulationThreads; i++)
            {
                UserSimulation userSimulation = new UserSimulation(i, connection);
                taskList.Add(userSimulation.Start(cancellationToken));
            }
            return taskList.ToArray();
        }
    }
}
