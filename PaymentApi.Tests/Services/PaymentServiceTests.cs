using System;
using System.Threading.Tasks;
using Moq;
using PaymentApi.Models;
using PaymentApi.Repositories;
using PaymentApi.Services;
using Xunit;

namespace PaymentApi.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IReferenceGenerator> _refGen = new();
        private readonly Mock<IPaymentGatewayFactory> _gatewayFactory = new();
        private readonly Mock<IPaymentGateway> _gateway = new();

        private PaymentService CreateService()
        {
            _gatewayFactory
                .Setup(f => f.GetGateway(It.IsAny<string>()))
                .Returns(_gateway.Object);

            return new PaymentService(_repo.Object, _uow.Object, _refGen.Object, _gatewayFactory.Object);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenAmountIsNotPositive()
        {
            var svc = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.CreateAsync(0m, "USD", "client-1", "demo"));
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenCurrencyNotAllowed()
        {
            var svc = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.CreateAsync(10m, "ABC", "client-1", "demo"));
        }

        [Fact]
        public async Task CreateAsync_ReturnsExisting_WhenClientRequestIdExists()
        {
            var existing = new Payment { Id = 1, ClientRequestId = "client-1" };
            _repo.Setup(r => r.GetByClientRequestIdAsync("client-1")).ReturnsAsync(existing);

            var svc = CreateService();

            var result = await svc.CreateAsync(100m, "USD", "client-1", "demo");

            Assert.Same(existing, result);
            _refGen.Verify(r => r.GenerateAsync(), Times.Never);
            _uow.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((Payment?)null);

            var svc = CreateService();

            var result = await svc.DeleteAsync(123);

            Assert.False(result);
            _repo.Verify(r => r.DeleteAsync(It.IsAny<Payment>()), Times.Never);
            _uow.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_DeletesAndCommits_WhenFound()
        {
            var existing = new Payment { Id = 123 };
            _repo.Setup(r => r.GetByIdAsync(123)).ReturnsAsync(existing);

            var svc = CreateService();

            var result = await svc.DeleteAsync(123);

            Assert.True(result);
            _repo.Verify(r => r.DeleteAsync(existing), Times.Once);
            _uow.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
