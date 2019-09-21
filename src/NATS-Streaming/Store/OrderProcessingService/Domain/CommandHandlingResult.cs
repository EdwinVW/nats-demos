
using Store.OrderProcessingService.Domain.Events;

namespace Store.OrderProcessingService.Domain
{
    /// <summary>
    /// Encapsulates the result of handling a command.
    /// </summary>
    public class CommandHandlingResult
    {
        /// <summary>
        /// Indication whether the command was handled succesfully.
        /// </summary>
        public bool Successfull { get; }

        /// <summary>
        /// The business-event that represents the successfull handling of the command.
        /// </summary>
        public BusinessEvent BusinessEvent { get; set; }

        /// <summary>
        /// The optional error-message in case of a failure.
        /// </summary>
        public string Errormessage { get; set; }

        public static CommandHandlingResult Success(BusinessEvent businessEvent)
        {
            return new CommandHandlingResult(true, businessEvent);
        }

        public static CommandHandlingResult Fail(string errorMessage)
        {
            return new CommandHandlingResult(false, null, errorMessage);
        }

        public CommandHandlingResult(bool successfull, BusinessEvent businessEvent, string errormessage = null)
        {
            Successfull = successfull;
            BusinessEvent = businessEvent;
            Errormessage = errormessage;
        }
    }
}