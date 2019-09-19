using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Store.Messaging;
using Store.Messaging.Events;
using Store.OrderProcessingService.Domain;
using Store.OrderProcessingService.Repositories;

namespace Store.OrderProcessingService
{
    public class Program
    {
        private static IOrderRepository _repo;
        private static NATSMessageBroker _commandsMessageBroker;
        private static STANMessageBroker _eventsMessageBroker;

        static void Main(string[] args)
        {
            //_repo = new NATSOrderRepository();
            _repo = new SQLiteOrderRepository();

            _commandsMessageBroker = new NATSMessageBroker("nats://localhost:4222");
            _commandsMessageBroker.StartMessageConsumer("store.commands.*", RequestAvailable);

            _eventsMessageBroker = new STANMessageBroker("nats://localhost:4223", "OrderprocessingService");

            Console.Clear();
            Console.WriteLine("OrderProcessingService online.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);

            _repo.Close();
            _commandsMessageBroker.StopMessageConsumers();
            _commandsMessageBroker.Dispose();

            _eventsMessageBroker.StopMessageConsumers();
            _eventsMessageBroker.Dispose();
        }

        private static string RequestAvailable(string messageType, string messageData)
        {
            string response = "OK";

            try
            {
                var method = DetermineHandlerMethod(messageType);
                if (method != null)
                {
                    response = method.Invoke(null, new object[] { messageData }).ToString();
                }
                else
                {
                    response = $"Error: Received unknown message-type '{messageType}'.";
                }
            }
            catch (Exception ex)
            {
                response = $"Error: {ex.Message}";
            }

            return response;
        }

        private static MethodInfo DetermineHandlerMethod(string messageType)
        {
            return typeof(Store.OrderProcessingService.Program)
                .GetMethod(messageType, BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static string CreateOrder(string messageData)
        {
            try
            {
                string[] messageParts = messageData.Split('|');
                string orderNumber = messageParts[0];
                Console.WriteLine($"Create order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber) ?? new Order();

                var result = order.Create(orderNumber);
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

        private static string OrderProduct(string messageData)
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

                var result = order.OrderProduct(productNumber);
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


        private static string RemoveProduct(string messageData)
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

                var result = order.RemoveProduct(productNumber);
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

        private static string CompleteOrder(string messageData)
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

                var result = order.Complete(shippingAddress);
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

        private static string ShipOrder(string messageData)
        {
            try
            {
                string[] messageParts = messageData.Split('|');
                string orderNumber = messageParts[0];
                Console.WriteLine($"Ship order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber);
                if (order == null)
                {
                    return $"Error: no order with specified order #{orderNumber} exists.";
                }

                var result = order.Ship();
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

        private static string CancelOrder(string messageData)
        {
            try
            {
                string[] messageParts = messageData.Split('|');
                string orderNumber = messageParts[0];
                Console.WriteLine($"Cancel order #{orderNumber}");

                Order order = _repo.GetByOrderNumber(orderNumber);
                if (order == null)
                {
                    return $"Error: no order with specified order #{orderNumber} exists.";
                }

                var result = order.Cancel();
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

        private static void PublishBusinessEvent(BusinessEvent e)
        {
            string eventType = e.GetType().AssemblyQualifiedName;
            string eventData = JsonSerializer.Serialize(e, e.GetType());
            _eventsMessageBroker.Publish("store.events", eventType, eventData);
        }
    }
}
