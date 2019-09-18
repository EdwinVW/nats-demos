using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HighVolumeApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: HighVolumeApp <number of parallel simulation tasks> <simulation duration in seconds>");
                return;
            }

            int numOfSimulationThreads = Convert.ToInt32(args[0]);
            int simulationDurationInSeconds = Convert.ToInt32(args[0]);

            CancellationTokenSource cts = new CancellationTokenSource();
            Task[] simulationTasks = StartSimulation(numOfSimulationThreads, cts.Token);

            Console.WriteLine("Simulation tasks started. Press any key to stop ...");
            Console.ReadLine();

            cts.Cancel();
            Task.WaitAll(simulationTasks, 5000);
        }

        private static Task[] StartSimulation(int numOfSimulationThreads, CancellationToken cancellationToken)
        {
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < numOfSimulationThreads; i++)
            {
                UserSimulation userSimulation = new UserSimulation(i);
                taskList.Add(userSimulation.Start(cancellationToken));
            }
            return taskList.ToArray();
        }
    }
}
