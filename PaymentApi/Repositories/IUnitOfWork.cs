namespace PaymentApi.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
    }
}
