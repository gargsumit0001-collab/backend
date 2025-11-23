using Microsoft.Extensions.DependencyInjection;
namespace PaymentApi.Services
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IServiceProvider _sp;
        public PaymentGatewayFactory(IServiceProvider sp) { _sp = sp; }
        public IPaymentGateway GetGateway(string? provider) => _sp.GetRequiredService<IDemoPaymentGateway>();
    }
}
