using PaymentApi.Data;

namespace PaymentApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentsDbContext _ctx;
        public UnitOfWork(PaymentsDbContext ctx) { _ctx = ctx; }
        public Task<int> CommitAsync() => _ctx.SaveChangesAsync();
    }
}
