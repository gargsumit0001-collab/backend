namespace PaymentApi.Services
{
    public record PaymentRequest(decimal Amount, string Currency, string ClientRequestId);
    public record PaymentResult(bool Success, string TransactionId, string Message);

    public interface IPaymentGateway
    {
        Task<PaymentResult> ChargeAsync(PaymentRequest request);
    }
}
