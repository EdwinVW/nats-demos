namespace Store.Messaging
{
    public class Delegates
    {
        public delegate string RequestAvailableCallback(string messageType, string messageData);
        public delegate void MessageAvailableCallback(string messageType, string messageData);
    }
}