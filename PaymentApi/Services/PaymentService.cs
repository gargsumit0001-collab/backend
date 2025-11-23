using PaymentApi.Models;
using PaymentApi.Repositories;

namespace PaymentApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IReferenceGenerator _refGen;
        private readonly IPaymentGatewayFactory _gatewayFactory;

        private static readonly string[] AllowedCurrencies = new[] { "USD", "EUR", "INR", "GBP" };

        public PaymentService(IPaymentRepository repo, IUnitOfWork uow, IReferenceGenerator refGen, IPaymentGatewayFactory gatewayFactory)
        {
            _repo = repo;
            _uow = uow;
            _refGen = refGen;
            _gatewayFactory = gatewayFactory;
        }

        public async Task<Payment> CreateAsync(decimal amount, string currency, string clientRequestId, string? provider = null)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be > 0");
            if (!AllowedCurrencies.Contains(currency)) throw new ArgumentException("Unsupported currency");

            var existing = await _repo.GetByClientRequestIdAsync(clientRequestId);
            if (existing != null) return existing;

            var reference = await _refGen.GenerateAsync();

            var gateway = _gatewayFactory.GetGateway(provider);
            var result = await gateway.ChargeAsync(new PaymentRequest(amount, currency, clientRequestId));

            var payment = new Payment
            {
                Amount = amount,
                Currency = currency,
                ClientRequestId = clientRequestId,
                Reference = reference,
                Provider = provider ?? "demo",
                TransactionId = result.TransactionId,
                Status = result.Success ? "Success" : "Failed",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(payment);
            await _uow.CommitAsync();

            return payment;
        }

        public Task<List<Payment>> ListAsync() => _repo.ListAsync();
        public Task<Payment?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<Payment?> UpdateAsync(int id, decimal amount, string currency)
        {
            var pay = await _repo.GetByIdAsync(id);
            if (pay == null) return null;
            if (amount <= 0) throw new ArgumentException("Amount must be > 0");
            if (!AllowedCurrencies.Contains(currency)) throw new ArgumentException("Unsupported currency");
            pay.Amount = amount;
            pay.Currency = currency;
            pay.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(pay);
            await _uow.CommitAsync();
            return pay;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pay = await _repo.GetByIdAsync(id);
            if (pay == null) return false;
            await _repo.DeleteAsync(pay);
            await _uow.CommitAsync();
            return true;
        }
    }
}
