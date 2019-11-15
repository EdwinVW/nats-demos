using System;
using System.Collections.Generic;
using System.Text;
using STAN.Client;
using System.Text.Json;
using Store.OrderProcessingService.Domain.Events;
using Store.OrderProcessingService.Domain;

namespace Store.OrderProcessingService.Repositories
{
    /// <summary>
    /// Order repository that uses a STAN stream as event-store.
    /// </summary>
    public class NATSOrderRepository : IOrderRepository
    {
        private Dictionary<string, List<BusinessEvent>> _eventStreams = new Dictionary<string, List<BusinessEvent>>();

        private IStanConnection _stanConnection;
        private const string CLIENTID = "Store-WriteModel";
        private const string EVENTSTREAM_SUBJECT = "store.orders.repository";

        public NATSOrderRepository()
        {
            try
            {
                var cf = new StanConnectionFactory();
                var options = StanOptions.GetDefaultOptions();
                options.NatsURL = "nats://localhost:4223";
                _stanConnection = cf.CreateConnection("test-cluster", CLIENTID, options);
                var subOptions = StanSubscriptionOptions.GetDefaultOptions();
                subOptions.DeliverAllAvailable();
                _stanConnection.Subscribe(EVENTSTREAM_SUBJECT, subOptions, (obj, args) =>
                {
                    try
                    {
                        string message = Encoding.UTF8.GetString(args.Message.Data);
                        string[] messageParts = message.Split('#');

                        string eventTypeDescriptor = 
                            $"Store.OrderProcessingService.Domain.Events.{messageParts[0]}";
                        Type eventType = Type.GetType(eventTypeDescriptor);
                        string eventData = message.Substring(message.IndexOf('#') + 1);
                        dynamic e = JsonSerializer.Deserialize(eventData, eventType);
                        if (_eventStreams.ContainsKey(e.OrderNumber))
                        {
                            _eventStreams[e.OrderNumber].Add(e);
                        }
                        else
                        {
                            _eventStreams.Add(e.OrderNumber, new List<BusinessEvent>() { e });

                            Console.WriteLine($"Order #{e.OrderNumber} found during replay.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void Close()
        {
            _stanConnection.Close();
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            if (_eventStreams.ContainsKey(orderNumber))
            {
                return new Order(_eventStreams[orderNumber]);
            }
            return null;
        }

        public void Update(string orderNumber, BusinessEvent e)
        {
            try
            {
                string eventType = e.GetType().Name;
                string eventData = JsonSerializer.Serialize(e, e.GetType());
                string message = $"{eventType}#{eventData}";
                _stanConnection.Publish(EVENTSTREAM_SUBJECT, Encoding.UTF8.GetBytes(message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}