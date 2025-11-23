using PaymentApi.Models;

namespace PaymentApi.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByClientRequestIdAsync(string clientRequestId);
        Task<Payment?> GetByIdAsync(int id);
        Task<List<Payment>> ListAsync();
        Task<Payment> AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(Payment payment);
        Task<int> IncrementReferenceCounterAsync(string dateKey);
    }
}
