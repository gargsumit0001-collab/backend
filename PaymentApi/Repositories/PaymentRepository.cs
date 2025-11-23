using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Models;

namespace PaymentApi.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentsDbContext _db;
        public PaymentRepository(PaymentsDbContext db) { _db = db; }

        public async Task<Payment?> GetByClientRequestIdAsync(string clientRequestId)
            => await _db.Payments.FirstOrDefaultAsync(p => p.ClientRequestId == clientRequestId);

        public async Task<Payment?> GetByIdAsync(int id) => await _db.Payments.FindAsync(id);

        public async Task<List<Payment>> ListAsync() => await _db.Payments.OrderByDescending(p => p.CreatedAt).ToListAsync();

        public async Task<Payment> AddAsync(Payment payment)
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(Payment payment)
        {
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Payment payment)
        {
            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();
        }

        public async Task<int> IncrementReferenceCounterAsync(string dateKey)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            var counter = await _db.ReferenceCounters.FindAsync(dateKey);
            if (counter == null)
            {
                counter = new ReferenceCounter { DateKey = dateKey, Counter = 1 };
                _db.ReferenceCounters.Add(counter);
            }
            else
            {
                counter.Counter += 1;
                _db.ReferenceCounters.Update(counter);
            }
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return counter.Counter;
        }
    }
}
