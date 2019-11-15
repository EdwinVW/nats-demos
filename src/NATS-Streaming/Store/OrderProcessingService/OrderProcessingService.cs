using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using NATS.Client;
using STAN.Client;
using Store.OrderProcessingService.Domain;
using Store.OrderProcessingService.Domain.Events;
using Store.OrderProcessingService.Repositories;

namespace Store.OrderProcessingService
{
    public class OrderProcessingService
    {
        private IOrderRepository _repo;
        private IConnection _natsConnection;
        private IStanConnection _stanConnection;
        private IAsyncSubscription _commandsSubscription;

        public void Start()
        {
            _repo = new SQLServerOrderRepository();

            // connect to NATS
            var natsConnectionFactory = new ConnectionFactory();
            _natsConnection = natsConnectionFactory.CreateConnection("nats://localhost:4222");

            // connect to STAN
            var cf = new StanConnectionFactory();
            var options = StanOptions.GetDefaultOptions();
            options.NatsURL = "nats://localhost:4223";
            _stanConnection = cf.CreateConnection("test-cluster", "OrderProcessingService", options);

            // create commands subscription
            _commandsSubscription = _natsConnection.SubscribeAsync("store.commands.*", CommandReceived);
        }

        public void Stop()
        {
            _repo.Close();
            _natsConnection.Close();
            _stanConnection.Close();
        }

        private void CommandReceived(object sender, MsgHandlerEventArgs args)
        {
            // get message data and determine message-type
            // (message-type is embedded in the subject: store.commands.<message-type>)
            string messageData = System.Text.Encoding.UTF8.GetString(args.Message.Data);
            string messageType = args.Message.Subject.Split('.').Last();

            // handle message
            string response;
            try
            {
                switch (messageType)
                {
                    case "CreateOrder":
                        response = CreateOrder(messageData);
                        break;
                    case "OrderProduct":
                        response = OrderProduct(messageData);
                        break;
                    case "RemoveProduct":
                        response = RemoveProduct(messageData);
                        break;
                    case "CompleteOrder":
                        response = CompleteOrder(messageData);
                        break;
                    case "ShipOrder":
                        response = ShipOrder(messageData);
                        break;
                    case "CancelOrder":
                        response = CancelOrder(messageData);
                        break;
                    default:
                        response = $"Error: Received unknown message-type '{messageType}'.";
                        break;
                }
            }
            catch (Exception ex)
            {
                response = $"Error: {ex.Message}";
            }

            // send reply
            if (args.Message.Reply != null)
            {
                byte[] message = Encoding.UTF8.GetBytes(response);
                _natsConnection.Publish(args.Message.Reply, message);
            }
        }

        private string CreateOrder(string messageData)
        {
            try
            {
                string orderNumber = messageData;
                Console.WriteLine($"Create order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber) ?? new Order();

                CommandHandlingResult result = order.Create(orderNumber);
                if (result.Successfull)
                {
                    _repo.Update(orderNumber, result.BusinessEvent);
                    PublishBusinessEvent(result.BusinessEvent);
                }
                else
                {
                    return result.Errormessage;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        private string OrderProduct(string messageData)
        {
            try
            {
                string[] messageParts = messageData.Split('|');
                string orderNumber = messageParts[0];
                string productNumber = messageParts[1];
                Console.WriteLine($"Add product #{productNumber} to order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber);
                if (order == null)
                {
                    return $"Error: no order with specified order #{orderNumber} exists.";
                }

                CommandHandlingResult result = order.OrderProduct(productNumber);
                if (result.Successfull)
                {
                    _repo.Update(orderNumber, result.BusinessEvent);
                    PublishBusinessEvent(result.BusinessEvent);
                }
                else
                {
                    return result.Errormessage;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        private string RemoveProduct(string messageData)
        {
            try
            {
                string[] messageParts = messageData.Split('|');
                string orderNumber = messageParts[0];
                string productNumber = messageParts[1];
                Console.WriteLine($"Remove product #{productNumber} from order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber);
                if (order == null)
                {
                    return $"Error: no order with specified order #{orderNumber} exists.";
                }

                CommandHandlingResult result = order.RemoveProduct(productNumber);
                if (result.Successfull)
                {
                    _repo.Update(orderNumber, result.BusinessEvent);
                    PublishBusinessEvent(result.BusinessEvent);
                }
                else
                {
                    return result.Errormessage;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        private string CompleteOrder(string messageData)
        {
            try
            {
                string[] messageParts = messageData.Split('|');
                string orderNumber = messageParts[0];
                string shippingAddress = messageParts[1];
                Console.WriteLine($"Complete order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber);
                if (order == null)
                {
                    return $"Error: no order with specified order #{orderNumber} exists.";
                }

                CommandHandlingResult result = order.Complete(shippingAddress);
                if (result.Successfull)
                {
                    _repo.Update(orderNumber, result.BusinessEvent);
                    PublishBusinessEvent(result.BusinessEvent);
                }
                else
                {
                    return result.Errormessage;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        private string ShipOrder(string messageData)
        {
            try
            {
                string orderNumber = messageData;
                Console.WriteLine($"Ship order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber);
                if (order == null)
                {
                    return $"Error: no order with specified order #{orderNumber} exists.";
                }

                CommandHandlingResult result = order.Ship();
                if (result.Successfull)
                {
                    _repo.Update(orderNumber, result.BusinessEvent);
                    PublishBusinessEvent(result.BusinessEvent);
                }
                else
                {
                    return result.Errormessage;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        private string CancelOrder(string messageData)
        {
            try
            {
                string orderNumber = messageData;
                Console.WriteLine($"Cancel order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber);
                if (order == null)
                {
                    return $"Error: no order with specified order #{orderNumber} exists.";
                }

                CommandHandlingResult result = order.Cancel();
                if (result.Successfull)
                {
                    _repo.Update(orderNumber, result.BusinessEvent);
                    PublishBusinessEvent(result.BusinessEvent);
                }
                else
                {
                    return result.Errormessage;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        private void PublishBusinessEvent(BusinessEvent e)
        {
            string eventType = e.GetType().Name;
            string eventData = JsonSerializer.Serialize(e, e.GetType());
            
            // create message
            // Event-type is embedded in the message:
            //   <event-type>#<value>|<value>|<value>
            string body = $"{eventType}#{eventData}";
            byte[] message = Encoding.UTF8.GetBytes(body);

            _stanConnection.Publish("store.events", message);
        }
    }
}