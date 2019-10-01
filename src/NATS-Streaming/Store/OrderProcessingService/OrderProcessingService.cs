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
            _repo = new SQLiteOrderRepository();

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
            // get message data and type
            string messageData = System.Text.Encoding.UTF8.GetString(args.Message.Data);
            string messageType = args.Message.Subject.Split('.').Last();

            // handle message
            string response;
            try
            {
                response = CallMessageHandler(messageType, messageData);
            }
            catch (Exception ex)
            {
                response = $"Error: {ex.Message}";
            }

            // send reply
            if (args.Message.Reply != null)
            {
                _natsConnection.Publish(args.Message.Reply, Encoding.UTF8.GetBytes(response));
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
            
            string message = $"{eventType}#{eventData}";

            _stanConnection.Publish("store.events", Encoding.UTF8.GetBytes(message));
        }

        #region Private helpers

        private string CallMessageHandler(string messageType, string messageData)
        {
            var method = this.GetType().GetMethod(messageType, BindingFlags.Instance | BindingFlags.NonPublic);

            if (method != null)
            {
                return method.Invoke(this, new object[] { messageData }).ToString();
            }
            else
            {
                return $"Error: Received unknown message-type '{messageType}'.";
            }
        }

        #endregion         
    }
}