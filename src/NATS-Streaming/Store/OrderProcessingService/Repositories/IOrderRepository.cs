using Store.OrderProcessingService.Domain.Events;
using Store.OrderProcessingService.Domain;

namespace Store.OrderProcessingService.Repositories
{
    public interface IOrderRepository
    {
        void Close();
        Order GetByOrderNumber(string orderNumber);
        void Update(string orderNumber, BusinessEvent e);
    }
}