namespace Store.Messaging
{
    public class Delegates
    {
        public delegate string NATSRequestAvailableCallback(string messageType, string messageData);
        public delegate void NATSMessageAvailableCallback(string messageType, string messageData);
        public delegate void STANMessageAvailableCallback(string messageType, string messageData, ulong sequenceNumber);
    }
}