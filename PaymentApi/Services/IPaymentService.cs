using PaymentApi.Models;
namespace PaymentApi.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreateAsync(decimal amount, string currency, string clientRequestId, string? provider = null);
        Task<List<Payment>> ListAsync();
        Task<Payment?> UpdateAsync(int id, decimal amount, string currency);
        Task<bool> DeleteAsync(int id);
        Task<Payment?> GetByIdAsync(int id);
    }
}
