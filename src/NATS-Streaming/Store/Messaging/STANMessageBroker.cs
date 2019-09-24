using System;
using System.Collections.Generic;
using System.Text;
using STAN.Client;
using static Store.Messaging.Delegates;

namespace Store.Messaging
{
    public class STANMessageBroker : IDisposable
    {
        private bool _disposed = false;
        private string _url;
        private List<IStanSubscription> _subscriptions;
        private IStanConnection _connection;

        public STANMessageBroker(string url, string clientId)
        {
            _url = url;
            _subscriptions = new List<IStanSubscription>();
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

        public void StartDurableMessageConsumer(string subject, STANMessageAvailableCallback callback, string durableName)
        {
            StartMessageConsumerInternal(subject, callback, durableName, false, null);
        }
        
        public void StartRegularMessageConsumer(string subject, STANMessageAvailableCallback callback, bool replay, ulong? startAtSeqNr)
        {
            StartMessageConsumerInternal(subject, callback, null, replay, startAtSeqNr);
        }

        private void StartMessageConsumerInternal(string subject, STANMessageAvailableCallback callback, string durableName, bool replay, ulong? startAtSeqNr)
        {
            try
            {
                var subOptions = StanSubscriptionOptions.GetDefaultOptions();
                subOptions.DurableName = durableName;
                if (replay)
                {
                    if (startAtSeqNr.HasValue)
                    {
                        subOptions.StartAt(startAtSeqNr.Value);
                    }
                    else
                    {
                        subOptions.DeliverAllAvailable();
                    }
                }

                var sub = _connection.Subscribe(subject, subOptions, (obj, args) =>
                {
                    string message = System.Text.Encoding.UTF8.GetString(args.Message.Data);
                    string[] messageParts = message.Split('#');
                    string eventType = messageParts[0];
                    string eventData = message.Substring(message.IndexOf('#') + 1);
                    ulong sequenceNumber = args.Message.Sequence;
                    callback.Invoke(eventType, eventData, sequenceNumber);
                });

                _subscriptions.Add(sub);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void StopMessageConsumers()
        {
            foreach(var sub in _subscriptions)
            {
                sub.Close();
            }
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
    }
}