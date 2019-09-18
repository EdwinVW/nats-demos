using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STAN.Client;
using static Store.Messaging.Delegates;

namespace Store.Messaging
{
    public class STANMessageBroker : IDisposable
    {
        private bool _disposed = false;
        private string _url;
        private CancellationTokenSource _cts;
        private List<Task> _consumerTasks;        
        private IStanConnection _connection;
        
        public STANMessageBroker(string url, string clientId)
        {
            _url = url;
            _cts = new CancellationTokenSource();
            _consumerTasks = new List<Task>();
            var cf = new StanConnectionFactory();
            var options = StanOptions.GetDefaultOptions();
            options.NatsURL = _url;            
            _connection = cf.CreateConnection("test-cluster", clientId, options);                
        }

        public void Publish(string subject, string messageType, string messageData)
        {
            try
            {
                var payload = $"{messageType}#{messageData}";
                _connection.Publish(subject, Encoding.UTF8.GetBytes(payload));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void StartMessageConsumer(string subject, MessageAvailableCallback callback, bool replay = false)
        {
            _consumerTasks.Add(Task.Run(() => ConsumerWorker(subject, callback, replay)));
        }

        public void StartDurableMessageConsumer(string subject, MessageAvailableCallback callback, string durableName)
        {
            _consumerTasks.Add(Task.Run(() => ConsumerWorker(subject, callback, false, durableName)));
        }

        public void StopMessageConsumers()
        {
            _cts.Cancel();
            Task.WaitAll(_consumerTasks.ToArray(), 5000);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);             
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing) 
            {
                _connection.Close();
            }
            
            _disposed = true;
        }

        private void ConsumerWorker(string subject, MessageAvailableCallback callback, bool replay = false, string durableName = null)
        {
            try
            {
                var subOptions = StanSubscriptionOptions.GetDefaultOptions();
                subOptions.DurableName = durableName;
                if (replay)
                {
                    subOptions.DeliverAllAvailable();
                }
                _connection.Subscribe(subject, subOptions, (obj, args) =>
                {
                    string message = System.Text.Encoding.UTF8.GetString(args.Message.Data);
                    string[] messageParts = message.Split('#');
                    string eventType = messageParts[0];
                    string eventData = message.Substring(message.IndexOf('#') + 1);
                    callback.Invoke(eventType, eventData);
                });

                _cts.Token.WaitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}