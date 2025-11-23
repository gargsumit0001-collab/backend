namespace PaymentApi.Services
{
    public class DemoPaymentGateway : IDemoPaymentGateway
    {
        public Task<PaymentResult> ChargeAsync(PaymentRequest request)
        {
            var txn = Guid.NewGuid().ToString();
            return Task.FromResult(new PaymentResult(true, txn, "Demo gateway approved"));
        }
    }
}
