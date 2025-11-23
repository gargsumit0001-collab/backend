using System.Threading.Tasks;
using Moq;
using PaymentApi.Repositories;
using PaymentApi.Services;
using Xunit;

namespace PaymentApi.Tests.Services
{
    public class DailySequentialReferenceGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_UsesDateAndReturnsFormattedString()
        {
            var repo = new Mock<IPaymentRepository>();
            repo.Setup(r => r.IncrementReferenceCounterAsync(It.IsAny<string>()))
                .ReturnsAsync(42);

            var gen = new DailySequentialReferenceGenerator(repo.Object);

            var reference = await gen.GenerateAsync();

            Assert.StartsWith("PAY-", reference);
            Assert.EndsWith("-0042", reference);
        }
    }
}
