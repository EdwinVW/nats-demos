using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using static Store.Messaging.Delegates;

namespace Store.Messaging
{
    public class NATSMessageBroker : IDisposable
    {
        private bool _disposed = false;
        private string _url;
        private CancellationTokenSource _cts;
        private List<Task> _consumerTasks;
        private IConnection _connection;

        public NATSMessageBroker(string url)
        {
            _url = url;
            _cts = new CancellationTokenSource();
            _consumerTasks = new List<Task>();
            var cf = new ConnectionFactory();
            _connection = cf.CreateConnection(_url);
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

        public void Publish(string subject, string messageType, string messageData)
        {
            var natsSubject = $"{subject}.{messageType}";
            _connection.Publish(natsSubject, Encoding.UTF8.GetBytes(messageData));
        }

        public string Request(string subject, string messageType, string messageData, int timeOutMs)
        {
            var natsSubject = $"{subject}.{messageType}";
            var response = _connection.Request(natsSubject, Encoding.UTF8.GetBytes(messageData), timeOutMs);
            return Encoding.UTF8.GetString(response.Data);
        }

        public void StartMessageConsumer(string subject, RequestAvailableCallback callback)
        {
            _consumerTasks.Add(Task.Run(() => ConsumerWorker(subject, callback)));
        }

        public void StopMessageConsumers()
        {
            _cts.Cancel();
            Task.WaitAll(_consumerTasks.ToArray(), 5000);
        }

        private void ConsumerWorker(string subject, RequestAvailableCallback callback)
        {
            _connection.SubscribeAsync(subject, (obj, args) =>
            {
                string message = System.Text.Encoding.UTF8.GetString(args.Message.Data);
                string eventType = args.Message.Subject.Substring(args.Message.Subject.LastIndexOf('.') + 1);
                string response = callback.Invoke(eventType, message);
                if (response != null && args.Message.Reply != null)
                {
                    _connection.Publish(args.Message.Reply, Encoding.UTF8.GetBytes(response));
                }
            });

            _cts.Token.WaitHandle.WaitOne();
        }      
    }
}