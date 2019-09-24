using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NATS.Client;
using static Store.Messaging.Delegates;

namespace Store.Messaging
{
    public class NATSMessageBroker : IDisposable
    {
        private bool _disposed = false;
        private string _url;
        private List<IAsyncSubscription> _subscriptions;
        private IConnection _connection;

        public NATSMessageBroker(string url)
        {
            _url = url;
            _subscriptions = new List<IAsyncSubscription>();
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

        public void StartMessageConsumer(string subject, NATSRequestAvailableCallback callback)
        {
            var sub = _connection.SubscribeAsync(subject, (obj, args) =>
            {
                string message = System.Text.Encoding.UTF8.GetString(args.Message.Data);
                string eventType = args.Message.Subject.Split('.').Last();
                string response = callback.Invoke(eventType, message);
                if (response != null && args.Message.Reply != null)
                {
                    _connection.Publish(args.Message.Reply, Encoding.UTF8.GetBytes(response));
                }
            });
            _subscriptions.Add(sub);
        }

        public void StopMessageConsumers()
        {
            foreach(var sub in _subscriptions)
            {
                sub.Drain();
            }
        }
    }
}