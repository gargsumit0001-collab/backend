using PaymentApi.Repositories;
namespace PaymentApi.Services
{
    public class DailySequentialReferenceGenerator : IReferenceGenerator
    {
        private readonly IPaymentRepository _repo;
        public DailySequentialReferenceGenerator(IPaymentRepository repo) { _repo = repo; }
        public async Task<string> GenerateAsync()
        {
            var dateKey = DateTime.UtcNow.ToString("yyyyMMdd");
            var counter = await _repo.IncrementReferenceCounterAsync(dateKey);
            return $"PAY-{dateKey}-{counter.ToString("D4")}";
        }
    }
}
